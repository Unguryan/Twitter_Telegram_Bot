// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Twitter_Telegram.EF_Core;
using Twitter_Telegram.EF_Core.Context;
using Twitter_Telegram.Infrastructure;
using Twitter_Telegram.Telegram;

var builder = Host.CreateDefaultBuilder(args);

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

builder.ConfigureServices(services =>
{
    services.AddEF_Core(config);
    services.AddInfrastructure(config);
    services.AddTelegram(config);
});

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    using (var db = scope.ServiceProvider.GetRequiredService<TwitterContext>())
    {
        await db.Database.EnsureCreatedAsync();
    }
}

await host.RunAsync();