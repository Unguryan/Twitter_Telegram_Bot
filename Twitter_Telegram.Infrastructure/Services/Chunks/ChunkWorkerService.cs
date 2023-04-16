using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Chucks;
using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.Infrastructure.Services.Chunks
{
    public class ChunkWorkerService : IChunkWorkerService
    {
        private readonly ISubscriptionService _subscriptionService;

        private readonly INotifySubscriptionService _notifySubscriptionService;

        private readonly IApiReader _apiReader;

        public ChunkWorkerService(ISubscriptionService subscriptionService,
                                  INotifySubscriptionService notifySubscriptionService,
                                  IApiReader apiReader)
        {
            _subscriptionService = subscriptionService;
            _apiReader = apiReader;
            _notifySubscriptionService = notifySubscriptionService;
        }

        public async Task<List<string>?> CheckChunkV1(ChunkViewModel chunk)
        {
            var outs = new List<string>();

            foreach (var userName in chunk.Usernames)
            {
                var savedSub = await _subscriptionService.GetSubscriptionsByUsernameAsync(userName);
                var updatedSubFriends = await _apiReader.GetUserFriendsByUsernameAsync(userName);

                if (updatedSubFriends == null)
                {
                    outs.Add(userName);
                    continue;
                }

                if(savedSub == null)
                {
                    savedSub = await _subscriptionService.AddSubscriptionAsync(userName);
                }

                if (savedSub?.Friends == null || !savedSub.Friends.Any())
                {
                    await _subscriptionService.ChangeSubscriptionsByUsernameAsync(userName, updatedSubFriends);
                    continue;
                }

                var newSubs = await CheckSubscription(savedSub.Friends, updatedSubFriends);
                await _subscriptionService.ChangeSubscriptionsByUsernameAsync(userName, updatedSubFriends);

                if (newSubs.Any())
                {
                    await _notifySubscriptionService.NotifyUsersAsync(userName, newSubs);
                }
            }

            return outs.Any() ? outs : null;
        }

        public async Task<List<string>?> CheckChunkV2(ChunkViewModel chunk)
        {
            var outs = new List<string>();

            foreach (var userName in chunk.Usernames)
            {
                var savedSub = await _subscriptionService.GetSubscriptionsByUsernameAsync(userName);
                var updatedSubFriends = await _apiReader.GetUserFriendIdsByUsernameAsync(userName);


                if (updatedSubFriends == null)
                {
                    outs.Add(userName);
                    continue;
                }

                if (savedSub == null)
                {
                    savedSub = await _subscriptionService.AddSubscriptionAsync(userName);
                }

                if (savedSub?.Friends == null || !savedSub.Friends.Any())
                {
                    await _subscriptionService.ChangeSubscriptionsByUsernameAsync(userName, updatedSubFriends);
                    continue;
                }

                var newSubs = await CheckSubscription(savedSub.Friends, updatedSubFriends);
                await _subscriptionService.ChangeSubscriptionsByUsernameAsync(userName, updatedSubFriends);

                if (newSubs.Any())
                {
                    await _notifySubscriptionService.NotifyUsersAsync(userName, newSubs);
                }
            }

            return outs.Any() ? outs : null;
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
