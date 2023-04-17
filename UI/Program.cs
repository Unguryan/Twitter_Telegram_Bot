// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Twitter_Telegram.EF_Core;
using Twitter_Telegram.Infrastructure;
using Twitter_Telegram.Telegram;

var builder = Host.CreateDefaultBuilder(args);

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

builder.ConfigureServices(services =>
{
    services.AddInfrastructure(config);
    services.AddTelegram(config);
    services.AddEF_Core(config);
});

var host = builder.Build();

await host.RunAsync();