using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Chucks;

namespace Twitter_Telegram.Infrastructure.Services
{
    public class SubscriptionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public SubscriptionBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SubscriptionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var timeout = TimeSpan.FromMinutes(1);
                await Task.Delay(timeout, stoppingToken);

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var subService = scope.ServiceProvider.GetRequiredService<ISubscriptionWorkerService>();

                        var count = await subService.CheckSubscriptions(stoppingToken);
                        
                        _logger.LogInformation($"{DateTime.Now.ToShortTimeString()}:" +
                            $" Subscription worker: Checked {count} username.");
                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError("Subscription worker failed with exception: {Exception}", ex);

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
    }
}
