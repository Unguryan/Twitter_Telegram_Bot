﻿using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.App.Services.Chucks
{
    public interface IChunkWorkerService
    {
        //Task<List<Subscription>?> CheckChunkV2(ChunkViewModel chunk);

        //Task<CheckSubscriptionResultViewModel?> Check(Subscription sub);
        Task<List<CheckSubscriptionResultViewModel>?> CheckV2(List<Subscription> subs, GetUsersInfoResultViewModel users);
    }
}
