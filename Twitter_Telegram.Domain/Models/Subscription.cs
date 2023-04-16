namespace Twitter_Telegram.Domain.Models
{
    public class Subscription
    {
        public string Username { get; set; }

        public List<ulong> Friends { get; set; }

    }
}
