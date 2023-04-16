using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.App.Services
{
    public interface ISubscriptionService
    {
        Task<List<Subscription>> GetSubscriptionsAsync();

        //Task<List<Subscription>> GetSubscriptionsByUserIdAsync(long telegramUserId);
        Task<Subscription?> GetSubscriptionsByUsernameAsync(string twitterUsername);

        Task<bool> ChangeSubscriptionsByUsernameAsync(string twitterUsername, List<long> friends);

        Task<Subscription?> AddSubscriptionAsync(string twitterUsername);

        Task<Subscription?> RemoveSubscriptionAsync(string twitterUsername);

    }
}
