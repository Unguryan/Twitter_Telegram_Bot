using Twitter_Telegram.App.Repositories;
using Twitter_Telegram.App.Services.Telegram;
using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.Infrastructure.Services.Telegram
{
    public class TelegramUserService : ITelegramUserService
    {
        private readonly ITelegramRepository _telegramRepository;

        public TelegramUserService(ITelegramRepository telegramRepository)
        {
            _telegramRepository = telegramRepository;
        }

        public async Task<List<TelegramUser>> GetUsersAsync()
        {
            return await _telegramRepository.GetUsersAsync();
        }

        public async Task<List<TelegramUser>> GetUsersWithSubscriptionAsync(string userName)
        {
            return await _telegramRepository.GetUsersWithSubscriptionAsync(userName);
        }

        public async Task<TelegramUser?> GetUserByIdAsync(long userId)
        {
            return await _telegramRepository.GetUserByIdAsync(userId);
        }

        public async Task<TelegramUser?> AddUserByIdAsync(long userId)
        {
            if (await CheckIfUserExist(userId))
                return null;

            return await _telegramRepository.AddUserAsync(userId);
        }

        public async Task<TelegramUser?> ChangeUserStateAsync(long userId, TelegramUserState state)
        {
            if (!await CheckIfUserExist(userId))
                return null;

            return await _telegramRepository.ChangeUserStateAsync(userId, state);
        }

        public async Task<TelegramUser?> ChangeUserTempDataAsync(long userId, string tempData)
        {
            if (!await CheckIfUserExist(userId))
                return null;

            return await _telegramRepository.ChangeUserTempDataAsync(userId, tempData);
        }

        public async Task<bool> AddSubscriptionAsync(long userId, string username)
        {
            var user = await _telegramRepository.GetUserByIdAsync(userId);

            if (user == null || user.Usernames.Any(u => u.Equals(username)))
            {
                return false;
            }

            return await _telegramRepository.AddSubscriptionToUserAsync(userId, username);
        }

        public async Task<bool> RemoveSubscriptionAsync(long userId, string username)
        {
            var user = await _telegramRepository.GetUserByIdAsync(userId);

            if (user == null || !user.Usernames.Any(u => u.Equals(username)))
            {
                return false;
            }

            return await _telegramRepository.RemoveSubscriptionFromUserAsync(userId, username);
        }

        public async Task<bool> ActivateUserByIdAsync(long userId)
        {
            var user = await _telegramRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            if (user.IsActive)
            {
                return true;
            }

            return await _telegramRepository.ChangeUserIsActiveByIdAsync(userId);
        }

        private async Task<bool> CheckIfUserExist(long userId)
        {
            var user = await _telegramRepository.GetUserByIdAsync(userId);
            return user != null;
        }
    }
}
