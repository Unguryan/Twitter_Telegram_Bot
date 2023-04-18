using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Chucks;
using Twitter_Telegram.App.Services.Telegram;
using Twitter_Telegram.Domain.Models;

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
                var timeout = TimeSpan.FromSeconds(10);
                await Task.Delay(timeout, stoppingToken);

                var subs = new List<Subscription>();

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                        var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                        var apiReader = scope.ServiceProvider.GetRequiredService<IApiReader>();
                        subs = await subscriptionService.GetSubscriptionsAsync();
                        foreach (var sub in subs)
                        {
                            var twitterUser = await apiReader.GetUserInfoByUsernameAsync(sub.Username);
                            if (twitterUser == null)
                            {
                                await subscriptionService.RemoveSubscriptionAsync(sub.Username);

                                var users = await userService.GetUsersWithSubscriptionAsync(sub.Username);

                                if (users.Any())
                                {
                                    foreach (var user in users)
                                    {
                                        await userService.RemoveSubscriptionAsync(user.Id, sub.Username);
                                    }
                                }

                                continue;
                            }
                        }
                    }

                    while (subs.Any())
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var subService = scope.ServiceProvider.GetRequiredService<ISubscriptionWorkerService>();
                            var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

                            var updatedSubs = await subService.CheckSubscriptions(subs, stoppingToken);

                            foreach (var sub in updatedSubs)
                            {
                                var subToRemove = subs.First(s => s.Username == sub.Username);
                                subs.Remove(subToRemove);

                                await subscriptionService.ChangeSubscriptionsByUsernameAsync(sub.Username, sub.Friends);
                            }

                            _logger.LogInformation($"{DateTime.Now.ToShortTimeString()}:" +
                               $" Subscription worker: Checked {updatedSubs.Count} username. Subs Remain: {subs.Count}");
                        }

                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
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
