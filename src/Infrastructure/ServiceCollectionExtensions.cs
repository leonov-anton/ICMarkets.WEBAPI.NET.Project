using System;
using System.Net.Http;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Infrastructure.BlockCypher;
using ICMarkets.Infrastructure.Exceptions;
using ICMarkets.Infrastructure.EF;
using ICMarkets.Infrastructure.Jobs;
using ICMarkets.Infrastructure.Options;
using ICMarkets.Infrastructure.RateLimiting;
using ICMarkets.Infrastructure.ReferenceData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestEase;
using StackExchange.Redis;

namespace ICMarkets.Infrastructure;

public static class ServiceCollectionExtensions
{
    private const string DataProviderSectionName = "dataProvider";
    private const string DatabaseConnectionName = "Blockchain";
    private const string RedisConnectionName = "Redis";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Hosted services start in registration order: reference data must be loaded before the jobs run.
        return services
            .AddDataProvider(config)
            .AddDatabase(config)
            .AddRedis(config)
            .AddReferenceData()
            .AddJobs();
    }

    private static IServiceCollection AddDataProvider(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection(DataProviderSectionName);
        var options = section.Get<DataProviderOptions>();

        if (string.IsNullOrWhiteSpace(options?.BaseUrl))
            throw new ConfigurationValueNotProvidedException(DataProviderSectionName, nameof(DataProviderOptions.BaseUrl));

        services.Configure<DataProviderOptions>(section);
        services.AddSingleton<IRateLimiterHub, RateLimiterHub>();

        services.AddHttpClient(nameof(IBlockCypherApi), client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddTransient(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(IBlockCypherApi));

            return new RestClient(httpClient)
            {
                ResponseDeserializer = new SystemTextJsonResponseDeserializer()
            }.For<IBlockCypherApi>();
        });

        services.AddTransient<IBlockchainClient, BlockCypherBlockchainClient>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = GetRequiredConnectionString(config, DatabaseConnectionName);

        services.AddDbContext<BlockchainDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        if (config.GetValue<bool>("Database:MigrateOnStartup"))
            services.AddHostedService<DatabaseMigrator>();

        return services;
    }

    private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration config)
    {
        var redisOptions = ConfigurationOptions.Parse(GetRequiredConnectionString(config, RedisConnectionName));
        // Keep reconnecting in the background instead of failing the host when Redis is down at startup.
        redisOptions.AbortOnConnectFail = false;

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));

        return services;
    }

    private static IServiceCollection AddReferenceData(this IServiceCollection services)
    {
        // One instance for both: the loader needs the concrete type to call LoadAsync.
        services.AddSingleton<ReferenceDataCache>();
        services.AddSingleton<IReferenceDataCache>(sp => sp.GetRequiredService<ReferenceDataCache>());
        services.AddHostedService<ReferenceDataLoader>();

        return services;
    }

    private static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddHostedService<BlockCypherDataGetterJob>();

        return services;
    }

    private static string GetRequiredConnectionString(IConfiguration config, string name)
    {
        var connectionString = config.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ConfigurationValueNotProvidedException("ConnectionStrings", name);

        return connectionString;
    }
}
