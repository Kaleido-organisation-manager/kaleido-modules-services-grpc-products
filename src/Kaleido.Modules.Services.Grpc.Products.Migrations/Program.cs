using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Kaleido.Modules.Services.Grpc.Products.Common.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration((hostingContext, config) =>
{
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    config.AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true);
    config.AddEnvironmentVariables();
});

builder.ConfigureServices((hostContext, services) =>
{
    services.AddDbContext<ProductsDbContext>(options =>
        options.UseNpgsql(hostContext.Configuration.GetConnectionString("Products"),
            b => b.MigrationsAssembly("Kaleido.Modules.Services.Grpc.Products.Migrations")));
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var process = Process.Start("dotnet", "ef migrations add Products");
        await process.WaitForExitAsync();

        var context = services.GetRequiredService<ProductsDbContext>();

        await context.Database.MigrateAsync();

        Console.WriteLine("Migration completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
    }
}
