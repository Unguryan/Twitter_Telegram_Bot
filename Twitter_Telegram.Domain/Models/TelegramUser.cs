namespace Twitter_Telegram.Domain.Models
{
    public class TelegramUser
    {
        public long Id { get; set; }

        public TelegramUserState State { get; set; }

        public bool IsActive { get; set; }

        public List<string> UserNames { get; set; }

        public string? UserTempData { get; set; }
    }
}
