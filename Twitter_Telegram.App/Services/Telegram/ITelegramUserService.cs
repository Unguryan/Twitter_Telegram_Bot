using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.App.Services.Telegram
{
    public interface ITelegramUserService
    {
        Task<List<TelegramUser>> GetUsersAsync();

        Task<List<TelegramUser>> GetUsersWithSubscriptionAsync(string userName);

        Task<TelegramUser?> GetUserByIdAsync(long userId);

        Task<TelegramUser?> ChangeUserStateAsync(long userId, TelegramUserState state);
        
        Task<TelegramUser?> ChangeUserTempDataAsync(long userId, string tempData);

        Task<TelegramUser?> AddUserByIdAsync(long userId);

        Task<bool> ActivateUserByIdAsync(long userId);

        Task<bool> AddSubscriptionAsync(long userId, string username);

        Task<bool> RemoveSubscriptionAsync(long userId, string username);
    }
}
