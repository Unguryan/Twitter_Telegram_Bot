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
        private readonly IApiReader _apiReader;

        public ChunkWorkerService(
                                  INotifySubscriptionService notifySubscriptionService,
                                  IApiReader apiReader)
        {
            _notifySubscriptionService = notifySubscriptionService;
            _apiReader = apiReader;
        }

        public async Task<CheckSubscriptionResultViewModel> CheckV2(Subscription sub)
        {
            var subInfo = await _apiReader.GetUserInfoByUsernameAsync(sub.Username);

            if (subInfo.IsOut)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsOut = true,
                    Subscription = sub,
                    IsFound = false,
                    IsChecked = false,
                };
            }

            if (!subInfo.IsFound && !subInfo.IsOut)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsOut = false,
                    Subscription = sub,
                    IsFound = false,
                    IsChecked = true
                };
            }

            if (!sub.IsInit)
            {
                return await InitSubcription(sub, subInfo.TwitterUser);
            }

            if(sub.FriendsCount == subInfo.TwitterUser.FriendsCount)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsFound = true,
                    IsChecked = true,
                    IsUpdated = false,
                    IsOut = false,
                    Subscription = sub
                };
            }

            var updatedSubFriends = await _apiReader.GetUserFriendIdsByUsernameAsync(sub.Username, subInfo.TwitterUser.FriendsCount);

            if (updatedSubFriends.IsOut)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsOut = true,
                    IsFound = false,
                    IsChecked = false,
                    IsUpdated = false,
                    Subscription = sub
                };
            }

            if (!updatedSubFriends.IsFound && !updatedSubFriends.IsOut)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsFound = true,
                    IsChecked = false,
                    IsUpdated = false,
                    IsOut = false,
                    Subscription = sub
                };
            }

            var newSubs = await CheckSubscription(sub.Friends, updatedSubFriends.FriendIds);
            if (newSubs.Any())
            {
                await _notifySubscriptionService.NotifyUsersAsync(sub.Username, newSubs);
            }

            sub.Friends = updatedSubFriends.FriendIds;
            sub.FriendsCount = subInfo.TwitterUser.FriendsCount;

            return new CheckSubscriptionResultViewModel()
            {
                IsFound = true,
                IsChecked = true,
                IsUpdated = true,
                IsOut = false,
                Subscription = sub
            };
        }

        private async Task<CheckSubscriptionResultViewModel> InitSubcription(Subscription sub, TwitterUser subInfo)
        {
            var updatedSubFriends = await _apiReader.GetUserFriendIdsByUsernameAsync(sub.Username, subInfo.FriendsCount);

            if (updatedSubFriends.IsOut)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsOut = true,
                    IsFound = false,
                    IsChecked = false,
                    IsUpdated = false,
                    Subscription = sub
                };
            }

            if (!updatedSubFriends.IsFound && !updatedSubFriends.IsOut)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsFound = true,
                    IsChecked = false,
                    IsUpdated = false,
                    IsOut = false,
                    Subscription = sub
                };
            }

            sub.Friends = updatedSubFriends.FriendIds;
            sub.FriendsCount = subInfo.FriendsCount;

            return new CheckSubscriptionResultViewModel()
            {
                IsFound = true,
                IsChecked = true,
                IsUpdated = true,
                IsOut = false,
                Subscription = sub
            };
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
