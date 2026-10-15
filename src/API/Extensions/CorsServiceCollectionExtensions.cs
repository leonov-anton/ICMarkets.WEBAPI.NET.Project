using ICMarkets.API.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ICMarkets.API.Extensions;

public static class CorsServiceCollectionExtensions
{
    /// <summary>Registers the default CORS policy built from the "Cors" configuration section.</summary>
    public static IServiceCollection AddCorsFromConfiguration(this IServiceCollection services, IConfiguration config)
    {
        var settings = config.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(settings.AllowedOrigins)
                      .WithMethods(settings.AllowedMethods)
                      .WithHeaders(settings.AllowedHeaders)
                      .WithExposedHeaders(settings.ExposedHeaders);
            });
        });

        return services;
    }
}
