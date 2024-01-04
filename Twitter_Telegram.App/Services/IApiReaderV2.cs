using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.App.Services
{
    public interface IApiReaderV2
    {
        Task<GetUserFriendIdsResultViewModel> GetUserFriendIdsByUsernameAsync(string username, int count);

        Task<GetUsersInfoResultViewModel> GetUserInfoByUserIdAsync(List<string> userId);

        Task<GetUsersInfoResultViewModel> GetUserInfoByUsernameAsync(List<string> username);
    }
}
