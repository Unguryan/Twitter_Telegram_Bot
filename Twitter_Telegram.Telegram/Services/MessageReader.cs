using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.App.Services.Telegram;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.Telegram.Models;

namespace Twitter_Telegram.Telegram.Services
{
    public class MessageReader : IMessageReader
    {
        private readonly ITelegramUserService _userService;
        private readonly IApiReader _apiReader;
        private readonly ISubscriptionFacade _facade;
        private readonly ITelegramBotClient _telegramBot;

        public MessageReader(ITelegramUserService userService,
                             IApiReader apiReader,
                             ISubscriptionFacade facade,
                             ITelegramBotClient telegramBot)
        {
            _userService = userService;
            _apiReader = apiReader;
            _facade = facade;
            _telegramBot = telegramBot;
        }

        public async Task ReadUserMessageAsync(long userId, string message)
        {
            var user = await _userService.GetUserByIdAsync(userId);

            if(user == null)
            {
                user = await _userService.AddUserByIdAsync(userId);
            }

            if(message.Equals("/activate password1488"))
            {
                await ActivateUser(user);
            }

            if (!user.IsActive)
            {
                return;
            }

            if (await CheckBackToMain(user, message))
            {
                return;
            }

            var res = await ParseCommand(user, message);

            if (!res)
            {
                await ParseMessage(user, message);
            }
        }

        private async Task<bool> ParseCommand(TelegramUser? user, string message)
        {
            var action = message switch
            {
                "/start" => SendStartCommand(user),
                "/menu" => SendMenuCommand(user),
                "/mysub" => SendMySubscriptionCommand(user),
                "/addsub" => SendAddSubscriptionCommand(user),
                "/removesub" => SendRemoveSubscriptionCommand(user),
                _ => null,
            };

            if(action == null)
            {
                return false;
            }

            await action;
            return true;
        }

        private async Task ParseMessage(TelegramUser? user, string message)
        {
            var action = user.State switch
            {
                TelegramUserState.MainMenu => ParseMainMenuResponse(user, message),
                TelegramUserState.AddNewSubscription => ParseAddSubResponse(user, message),
                TelegramUserState.AddNewSubscriptionConfirm => ParseAddSubConfirmResponse(user, message),
                TelegramUserState.RemoveSubscription => ParseRemoveSubResponse(user, message),
                TelegramUserState.RemoveSubscriptionConfirm => ParseRemoveSubConfirmResponse(user, message),
                _ => null,
            };

            if(action != null)
            {
                await action;
            }
        }

        private async Task ParseMainMenuResponse(TelegramUser user, string message)
        {
            var action = message switch
            {
                "Add sub" => SendAddSubscriptionCommand(user),
                "Sub list" => SendMySubscriptionCommand(user),
                "Remove sub" => SendRemoveSubscriptionCommand(user),
                _ => null
            };

            if(action == null)
            {
                await SendTextMessageAsync(user.Id, "No such command.");
                return;
            }

            await action;
        }

        private async Task ParseAddSubResponse(TelegramUser user, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await SendTextMessageAsync(user.Id, "No such command.");
                return;
            }

            if (user.Usernames.Any(x => x.Equals(message)))
            {
                await SendTextMessageAsync(user.Id, $"User <b>{message}</b> already in sub list.");
                return;
            }

            var twitterUser = await _apiReader.GetUserInfoByUsernameAsync(message);

            if(twitterUser == null)
            {
                await SendTextMessageAsync(user.Id, $"User <b>{message}</b> does not exist.");
                return;
            }

            await _userService.ChangeUserTempDataAsync(user.Id, twitterUser.Username);
            await _userService.ChangeUserStateAsync(user.Id, TelegramUserState.AddNewSubscriptionConfirm);

            var messageVM = TelegramStateHelper.GetTelegramState(TelegramUserState.AddNewSubscriptionConfirm);
            await SendTextMessageAsync(user.Id, string.Format(messageVM.Message, message), messageVM.Keyboard);
        }

        private async Task ParseAddSubConfirmResponse(TelegramUser user, string message)
        {
            if (string.IsNullOrEmpty(user.UserTempData))
            {
                await SendTextMessageAsync(user.Id, "Error. User non in temp data.");
                await SendMenuCommand(user);
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                await SendTextMessageAsync(user.Id, "No such command.");
                return;
            }

            if (message.Equals("Confirm"))
            {
                var res = await _facade.AddSubscriptionAsync(user.Id, user.UserTempData);
                if(res)
                {
                    await SendTextMessageAsync(user.Id, $"Success. User: <a href='https://twitter.com/{user.UserTempData}'>{user.UserTempData}</a> added.");
                }
                else
                {
                    await SendTextMessageAsync(user.Id, $"Error. User was not added.");
                }

                await _userService.ChangeUserTempDataAsync(user.Id, string.Empty);
                await SendMenuCommand(user);
            }
        }

        private async Task ParseRemoveSubResponse(TelegramUser user, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                await SendTextMessageAsync(user.Id, "No such command.");
                return;
            }

