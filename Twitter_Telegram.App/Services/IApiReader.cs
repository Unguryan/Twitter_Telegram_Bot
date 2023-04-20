using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.App.Services
{
    public interface IApiReader
    {

        Task<GetUserFriendIdsResultViewModel> GetUserFriendIdsByUsernameAsync(string username, int count);

        Task<GetUserInfoResultViewModel> GetUserInfoByUserIdAsync(string userId);

        Task<GetUserInfoResultViewModel> GetUserInfoByUsernameAsync(string username);
    }
}
