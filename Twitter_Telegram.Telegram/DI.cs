using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Twitter_Telegram.App.Services.Telegram;
using Twitter_Telegram.Domain.Config;
using Twitter_Telegram.Telegram.Services;

namespace Twitter_Telegram.Telegram
{
    public static class DI
    {
        public static void AddTelegram(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("telegram_bot_client")
               .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
               {
                   TelegramOptions? botConfig = sp.GetConfiguration<TelegramOptions>();
                   TelegramBotClientOptions options = new(botConfig.BotToken);
                   return new TelegramBotClient(options, httpClient);
               });

            services.AddScoped<UpdateHandler>();
            services.AddScoped<ReceiverService>();
            services.AddHostedService<PollingService>();

            services.AddScoped<IMessageReader, MessageReader>();
        }
    }
}
