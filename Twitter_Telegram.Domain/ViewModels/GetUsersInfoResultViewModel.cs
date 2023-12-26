using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.Domain.ViewModels
{
    public class GetUsersInfoResultViewModel
    {
        public bool IsOut { get; set; }

        public bool IsFound { get; set; }

        public List<TwitterUser>? TwitterUsers { get; set; }
    }
}
