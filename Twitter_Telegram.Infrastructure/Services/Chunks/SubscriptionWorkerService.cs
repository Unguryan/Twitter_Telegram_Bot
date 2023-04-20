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

        public async Task<List<CheckSubscriptionResultViewModel>> CheckSubscriptions(List<Subscription> subs, CancellationToken cancellationToken)
        {
            var updatedSubs = new List<CheckSubscriptionResultViewModel>();
                
            foreach (var subscription in subs)
            {
                var resVM = await _chunkWorkerService.CheckV2(subscription);

                updatedSubs.Add(resVM);

                //if (resVM.IsChecked)
                //{
                //    updatedSubs.Add(resVM);
                //}
            }

            //var chunks = await _chunkFactory.CreateChunksAsync(subs);

            //    for (int i = 0; i < chunks.Count; i++)
            //    {
            //        var firstChuck = chunks[i];
            //        var updatedV1 = await _chunkWorkerService.CheckChunkV2(firstChuck);

            //        if (updatedV1 != null && updatedV1.Any())
            //        {
            //            foreach (var item in updatedV1)
            //            {
            //                updatedSubs.Add(item);
            //            }
            //        }
            //    }

            return updatedSubs;
        }
    }
}
