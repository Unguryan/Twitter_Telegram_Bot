using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Twitter_Telegram.App;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.Infrastructure.Services;

namespace Twitter_Telegram.Infrastructure
{
    public static class DI
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAssembly(configuration);

            services.AddScoped<IApiReader, ApiReader>();
        }
    }
}
