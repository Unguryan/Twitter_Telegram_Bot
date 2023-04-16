namespace Twitter_Telegram.App.Services
{
    public interface INotifySubscriptionService
    {
        Task NotifyUsersAsync(string subUsername, List<long> updatedFriends);
    }
}
