using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.Domain.ViewModels
{
    public class CheckSubscriptionResultViewModel
    {
        public Subscription Subscription { get; set; }

        public bool IsOut { get; set; }

        public bool IsFound { get; set; }
        
        public bool IsChecked { get; set; }

        public bool IsUpdated { get; set; }

    }
}
