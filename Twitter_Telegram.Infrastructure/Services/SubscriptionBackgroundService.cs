using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Chucks;
using Twitter_Telegram.App.Services.Telegram;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.Domain.ViewModels;

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
                var twitterUsers = new List<GetUsersInfoResultViewModel>();

                try
                {
                    var isTimeOut = false;

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                        var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                        var apiReader = scope.ServiceProvider.GetRequiredService<IApiReaderV2>();
                        var notifyService = scope.ServiceProvider.GetRequiredService<INotifySubscriptionService>();

                        subs = await subscriptionService.GetSubscriptionsAsync();
                        //test(subs);

                        if(subs.Count == 0)
                        {
                            continue;
                        }

                        twitterUsers = await apiReader.GetUserInfoByUsernameAsync(subs.Select(u => u.Username).ToList());

                        foreach (var res in twitterUsers)
                        {
                            if (res.IsOut)
                            {
                                isTimeOut = true;
                                break;
                            }

                            foreach (var sub in subs)
                            {
                                var twitterUser = res.TwitterUsers.FirstOrDefault(u => u.Username == sub.Username);

                                if (twitterUser == null)
                                {
                                    await subscriptionService.RemoveSubscriptionAsync(sub.Username);

                                    var users = await userService.GetUsersWithSubscriptionAsync(sub.Username);

                                    if (users.Any())
                                    {
                                        foreach (var user in users)
                                        {
                                            await userService.RemoveSubscriptionAsync(user.Id, sub.Username);
                                            await notifyService.SubscriptionRemovedAsync(user.Id, sub.Username);
                                        }
                                    }

                                    continue;
                                }
                            }
                        }
                    }

                    while (subs.Any())
                    {
                        if (isTimeOut)
                        {
                            _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}: TIMEOUT!!!");
                            _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}: TIMEOUT!!!");
                            _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}: TIMEOUT!!!");
                            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                            isTimeOut = false;
                        }

                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var subService = scope.ServiceProvider.GetRequiredService<ISubscriptionWorkerService>();
                            var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                            var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                            var notifyService = scope.ServiceProvider.GetRequiredService<INotifySubscriptionService>();

                            var updatedSubs = await subService.CheckSubscriptions(subs, twitterUsers, stoppingToken);

                            if(updatedSubs == null)
                            {
                                _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}: TimeOut!");
                                isTimeOut = true;
                                continue;
                            }
                            else
                            {
                                foreach (var sub in updatedSubs)
                                {
                                    if (sub.IsOut)
                                    {
                                        _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}: TimeOut: {updatedSubs.Count}, {sub.Subscription.Username}");
                                        isTimeOut = true;
                                        continue;
                                    }

                                    if (!sub.IsFound)
                                    {
                                        var subToRemove = subs.First(s => s.Username == sub.Subscription.Username);
                                        subs.Remove(subToRemove);
                                        //await subscriptionService.RemoveSubscriptionAsync(sub.Subscription.Username);

                                        //var users = await userService.GetUsersWithSubscriptionAsync(sub.Subscription.Username);

                                        //if (users.Any())
                                        //{
                                        //    foreach (var user in users)
                                        //    {
                                        //        await userService.RemoveSubscriptionAsync(user.Id, sub.Subscription.Username);
                                        //        await notifyService.SubscriptionRemovedAsync(user.Id, sub.Subscription.Username);
                                        //    }
                                        //}

                                        _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}: Not found: {updatedSubs.Count}, {sub.Subscription.Username}");

                                        continue;
                                    }

                                    if (sub.IsFound && !sub.IsChecked)
                                    {
                                        var subToRemove = subs.First(s => s.Username == sub.Subscription.Username);
                                        subs.Remove(subToRemove);
                                        //await subscriptionService.RemoveSubscriptionAsync(sub.Subscription.Username);

                                        //var users = await userService.GetUsersWithSubscriptionAsync(sub.Subscription.Username);

                                        //if (users.Any())
                                        //{
                                        //    foreach (var user in users)
                                        //    {
                                        //        await userService.RemoveSubscriptionAsync(user.Id, sub.Subscription.Username);
                                        //        await notifyService.SubscriptionRemovedAsync(user.Id, sub.Subscription.Username);
                                        //    }
                                        //}

                                        _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}: AUTH ERROR: {updatedSubs.Count}, {sub.Subscription.Username}");

                                        continue;
                                    }

                                    if (sub.IsChecked)
                                    {
                                        var subToRemove = subs.FirstOrDefault(s => s.Username == sub.Subscription.Username);
                                        
                                        if(subToRemove != null)
                                        {
                                            subs.Remove(subToRemove);
                                            await subscriptionService.ChangeSubscriptionLastTimeCheckAsync(sub.Subscription.Username);
                                            _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}:" + $"Checked: {sub.Subscription.Username},Friends: {sub.Subscription.FriendsCount}");

                                        }
                                    }

                                    if (sub.IsUpdated)
                                    {
                                        await subscriptionService.ChangeSubscriptionsByUsernameAsync(
                                            sub.Subscription.Username,
                                            sub.Subscription.FriendsCount.Value,
                                            sub.Subscription.Friends);

                                        _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}:" + $"Updated: {sub.Subscription.Username},Friends: {sub.Subscription.FriendsCount}");
                                    }
                                }

                                _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}:" +
                                   $" Subscription worker: Checked {updatedSubs.Count} username. Subs Remain: {subs.Count}. TIME OUT FOR 15 MIN.");
                                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                            }
                        }
                            

                        //await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError("Subscription worker failed with exception: {Exception}", ex);

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        //private void test(List<Subscription> subs)
        //{
        //    var sb = new StringBuilder();

        //    foreach (var sub in subs)
        //    {
        //        sb.AppendLine($"\t\t\t\t\"{sub.Username}\",");
        //    }

        //    var res = sb.ToString();
        //}
    }
}
