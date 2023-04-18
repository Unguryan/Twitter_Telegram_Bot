using Twitter_Telegram.App.Services.Chucks;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.Infrastructure.Services.Chunks
{
    public class ChunkFactory : IChunkFactory
    {
        public Task<List<ChunkViewModel>> CreateChunksAsync(List<Subscription> subs)
        {
            var chunksStr = subs.Chunk(15).ToList();

            var res = new List<ChunkViewModel>();

            chunksStr.ForEach(c => res.Add(new ChunkViewModel()
            {
                Subscriptions = c.ToList()
            }));

            return Task.FromResult(res);
        }
    }
}
