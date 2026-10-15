using System.Threading;
using System.Threading.Tasks;
using ICMarkets.Application.Abstractions.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ICMarkets.Infrastructure.ReferenceData;

public class ReferenceDataLoader : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ReferenceDataCache _cache;

    public ReferenceDataLoader(IServiceScopeFactory scopeFactory, ReferenceDataCache cache)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await _cache.LoadAsync(unitOfWork, ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
