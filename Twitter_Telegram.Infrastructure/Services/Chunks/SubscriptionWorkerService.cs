using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Chucks;
using Twitter_Telegram.Domain.Models;

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

        public async Task<List<Subscription>> CheckSubscriptions(List<Subscription> subs, CancellationToken cancellationToken)
        {
            var updatedSubs = new List<Subscription>();
                var chunks = await _chunkFactory.CreateChunksAsync(subs);

                for (int i = 0; i < chunks.Count; i++)
                {
                    var firstChuck = chunks[i];
                    var updatedV1 = await _chunkWorkerService.CheckChunkV2(firstChuck);

                    if (updatedV1 != null && updatedV1.Any())
                    {
                        foreach (var item in updatedV1)
                        {
                            updatedSubs.Add(item);
                        }
                    }
                }

            return updatedSubs;
        }
    }
}
