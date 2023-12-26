using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.App.Services
{
    public interface IApiReaderV2
    {
        Task<GetUserFriendIdsResultViewModel> GetUserFriendIdsByUsernameAsync(string username, int count);

        Task<List<GetUsersInfoResultViewModel>> GetUserInfoByUserIdAsync(List<string> userId);

        Task<List<GetUsersInfoResultViewModel>> GetUserInfoByUsernameAsync(List<string> username);
    }
}
