using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using ProductClient.Interfaces;


namespace ProductClient.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddProductClient(this IServiceCollection serviceCollection)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        serviceCollection.AddTransient<IProductService, ProductService>();

        return serviceCollection;
    }

}
