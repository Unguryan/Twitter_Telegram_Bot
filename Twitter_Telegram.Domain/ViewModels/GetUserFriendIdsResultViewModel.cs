namespace Twitter_Telegram.Domain.ViewModels
{
    public class GetUserFriendIdsResultViewModel
    {
        public bool IsOut { get; set; }

        public bool IsFound { get; set; }

        public List<long>? FriendIds { get; set; }
    }
}
