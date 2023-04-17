using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Twitter_Telegram.App;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Chucks;
using Twitter_Telegram.App.Services.Telegram;
using Twitter_Telegram.Infrastructure.Services;
using Twitter_Telegram.Infrastructure.Services.Chunks;
using Twitter_Telegram.Infrastructure.Services.Telegram;

namespace Twitter_Telegram.Infrastructure
{
    public static class DI
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAssembly(configuration);

            services.AddScoped<IApiReader, ApiReader>();

            services.AddScoped<IChunkFactory, ChunkFactory>();
            services.AddScoped<IChunkWorkerService, ChunkWorkerService>();
            services.AddScoped<ISubscriptionWorkerService, SubscriptionWorkerService>();

            services.AddScoped<IApiReader, ApiReader>();
            services.AddScoped<ITelegramUserService, TelegramUserService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<ISubscriptionFacade, SubscriptionFacade>();

            services.AddHostedService<SubscriptionBackgroundService>();
        }
    }
}
