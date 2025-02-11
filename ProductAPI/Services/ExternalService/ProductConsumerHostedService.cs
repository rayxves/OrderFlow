using Microsoft.Extensions.DependencyInjection;
using ProductAPI.Services;

public class ProductConsumerHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ProductConsumerHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var consumerService = scope.ServiceProvider.GetRequiredService<ProductConsumerService>();

        await consumerService.InitializeAsync();
        await consumerService.StartConsumingAsync();
    }
}
