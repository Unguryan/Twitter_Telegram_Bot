using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Twitter_Telegram.Domain.Config;

namespace Twitter_Telegram.App
{
    public static class DI
    {
        public static void AddAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            var assebmly = Assembly.GetExecutingAssembly();
            services.AddMediatR(assebmly);

            var telegramOption = configuration.GetSection(TelegramOptions.SectionName);
            services.AddOptions<TelegramOptions>()
                    .Bind(telegramOption);

            var twitterOption = configuration.GetSection(TwitterOptions.SectionName);
            services.AddOptions<TwitterOptions>()
                    .Bind(twitterOption);
        }
    }
}
