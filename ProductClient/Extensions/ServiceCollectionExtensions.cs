using System;
using Microsoft.Extensions.DependencyInjection;
using ProductClient.Interfaces;


namespace ProductClient.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddProductClient(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IProductService, ProductService>();

        return serviceCollection;
    } 

}
