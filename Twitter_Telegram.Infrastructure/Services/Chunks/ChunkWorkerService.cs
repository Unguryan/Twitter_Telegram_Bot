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

        //private readonly ITelegramUserService _userService;

        private readonly IApiReader _apiReader;

        public ChunkWorkerService(
                                  INotifySubscriptionService notifySubscriptionService,
                                  //ITelegramUserService userService,
                                  IApiReader apiReader)
        {
            _notifySubscriptionService = notifySubscriptionService;
            //_userService = userService;
            _apiReader = apiReader;
        }

        //public async Task<List<Subscription>?> CheckChunkV2(ChunkViewModel chunk)
        //{
        //    var updatedV2 = new List<Subscription>();

        //    foreach (var sub in chunk.Subscriptions)
        //    {
        //        var updatedSubFriends = await _apiReader.GetUserFriendIdsByUsernameAsync(sub.Username);


        //        if (updatedSubFriends == null)
        //        {
        //            continue;
        //        }

        //        var updatedSub = new Subscription()
        //        {
        //            Username = sub.Username,
        //            Friends = updatedSubFriends,
        //            LastTimeChecked = sub.LastTimeChecked
        //        };

        //        if (sub?.Friends == null || sub.LastTimeChecked == null)
        //        {
        //            updatedV2.Add(updatedSub);
        //            continue;
        //        }

        //        var newSubs = await CheckSubscription(sub.Friends, updatedSubFriends);
        //        updatedV2.Add(updatedSub);

        //        if (newSubs.Any())
        //        {
        //            await _notifySubscriptionService.NotifyUsersAsync(sub.Username, newSubs);
        //        }
        //    }

        //    return updatedV2.Any() ? updatedV2 : null;
        //}

        public async Task<CheckSubscriptionResultViewModel> CheckV2(Subscription sub)
        {
            var subInfo = await _apiReader.GetUserInfoByUsernameAsync(sub.Username);
            if(subInfo == null)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    Subscription = sub,
                    IsFound = false,
                    IsChecked = true
                };
            }

            if (!sub.IsInit)
            {
                return await InitSubcription(sub, subInfo);
            }

            if(sub.FriendsCount == subInfo.FriendsCount)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsFound = true,
                    IsChecked = true,
                    IsUpdated = false,
                    Subscription = sub
                };
            }

            var updatedSubFriends = await _apiReader.GetUserFriendIdsByUsernameAsync(sub.Username, subInfo.FriendsCount);

            if(updatedSubFriends == null)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsFound = true,
                    IsChecked = false,
                    IsUpdated = false,
                    Subscription = sub
                };
            }

            var newSubs = await CheckSubscription(sub.Friends, updatedSubFriends);
            if (newSubs.Any())
            {
                await _notifySubscriptionService.NotifyUsersAsync(sub.Username, newSubs);
            }

            sub.Friends = updatedSubFriends;
            sub.FriendsCount = subInfo.FriendsCount;

            return new CheckSubscriptionResultViewModel()
            {
                IsFound = true,
                IsChecked = true,
                IsUpdated = true,
                Subscription = sub
            };
        }

        private async Task<CheckSubscriptionResultViewModel> InitSubcription(Subscription sub, TwitterUser subInfo)
        {
            var updatedSubFriends = await _apiReader.GetUserFriendIdsByUsernameAsync(sub.Username, subInfo.FriendsCount);

            if (updatedSubFriends == null)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsFound = true,
                    IsChecked = false,
                    IsUpdated = false,
                    Subscription = sub
                };
            }

            sub.Friends = updatedSubFriends;
            sub.FriendsCount = subInfo.FriendsCount;

            return new CheckSubscriptionResultViewModel()
            {
                IsFound = true,
                IsChecked = true,
                IsUpdated = true,
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
