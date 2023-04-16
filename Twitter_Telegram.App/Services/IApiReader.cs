using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.App.Services
{
    public interface IApiReader
    {
        Task<List<long>?> GetUserFriendsByUsernameAsync(string username);

        Task<List<long>?> GetUserFriendIdsByUsernameAsync(string username);

        Task<TwitterUser?> GetUserInfoByUserIdAsync(string userId);

        Task<TwitterUser?> GetUserInfoByUsernameAsync(string username);
    }
}
