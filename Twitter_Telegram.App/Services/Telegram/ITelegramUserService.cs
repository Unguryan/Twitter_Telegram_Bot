using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.App.Services.Telegram
{
    public interface ITelegramUserService
    {
        Task<List<TelegramUser>> GetUsersAsync();

        Task<TelegramUser?> GetUserByIdAsync(long userId);

        Task<TelegramUser?> AddUserByIdAsync(long userId, string userName);

        Task<bool> ActivateUserByIdAsync(long userId);

        Task<bool> AddSubscriptionAsync(long userId, string username);

        Task<bool> RemoveSubscriptionAsync(long userId, string username);
    }
}
