using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.App.Repositories
{
    public interface ITelegramRepository
    {
        Task<List<TelegramUser>> GetUsersAsync();

        Task<List<TelegramUser>> GetUsersWithSubscriptionAsync(string userName);

        Task<TelegramUser?> GetUserByIdAsync(long userId);

        Task<TelegramUser?> AddUserAsync(long userId);

        Task<TelegramUser?> ChangeUserStateAsync(long userId, TelegramUserState state);

        Task<TelegramUser?> ChangeUserTempDataAsync(long userId, string tempData);

        Task<bool> ChangeUserIsActiveByIdAsync(long userId);

        Task<bool> AddSubscriptionToUserAsync(long userId, string username);

        Task<bool> RemoveSubscriptionFromUserAsync(long userId, string username);
    }
}
