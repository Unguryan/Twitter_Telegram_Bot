using Microsoft.Extensions.DependencyInjection;
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
        //Just to Get updated repositories (DbContext)
        private readonly IServiceProvider _serviceProvider;

        private readonly IApiReaderV2 _apiReader;

        private readonly ITelegramBotClient _telegramBot;

        public MessageReader(
                             IApiReaderV2 apiReader,
                             IServiceProvider serviceProvider,
                             ITelegramBotClient telegramBot)
        {
            _apiReader = apiReader;
            _serviceProvider = serviceProvider;
            _telegramBot = telegramBot;
        }

        public async Task ReadUserMessageAsync(long userId, string message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();

                var user = await userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    user = await userService.AddUserByIdAsync(userId);
                }

                if (message.Equals("/activate password1488"))
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
                "/addsubs password1488" => AddSubsCommand(user),
                _ => null,
            };

            if (action == null)
            {
                return false;
            }

            await action;
            return true;
        }

        private async Task AddSubsCommand(TelegramUser? user)
        {
            var list = new List<string>()
            {
                "sina_eth_",
                "_jamico",
                "achalvs",
                "AmosMeiri",
                "boz_menzalji",
                "johnniecosmos",
                "hdevalence",
                "jh_0x",
                "Provalidator",
                "yekiM_o",

                "EthExploring",
                "ChorusOne",
                "ceterispar1bus",
                "Ruesandora0",
                "DaBooligan",
                "javalry1",
                "EnigmaValidator",
                "VelvetMilkman",
                "ProofOfStack",
                "DanSimerman",

                "NodeStake_top",
                "ChillinValidtn",
                "swiss_staking",
                "JoeAbbey",
                "mikeyjhlee",
                "dorianjcor",
                "mlmi1r",
                "stakecito",
                "StakeandRelax",
                "nuss_eli",

                "validatrium",
                "HighStakes_CH",
                "LeThang137",
                "yongjoojung",
                "RoyLearner",
                "Galois_Capital",
                "cooper_emmons",
                "bernardchancl",
                "xxxxpark",
                "stevenshi_",

                "wallfac3r",
                "robcrt",
                "m_mark_0",
                "thealbrechtwolf",
                "0xRaghav",
                "shanav_m",
                "MariaShen",
                "cheryldchan",
                "cvhessert",
                "anjan_vinod",

                "TraderNoah",
                "jmasonbump",
                "boobcactus",
                "stakefrites_",
                "brezshares",
                "theevanchen",
                "0xshishi",
                "reganbozman",
                "Hootie_R",
                "PChuzeville",

                "AlpenSheth",
                "daongok",
                "EvansHuangfu",
                "RoHumanist",
                "dogemos",
                "yjbdrr",
                "0xgorany",
                "KamBenbrik",
                "genznodes",
                "fededaffina",

                "c__bergz",
                "gpl_94",
                "jamiedsully",
                "samuelmandrew",
                "a41_john",
                "linfluence",
                "DavidAlderman_",
                "ggscanlon",
                "mishika251",
                "hi_kl__o_",

                "jad3lore",
                "joseph_13x",
                "yunfan0x",
                "EllaineXu",
                "stevencquinn",
                "_alekslarsen",
                "sproule_",
                "_MinTeo",
                "dcjanio",
                "mattashley__",

                "odagius",
                "weikengchen",
                "SpencerFarrar",
                "4lili41",
                "polkachu_intern",
                "alexhevans",
                "stefancoh",
                "abondar92",
                "roybitsir",
                "Crypto_Backer1",

                "anujshankar95",
                "0xyanshu",
                "armaan_lgns",
                "nishantdiddigi7",
            };

            var outs = new List<string>();

            foreach (var item in list)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var facade = scope.ServiceProvider.GetRequiredService<ISubscriptionFacade>();

                    var res = await facade.AddSubscriptionAsync(user.Id, item);
                    if (!res)
                    {
                        outs.Add(item);
                    }
                }
            }

            if (outs.Any())
            {
                var str = string.Empty;

                outs.ForEach(o => str += str + ", ");

                await SendTextMessageAsync(user.Id, $"Users Added. But, {outs.Count} user were not added.\n\n{str}");
            }
            else
            {
                await SendTextMessageAsync(user.Id, "Users Added.");
            }
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

            if (action != null)
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

            if (action == null)
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

            var twitterUsers = await _apiReader.GetUserInfoByUsernameAsync(new List<string>() { message });
            if (!twitterUsers.IsFound)
            {
                await SendTextMessageAsync(user.Id, $"User <b>{message}</b> does not exist.");
                return;
            }

            var twitterUser = twitterUsers.TwitterUsers[0];

            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                await userService.ChangeUserTempDataAsync(user.Id, twitterUser.Username);
                await userService.ChangeUserStateAsync(user.Id, TelegramUserState.AddNewSubscriptionConfirm);
            }

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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                    var facade = scope.ServiceProvider.GetRequiredService<ISubscriptionFacade>();
                    var res = await facade.AddSubscriptionAsync(user.Id, user.UserTempData);
                    if (res)
                    {
                        await SendTextMessageAsync(user.Id, $"Success. User: <a href='https://twitter.com/{user.UserTempData}'>{user.UserTempData}</a> added.");
                    }
                    else
                    {
                        await SendTextMessageAsync(user.Id, $"Error. User was not added.");
                        if (user.Usernames.Any(u => u.Equals(user.UserTempData)))
                        {
                            await SendTextMessageAsync(user.Id, $"User \"{user.UserTempData}\" already in list.");
                        }
                    }

                    await userService.ChangeUserTempDataAsync(user.Id, string.Empty);
                    await SendMenuCommand(user);
                }
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

            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                await userService.ChangeUserTempDataAsync(user.Id, message);
                await userService.ChangeUserStateAsync(user.Id, TelegramUserState.RemoveSubscriptionConfirm);
            }

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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                    var facade = scope.ServiceProvider.GetRequiredService<ISubscriptionFacade>();

                    var res = await facade.RemoveSubscriptionAsync(user.Id, user.UserTempData);
                    if (res)
                    {
                        await SendTextMessageAsync(user.Id, $"Success. User: <a href='https://twitter.com/{user.UserTempData}'>{user.UserTempData}</a> removed.");
                    }
                    else
                    {
                        await SendTextMessageAsync(user.Id, $"Error. User was not removed.");
                    }

                    await userService.ChangeUserTempDataAsync(user.Id, string.Empty);
                    await SendMenuCommand(user);
                }
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

            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                await userService.ChangeUserStateAsync(user.Id, TelegramUserState.RemoveSubscription);
            }
        }

        private async Task SendAddSubscriptionCommand(TelegramUser? user)
        {
            var message = TelegramStateHelper.GetTelegramState(TelegramUserState.AddNewSubscription);

            await SendTextMessageAsync(user.Id, message.Message, message.Keyboard);

            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                await userService.ChangeUserStateAsync(user.Id, TelegramUserState.AddNewSubscription);
            }
        }

        private async Task SendMySubscriptionCommand(TelegramUser? user)
        {
            if (user.Usernames == null || !user.Usernames.Any())
            {
                await SendTextMessageAsync(user.Id, "You do not have any subs to see.");
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                var subs = await subscriptionService.GetSubscriptionsAsync();

                var subsToSee = subs.Where(s => user.Usernames.Any(x => x.Equals(s.Username)));

                var message = TelegramStateHelper.GetTelegramState(TelegramUserState.SubscriptionList);

                var counter = 1;
                var sb = new StringBuilder();
                foreach (var sub in subsToSee)
                {
                    sb.Append(counter++).Append(". ").Append(sub.Username)
                        .Append(": ").Append(sub.Friends?.Count)
                        .Append(" F, ").Append(sub.LastTimeChecked?.ToShortTimeString())
                        .Append("\n");
                }

                await SendTextMessageAsync(user.Id, string.Format(message.Message, sb.ToString()), message.Keyboard);

                await SendMenuCommand(user);
            }
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

            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                await userService.ChangeUserStateAsync(user.Id, TelegramUserState.MainMenu);
            }
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
            using (var scope = _serviceProvider.CreateScope())
            {
                var userService = scope.ServiceProvider.GetRequiredService<ITelegramUserService>();
                var res = await userService.ActivateUserByIdAsync(user.Id);

                if (res)
                {
                    await SendTextMessageAsync(user.Id, "Activated!");
                }
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
