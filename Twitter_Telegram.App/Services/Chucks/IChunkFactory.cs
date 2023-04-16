using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.App.Services.Chucks
{
    public interface IChunkFactory
    {
        Task<List<ChunkViewModel>> CreateChunksAsync(List<string> usernames);
    }
}
