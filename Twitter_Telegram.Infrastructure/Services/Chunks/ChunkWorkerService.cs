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
        private readonly IApiReaderV2 _apiReader;

        public ChunkWorkerService(
                                  INotifySubscriptionService notifySubscriptionService,
                                  IApiReaderV2 apiReader)
        {
            _notifySubscriptionService = notifySubscriptionService;
            _apiReader = apiReader;
        }

        public async Task<List<CheckSubscriptionResultViewModel>?> CheckV2(List<Subscription> subs, GetUsersInfoResultViewModel subInfos)
        {
            var resultList = new List<CheckSubscriptionResultViewModel>();

            
                //if (!subInfos.IsOut)
                //{
                //    return null;
                //}

                if (subInfos.IsOut)
                {
                    return new List<CheckSubscriptionResultViewModel>() {
                        new CheckSubscriptionResultViewModel()
                        {
                            IsOut = true,
                            Subscription = null,
                            IsFound = false,
                            IsChecked = false,
                        }
                    };
                }

                foreach (var sub in subs)
                {
                    var subInfo = subInfos.TwitterUsers.FirstOrDefault(u => u.Username == sub.Username);

                    if (subInfo == null)
                    {
                        var notFound = new CheckSubscriptionResultViewModel()
                        {
                            IsOut = false,
                            Subscription = sub,
                            IsFound = false,
                            IsChecked = true
                        };

                        resultList.Add(notFound);
                        continue;
                    }

                    if (!sub.IsInit)
                    {
                        var initSub = await InitSubcription(sub, subInfo);
                        resultList.Add(initSub);
                        continue;
                    }

                    if (sub.FriendsCount == subInfo.FriendsCount)
                    {
                        var checkedUser = new CheckSubscriptionResultViewModel()
                        {
                            IsFound = true,
                            IsChecked = true,
                            IsUpdated = false,
                            IsOut = false,
                            Subscription = sub
                        };

                        resultList.Add(checkedUser);
                        continue;
                    }

                    var updatedSubFriends = await _apiReader.GetUserFriendIdsByUsernameAsync(sub.Username, subInfo.FriendsCount);

                    if (updatedSubFriends.IsOut)
                    {
                        var isOut = new CheckSubscriptionResultViewModel()
                        {
                            IsOut = true,
                            IsFound = false,
                            IsChecked = false,
                            IsUpdated = false,
                            Subscription = sub
                        };

                        resultList.Add(isOut);
                        continue;
                    }

                    if (updatedSubFriends.IsAuthError)
                    {
                        var isAuth = new CheckSubscriptionResultViewModel()
                        {
                            IsOut = false,
                            IsFound = true,
                            IsChecked = false,
                            IsUpdated = false,
                            Subscription = sub
                        };

                        resultList.Add(isAuth);
                        continue;
                    }

                    if (!updatedSubFriends.IsFound && !updatedSubFriends.IsOut)
                    {
                        var notFound = new CheckSubscriptionResultViewModel()
                        {
                            IsFound = false,
                            IsChecked = false,
                            IsUpdated = false,
                            IsOut = false,
                            Subscription = sub
                        };

                        resultList.Add(notFound);
                        continue;
                    }

                    var newSubs = await CheckSubscription(sub.Friends, updatedSubFriends.FriendIds);
                    if (newSubs.Any())
                    {
                        await _notifySubscriptionService.NotifyUsersAsync(sub.Username, newSubs);
                    }

                    sub.Friends = updatedSubFriends.FriendIds;
                    sub.FriendsCount = subInfo.FriendsCount;

                    var updated = new CheckSubscriptionResultViewModel()
                    {
                        IsFound = true,
                        IsChecked = true,
                        IsUpdated = true,
                        IsOut = false,
                        Subscription = sub
                    };

                    resultList.Add(updated);
                }

            

            return resultList;
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

            if (updatedSubFriends.IsAuthError)
            {
                return new CheckSubscriptionResultViewModel()
                {
                    IsOut = false,
                    IsFound = true,
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

        //public async Task<CheckSubscriptionResultViewModel?> Check(Subscription sub)
        //{
        //    var subInfo = await _apiReader.GetUserInfoByUsernameAsync(sub.Username);

        //    if (subInfo.IsOut)
        //    {
        //        return new CheckSubscriptionResultViewModel()
        //        {
        //            IsOut = true,
        //            Subscription = sub,
        //            IsFound = false,
        //            IsChecked = false,
        //        };
        //    }

        //    if (!subInfo.IsFound && !subInfo.IsOut)
        //    {
        //        return new CheckSubscriptionResultViewModel()
        //        {
        //            IsOut = false,
        //            Subscription = sub,
        //            IsFound = false,
        //            IsChecked = true
        //        };
        //    }

        //    if (!sub.IsInit)
        //    {
        //        return await InitSubcription(sub, subInfo.TwitterUser);
        //    }

        //    if (sub.FriendsCount == subInfo.TwitterUser.FriendsCount)
        //    {
        //        return new CheckSubscriptionResultViewModel()
        //        {
        //            IsFound = true,
        //            IsChecked = true,
        //            IsUpdated = false,
        //            IsOut = false,
        //            Subscription = sub
        //        };
        //    }

        //    var updatedSubFriends = await _apiReader.GetUserFriendIdsByUsernameAsync(sub.Username, subInfo.TwitterUser.FriendsCount);

        //    if (updatedSubFriends.IsOut)
        //    {
        //        return new CheckSubscriptionResultViewModel()
        //        {
        //            IsOut = true,
        //            IsFound = false,
        //            IsChecked = false,
        //            IsUpdated = false,
        //            Subscription = sub
        //        };
        //    }

        //    if (!updatedSubFriends.IsFound && !updatedSubFriends.IsOut)
        //    {
        //        return new CheckSubscriptionResultViewModel()
        //        {
        //            IsFound = true,
        //            IsChecked = false,
        //            IsUpdated = false,
        //            IsOut = false,
        //            Subscription = sub
        //        };
        //    }

        //    var newSubs = await CheckSubscription(sub.Friends, updatedSubFriends.FriendIds);
        //    if (newSubs.Any())
        //    {
        //        await _notifySubscriptionService.NotifyUsersAsync(sub.Username, newSubs);
        //    }

        //    sub.Friends = updatedSubFriends.FriendIds;
        //    sub.FriendsCount = subInfo.TwitterUser.FriendsCount;

        //    return new CheckSubscriptionResultViewModel()
        //    {
        //        IsFound = true,
        //        IsChecked = true,
        //        IsUpdated = true,
        //        IsOut = false,
        //        Subscription = sub
        //    };
        //}
    }
}
