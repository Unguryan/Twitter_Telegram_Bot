using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Telegram;

namespace Twitter_Telegram.Infrastructure.Services
{
    public class SubscriptionFacade : ISubscriptionFacade
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ITelegramUserService _userService;

        public SubscriptionFacade(ISubscriptionService subscriptionService, ITelegramUserService userService)
        {
            _subscriptionService = subscriptionService;
            _userService = userService;
        }

        public async Task<bool> AddSubscriptionAsync(long userId, string username)
        {
            var res = await _userService.AddSubscriptionAsync(userId, username);
            var sub = await _subscriptionService.AddSubscriptionAsync(username);

            return sub != null && res;
        }

        public async Task<bool> RemoveSubscriptionAsync(long userId, string username)
        {
            var res = await _userService.RemoveSubscriptionAsync(userId, username);
            var users = await _userService.GetUsersWithSubscriptionAsync(username);

            if (!users.Any())
            {
                var sub = await _subscriptionService.RemoveSubscriptionAsync(username);
                return sub != null && res;
            }

            return res;
        }
    }
}
