using Twitter_Telegram.Domain.Models;

namespace Core.Services.Interfaces
{
    public interface IApiReader
    {
        Task<string> GetUserDataAsync(string username);

        Task<List<int>> GetUserFriendIdsAsync(string username);

        Task<TwitterUser> GetUserInfoAsync(string UserId);
    }
}
