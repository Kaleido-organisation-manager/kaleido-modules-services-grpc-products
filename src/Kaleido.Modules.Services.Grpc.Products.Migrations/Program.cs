using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Modules.Services.Grpc.Products.Common.Models;
using Kaleido.Modules.Services.Grpc.Products.Common.Configuration;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    config.AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);
    config.AddEnvironmentVariables();
});

builder.ConfigureServices((hostContext, services) =>
{
    var productsConnectionString = hostContext.Configuration.GetConnectionString("Products");
    if (string.IsNullOrEmpty(productsConnectionString))
    {
        throw new ArgumentNullException(nameof(productsConnectionString), "Expected a value for the products db connection string");
    }
    var assemblyName = "Kaleido.Modules.Services.Grpc.Products.Migrations";
    services.AddKaleidoMigrationEntityDbContext<ProductEntity, ProductEntityDbContext>(productsConnectionString, assemblyName);
    services.AddKaleidoMigrationRevisionDbContext<ProductRevisionEntity, ProductRevisionDbContext>(productsConnectionString, assemblyName);
    services.AddKaleidoMigrationEntityDbContext<ProductPriceEntity, ProductPriceEntityDbContext>(productsConnectionString, assemblyName);
    services.AddKaleidoMigrationRevisionDbContext<ProductPriceRevisionEntity, ProductPriceRevisionDbContext>(productsConnectionString, assemblyName);

});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var productEntityContext = services.GetRequiredService<ProductEntityDbContext>();
    var productRevisionContext = services.GetRequiredService<ProductRevisionDbContext>();
    var productPriceEntityContext = services.GetRequiredService<ProductPriceEntityDbContext>();
    var productPriceRevisionContext = services.GetRequiredService<ProductPriceRevisionDbContext>();

    await productEntityContext.Database.MigrateAsync();
    await productRevisionContext.Database.MigrateAsync();
    await productPriceEntityContext.Database.MigrateAsync();
    await productPriceRevisionContext.Database.MigrateAsync();

    Console.WriteLine("Migration completed successfully.");
}
