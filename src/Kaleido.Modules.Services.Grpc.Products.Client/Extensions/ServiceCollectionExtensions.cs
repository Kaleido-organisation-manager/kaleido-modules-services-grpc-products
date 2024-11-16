using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using static Kaleido.Grpc.Products.GrpcProducts;

namespace Kaleido.Modules.Services.Grpc.Views.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProductsClient(this IServiceCollection services, string connectionString)
    {
        var channel = GrpcChannel.ForAddress(connectionString);
        var client = new GrpcProductsClient(channel);
        services.AddSingleton(client);
        return services;
    }
}