            if (!user.Usernames.Any(x => x.Equals(message)))
            {
                await SendTextMessageAsync(user.Id, $"User <b>{message}</b> does not exist in sub list.");
                return;
            }

            await _userService.ChangeUserTempDataAsync(user.Id, message);
            await _userService.ChangeUserStateAsync(user.Id, TelegramUserState.RemoveSubscriptionConfirm);

            var messageVM = TelegramStateHelper.GetTelegramState(TelegramUserState.RemoveSubscriptionConfirm);
            await SendTextMessageAsync(user.Id, string.Format(messageVM.Message, message), messageVM.Keyboard);
        }

        private async Task ParseRemoveSubConfirmResponse(TelegramUser user, string message)
        {
            if (string.IsNullOrEmpty(user.UserTempData))
            {
                await SendTextMessageAsync(user.Id, "Error. User non in temp data.");
                await SendMenuCommand(user);
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                await SendTextMessageAsync(user.Id, "No such command.");
                return;
            }

            if (message.Equals("Confirm"))
            {
                var res = await _facade.RemoveSubscriptionAsync(user.Id, user.UserTempData);
                if (res)
                {
                    await SendTextMessageAsync(user.Id, $"Success. User: <a href='https://twitter.com/{user.UserTempData}'>{user.UserTempData}</a> removed.");
                }
                else
                {
                    await SendTextMessageAsync(user.Id, $"Error. User was not removed.");
                }

                await _userService.ChangeUserTempDataAsync(user.Id, string.Empty);
                await SendMenuCommand(user);
            }
        }

        private async Task SendRemoveSubscriptionCommand(TelegramUser? user)
        {
            if (user.Usernames == null || !user.Usernames.Any())
            {
                await SendTextMessageAsync(user.Id, "You do not have any subs to remove.");
                return;
            }

            var message = TelegramStateHelper.GetTelegramState(TelegramUserState.RemoveSubscription);

            var keys = TelegramStateHelper.CreateButtons(user.Usernames).ToList();
            keys.Insert(0, TelegramStateHelper.CreateMainMenuButton());
            var keyBoard = new ReplyKeyboardMarkup(keys);

            await SendTextMessageAsync(user.Id, message.Message, keyBoard);

            await _userService.ChangeUserStateAsync(user.Id, TelegramUserState.RemoveSubscription);
        }

        private async Task SendAddSubscriptionCommand(TelegramUser? user)
        {
            var message = TelegramStateHelper.GetTelegramState(TelegramUserState.AddNewSubscription);

            await SendTextMessageAsync(user.Id, message.Message, message.Keyboard);

            await _userService.ChangeUserStateAsync(user.Id, TelegramUserState.AddNewSubscription);
        }

        private async Task SendMySubscriptionCommand(TelegramUser? user)
        {
            if(user.Usernames == null || !user.Usernames.Any())
            {
                await SendTextMessageAsync(user.Id, "You do not have any subs to see.");
                return;
            }

            var message = TelegramStateHelper.GetTelegramState(TelegramUserState.SubscriptionList);

            var counter = 1;
            var sb = new StringBuilder();
            foreach (var userName in user.Usernames)
            {
                sb.Append(counter++).Append(". ").Append(userName).Append("\n");
            }

            await SendTextMessageAsync(user.Id, string.Format(message.Message, sb.ToString()), message.Keyboard);

            await SendMenuCommand(user);
        }
        private async Task SendStartCommand(TelegramUser? user)
        {
            var message = TelegramStateHelper.GetTelegramState(TelegramUserState.Start);

            await SendTextMessageAsync(user.Id, message.Message, message.Keyboard);

            await SendMenuCommand(user);
        }

        private async Task SendMenuCommand(TelegramUser? user)
        {
            var message = TelegramStateHelper.GetTelegramState(TelegramUserState.MainMenu);

            await SendTextMessageAsync(user.Id, message.Message, message.Keyboard);

            await _userService.ChangeUserStateAsync(user.Id, TelegramUserState.MainMenu);
        }

        private async Task SendTextMessageAsync(long userId, string message, ReplyKeyboardMarkup? markup)
        {
            await _telegramBot.SendTextMessageAsync(new ChatId(userId),
                                                    message, 
                                                    ParseMode.Html, 
                                                    replyMarkup: markup);
        }

        private async Task SendTextMessageAsync(long userId, string message)
        {
            await _telegramBot.SendTextMessageAsync(new ChatId(userId),
                                                    message,
                                                    ParseMode.Html);
        }

        private async Task ActivateUser(TelegramUser? user)
        {
            var res = await _userService.ActivateUserByIdAsync(user.Id);

            if (res)
            {
                await SendTextMessageAsync(user.Id, "Activated!");
            }
        }

        private async Task<bool> CheckBackToMain(TelegramUser user, string message)
        {
            if (message.Equals("To main menu"))
            {
                await SendMenuCommand(user);
                return true;
            }

            return false;
        }
    }
}
