using Telegram.Bot.Types.ReplyMarkups;
using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.Telegram.Models
{
    public class TelegramStateMessage
    {
        public TelegramUserState State { get; set; }
        public string Message { get; set; }
        public ReplyKeyboardMarkup? Keyboard { get; set; }
    }
}
