namespace Twitter_Telegram.Domain.Config
{
    public class TelegramOptions
    {
        public static readonly string SectionName = "BotOptions";

        public string BotToken { get; set; }

    }
}
