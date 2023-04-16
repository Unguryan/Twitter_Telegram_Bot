using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.App.Services.Chucks
{
    public interface IChunkWorkerService
    {
        Task<List<string>?> CheckChunkV1(ChunkViewModel chunk);
        Task<List<string>?> CheckChunkV2(ChunkViewModel chunk);
    }
}
