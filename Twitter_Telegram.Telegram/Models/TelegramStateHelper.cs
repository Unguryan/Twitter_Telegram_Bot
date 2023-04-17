using Telegram.Bot.Types.ReplyMarkups;
using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.Telegram.Models
{
    public static class TelegramStateHelper
    {
        private static List<TelegramStateMessage> _messages;

        static TelegramStateHelper()
        {
            InitMessages();
        }
        public static TelegramStateMessage? GetTelegramState(TelegramUserState state)
        {
            return _messages.FirstOrDefault(x => x.State == state);
        }

        public static IEnumerable<KeyboardButton> CreateMainMenuButton()
        {
            return new[] { new KeyboardButton("To main menu") };
        }

        public static IEnumerable<IEnumerable<KeyboardButton>> CreateButtons(List<string> buttons)
        {
            List<List<KeyboardButton>> buttonsList = new List<List<KeyboardButton>>();

            for (int i = 0; i < buttons.Count; i++)
            {
                if((i + 1) < buttons.Count)
                {
                    buttonsList.Add(new List<KeyboardButton> { new KeyboardButton(buttons[i]), new KeyboardButton(buttons[i + 1]) });
                    i++;
                }
                else
                {
                    buttonsList.Add(new List<KeyboardButton> { new KeyboardButton(buttons[i]) });
                }
            }

            return buttonsList;
        }

        private static void InitMessages()
        {
            _messages = new List<TelegramStateMessage>()
            {
                new TelegramStateMessage()
                {
                    State = TelegramUserState.Start,
                    Message = "Hi. I am bot for checking friends for twitter users.",
                    Keyboard = null
                },
                new TelegramStateMessage()
                {
                    State = TelegramUserState.MainMenu,
                    Message = "Main Menu: ",
                    Keyboard = new ReplyKeyboardMarkup(new[]{
                        new[] { new KeyboardButton("Add sub") },
                        new[] { new KeyboardButton("Sub list") },
                        new[] { new KeyboardButton("Remove sub") }
                    
                    }),
                },
                new TelegramStateMessage()
                {
                    State = TelegramUserState.AddNewSubscription,
                    Message = "Type username without '@':",
                    Keyboard = new ReplyKeyboardMarkup(new[]{
                        new[] { new KeyboardButton("To main menu") },
                    }),
                },
                new TelegramStateMessage()
                {
                    State = TelegramUserState.AddNewSubscriptionConfirm,
                    Message = "User is found. User: <a href='https://twitter.com/{0}'>{0}</a>\nConfirm? (in keyboard)",
                    Keyboard = new ReplyKeyboardMarkup(new[]{
                        new[] { new KeyboardButton("Confirm") },
                        new[] { new KeyboardButton("To main menu") },
                    }),
                },
                new TelegramStateMessage()
                {
                    State = TelegramUserState.SubscriptionList,
                    Message = "Subscription list:\n\n{0}",
                    Keyboard = null
                },
                new TelegramStateMessage()
                {
                    State = TelegramUserState.RemoveSubscription,
                    Message = "Select subscription to remove:",
                    Keyboard = null
                },
                new TelegramStateMessage()
                {
                    State = TelegramUserState.RemoveSubscriptionConfirm,
                    Message = "You sure? Removing <b>{0}</b> user.\nConfirm?",
                    Keyboard = new ReplyKeyboardMarkup(new[]{
                        new[] { new KeyboardButton("Confirm") },
                        new[] { new KeyboardButton("To main menu") },
                    }),
                }

            };
        }
    }
}
