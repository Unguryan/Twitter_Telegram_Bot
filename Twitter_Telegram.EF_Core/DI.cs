using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Twitter_Telegram.App.Repositories;
using Twitter_Telegram.EF_Core.Context;
using Twitter_Telegram.EF_Core.Repositories;

namespace Twitter_Telegram.EF_Core
{
    public static class DI
    {
        public static void AddEF_Core(this IServiceCollection services, IConfiguration configuration)
        {
            var assebmly = Assembly.GetExecutingAssembly();
            services.AddAutoMapper(assebmly);

            services.AddDbContext<TwitterContext>();

            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<ITelegramRepository, TelegramRepository>();
        }
    }
}
