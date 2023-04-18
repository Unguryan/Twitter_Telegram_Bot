using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Chucks;
using Twitter_Telegram.App.Services.Telegram;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.Infrastructure.Services.Chunks
{
    public class ChunkWorkerService : IChunkWorkerService
    {

        private readonly INotifySubscriptionService _notifySubscriptionService;

        private readonly ITelegramUserService _userService;

        private readonly IApiReader _apiReader;

        public ChunkWorkerService(
                                  INotifySubscriptionService notifySubscriptionService,
                                  ITelegramUserService userService,
                                  IApiReader apiReader)
        {
            _notifySubscriptionService = notifySubscriptionService;
            _userService = userService;
            _apiReader = apiReader;
        }

        public async Task<List<Subscription>?> CheckChunkV2(ChunkViewModel chunk)
        {
            var updatedV2 = new List<Subscription>();

            foreach (var sub in chunk.Subscriptions)
            {
                var updatedSubFriends = await _apiReader.GetUserFriendIdsByUsernameAsync(sub.Username);


                if (updatedSubFriends == null)
                {
                    continue;
                }

                var updatedSub = new Subscription()
                {
                    Username = sub.Username,
                    Friends = updatedSubFriends,
                    LastTimeChecked = sub.LastTimeChecked
                };

                if (sub?.Friends == null || sub.LastTimeChecked == null)
                {
                    updatedV2.Add(updatedSub);
                    continue;
                }

                var newSubs = await CheckSubscription(sub.Friends, updatedSubFriends);
                updatedV2.Add(updatedSub);

                if (newSubs.Any())
                {
                    await _notifySubscriptionService.NotifyUsersAsync(sub.Username, newSubs);
                }
            }

            return updatedV2.Any() ? updatedV2 : null;
        }

        private async Task<List<long>> CheckSubscription(List<long> saved, List<long> updated)
        {
            var newSubs = new List<long>();

            foreach (var item in updated)
            {
                if(!saved.Any(s => s == item))
                {
                    newSubs.Add(item);
                }
            }

            return newSubs;
        }
    }
}
