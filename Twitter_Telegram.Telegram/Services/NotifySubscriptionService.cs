using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Telegram;
using Twitter_Telegram.Domain.Models;
using System.Text;
using System.Collections.Generic;
using System.Drawing;

namespace Twitter_Telegram.Infrastructure.Services
{
    public class NotifySubscriptionService : INotifySubscriptionService
    {
        private readonly ITelegramBotClient _client;
        private readonly ITelegramUserService _userService;
        private readonly IApiReaderV2 _apiReader;

        public NotifySubscriptionService(ITelegramBotClient client, ITelegramUserService userService, IApiReaderV2 apiReader)
        {
            _client = client;
            _userService = userService;
            _apiReader = apiReader;
        }

        public async Task NotifyUsersAsync(string subUsername, List<long> updatedFriends)
        {
            var users = await _userService.GetUsersWithSubscriptionAsync(subUsername);

            var friends = new List<TwitterUser>();

            while (true)
            {
                var result = await _apiReader.GetUserInfoByUserIdAsync(updatedFriends.Select(u => u.ToString()).ToList());

                foreach (var res in result)
                {
                    if (res.IsOut)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(15));
                        continue;
                    }

                    friends.AddRange(res.TwitterUsers);
                }

                break;
            }

            //foreach (var friendId in updatedFriends)
            //{
            //    while (true)
            //    {
            //        var user = await _apiReader.GetUserInfoByUserIdAsync(friendId.ToString());

            //        if (user.IsOut)
            //        {
            //            await Task.Delay(TimeSpan.FromMinutes(15));
            //            continue;
            //        }

            //        if (!user.IsFound)
            //        {
            //            break;
            //        }

            //        friends.Add(user.TwitterUser);
            //        break;
            //    }
            //}

            foreach (var userId in users.Select(u => u.Id))
            {
                var messages = CreateMessage(subUsername, friends.Select(f => f.Username).ToList());
                foreach (var message in messages)
                {
                    await SendTextMessageAsync(userId, message);
                }
            }
        }

        public async Task SubscriptionRemovedAsync(long userId, string username)
        {
            var message = CreateErrorMessage(username);
            await SendTextMessageAsync(userId, message);
        }

        private string CreateErrorMessage(string userName)
        {
            return "<b>Error!</b>\n\n" +
                  $"Sub: <a href='https://twitter.com/{userName}'>{userName}</a> was not found. REMOVED!";
        }

        private List<string> CreateMessage(string subUsername, List<string> userNames)
        {
            var result = new List<string>();
            var subMessage = new StringBuilder();

            for (int i = 0; i < userNames.Count; i += 40)
            {
                var chunk = new string[Math.Min(40, userNames.Count - i)];
                Array.Copy(userNames.ToArray(), i, chunk, 0, chunk.Length);

                var message = "<b>New following!</b>\n\n" +
                       $"Sub: <a href='https://twitter.com/{subUsername}'>{subUsername}</a>\n\n";
                subMessage.Append(message);

                for (int j = 0; j < chunk.Length; j++)
                {
                    subMessage.Append($"{(j + 1)}. New following: <a href='https://twitter.com/{chunk[j]}'>{chunk[j]}</a>\n");
                }

                result.Add(subMessage.ToString());
                subMessage.Clear();
            }

            //var isInit = false;
            //var counter = 0;
            //foreach (var userName in userNames)
            //{
            //    if (!isInit)
            //    {
            //        var message = "<b>New following!</b>\n\n" +
            //           $"Sub: <a href='https://twitter.com/{subUsername}'>{subUsername}</a>\n\n";
            //        subMessage.Append(message);
            //        isInit = true;
            //    }

            //    if(counter == 40)
            //    {
            //        result.Add(subMessage.ToString());
            //        isInit = false;
            //        counter = 0;
            //        subMessage.Clear();
            //    }

            //    subMessage.Append($"{++counter}. New following: <a href='https://twitter.com/{userName}'>{userName}</a>\n");
            //}

            return result;
        }

        private async Task SendTextMessageAsync(long userId, string message)
        {
            await _client.SendTextMessageAsync(new ChatId(userId),
                                                    message,
                                                    ParseMode.Html);
        }
    }
}
