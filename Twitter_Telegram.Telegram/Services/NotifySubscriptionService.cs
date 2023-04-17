using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Telegram;
using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.Infrastructure.Services
{
    public class NotifySubscriptionService : INotifySubscriptionService
    {
        private readonly ITelegramBotClient _client;
        private readonly ITelegramUserService _userService;
        private readonly IApiReader _apiReader;

        public NotifySubscriptionService(ITelegramBotClient client, ITelegramUserService userService, IApiReader apiReader)
        {
            _client = client;
            _userService = userService;
            _apiReader = apiReader;
        }

        public async Task NotifyUsersAsync(string subUsername, List<long> updatedFriends)
        {
            var users = await _userService.GetUsersWithSubscriptionAsync(subUsername);

            var friends = new List<TwitterUser>();

            foreach (var friendId in updatedFriends)
            {
                var user = await _apiReader.GetUserInfoByUserIdAsync(friendId.ToString());
                if (user != null)
                {
                    friends.Add(user);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMinutes(15));
                    user = await _apiReader.GetUserInfoByUserIdAsync(friendId.ToString());
                    friends.Add(user);
                }
            }

            foreach (var userId in users.Select(u => u.Id))
            {
                foreach (var userName in friends.Select(f => f.Username))
                {
                    var message = CreateMessage(subUsername, userName);
                    await SendTextMessageAsync(userId, message);
                }
            }
        }

        private string CreateMessage(string subUsername, string userName)
        {
            return "<b>New following!</b>\n\n" +
                  $"Sub: <a href='https://twitter.com/{subUsername}'>{subUsername}</a>\n" +
                  $"New following: <a href='https://twitter.com/{userName}'>{userName}</a>";
        }

        private async Task SendTextMessageAsync(long userId, string message)
        {
            await _client.SendTextMessageAsync(new ChatId(userId),
                                                    message,
                                                    ParseMode.Html);
        }
    }
}
