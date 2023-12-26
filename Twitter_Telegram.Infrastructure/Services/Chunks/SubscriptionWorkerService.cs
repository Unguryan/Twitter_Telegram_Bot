using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Chucks;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.Infrastructure.Services.Chunks
{
    public class SubscriptionWorkerService : ISubscriptionWorkerService
    {
        private readonly IChunkFactory _chunkFactory;
        private readonly IChunkWorkerService _chunkWorkerService;

        public SubscriptionWorkerService(IChunkFactory chunkFactory, IChunkWorkerService chunkWorkerService)
        {
            _chunkFactory = chunkFactory;
            _chunkWorkerService = chunkWorkerService;
        }

        public async Task<List<CheckSubscriptionResultViewModel>?> CheckSubscriptions(List<Subscription> subs, List<GetUsersInfoResultViewModel> users, CancellationToken cancellationToken)
        {
            //var updatedSubs = new List<CheckSubscriptionResultViewModel>();
                
            //foreach (var subscription in subs)
            //{
            //    var resVM = await _chunkWorkerService.CheckV2(subscription);

            //    if(resVM != null)
            //    {
            //        updatedSubs.Add(resVM);
            //        if (resVM.IsOut)
            //        {
            //            break;
            //        }
            //    }
            //}

            return await _chunkWorkerService.CheckV2(subs, users);
        }
    }
}
