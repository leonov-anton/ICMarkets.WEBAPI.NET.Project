using ICMarkets.Application.Queries.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ICMarkets.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration config)
    {
        services.AddMediator(options =>
        {
            // Handlers depend on the scoped IUnitOfWork.
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(GetCoinChainHistoryQueryValidator)];
        });

        return services;
    }
}
