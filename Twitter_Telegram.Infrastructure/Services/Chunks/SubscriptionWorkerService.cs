using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Chucks;

namespace Twitter_Telegram.Infrastructure.Services.Chunks
{
    public class SubscriptionWorkerService : ISubscriptionWorkerService
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IChunkFactory _chunkFactory;
        private readonly IChunkWorkerService _chunkWorkerService;

        public SubscriptionWorkerService(ISubscriptionService subscriptionService, IChunkFactory chunkFactory, IChunkWorkerService chunkWorkerService)
        {
            _subscriptionService = subscriptionService;
            _chunkFactory = chunkFactory;
            _chunkWorkerService = chunkWorkerService;
        }

        public async Task<int> CheckSubscriptions(CancellationToken cancellationToken)
        {
            //var subService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
            //var chunkFactory = scope.ServiceProvider.GetRequiredService<IChunkFactory>();
            //var chunkWorkerService = scope.ServiceProvider.GetRequiredService<IChunkWorkerService>();

            //var subs = await _subscriptionService.GetSubscriptionsAsync();
            //List<string> outSubs = new List<string>();
            //var chunks = await _chunkFactory.CreateChunksAsync(subs.Select(s => s.Username).ToList());

            //for (int i = 0; i < chunks.Count; i++)
            //{
            //    var firstChuck = chunks[i];
            //    var outsV1 = await _chunkWorkerService.CheckChunkV1(firstChuck);

            //    if (outsV1 != null && outsV1.Any())
            //    {
            //        outSubs.AddRange(outsV1);
            //    }

            //    if ((i + 1) < chunks.Count)
            //    {
            //        var secondChuck = chunks[i + 1];
            //        var outsV2 = await _chunkWorkerService.CheckChunkV2(secondChuck);

            //        if (outsV2 != null && outsV2.Any())
            //        {
            //            outSubs.AddRange(outsV2);
            //        }

            //        i++;
            //    }

            //    await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
            //}

            var subs = await _subscriptionService.GetSubscriptionsAsync();
            List<string> outSubs = subs.Select(s => s.Username).ToList();
            //var chunks = await _chunkFactory.CreateChunksAsync(subs.Select(s => s.Username).ToList());


            while (outSubs.Any())
            {
                var chunks = await _chunkFactory.CreateChunksAsync(outSubs);

                for (int i = 0; i < chunks.Count; i++)
                {
                    var firstChuck = chunks[i];
                    var outsV1 = await _chunkWorkerService.CheckChunkV1(firstChuck);

                    if (outsV1 != null && outsV1.Any())
                    {
                        foreach (var item in outsV1)
                        {
                            firstChuck.Usernames.Remove(item);
                        }
                    }

                    if (firstChuck.Usernames.Any())
                    {
                        foreach (var item in firstChuck.Usernames)
                        {
                            outSubs.Remove(item);
                        }

                    }

                    if ((i + 1) < chunks.Count)
                    {
                        var secondChuck = chunks[i + 1];
                        var outsV2 = await _chunkWorkerService.CheckChunkV2(secondChuck);

                        if (outsV2 != null && outsV2.Any())
                        {
                            foreach (var item in outsV2)
                            {
                                secondChuck.Usernames.Remove(item);
                            }
                        }

                        if (secondChuck.Usernames.Any())
                        {
                            foreach (var item in secondChuck.Usernames)
                            {
                                outSubs.Remove(item);
                            }

                        }

                        i++;
                    }

                    await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
                }

                //foreach (var chunk in subChunks)
                //{
                //    var outsV2 = await _chunkWorkerService.CheckChunkV2(chunk);

                //if (outsV2 != null && outsV2.Any())
                //{
                //    foreach (var item in outsV2)
                //    {
                //        chunk.Usernames.Remove(item);
                //    }
                //}

                //if (chunk.Usernames.Any())
                //{
                //    foreach (var item in chunk.Usernames)
                //    {
                //        outSubs.Remove(item);
                //    }

                //}
                //}
            }

            return subs.Count();
        }
    }
}
