using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.Domain.ViewModels
{
    public class GetUserInfoResultViewModel
    {
        public bool IsOut { get; set; }

        public bool IsFound { get; set; }

        public TwitterUser? TwitterUser { get; set; }
    }
}
