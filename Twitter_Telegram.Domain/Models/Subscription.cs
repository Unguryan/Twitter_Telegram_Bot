namespace Twitter_Telegram.Domain.Models
{
    public class Subscription
    {
        public string Username { get; set; }

        public List<long> Friends { get; set; }

        public DateTime? LastTimeChecked { get; set; }

    }
}
