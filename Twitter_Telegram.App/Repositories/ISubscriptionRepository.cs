using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.App.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<List<Subscription>> GetSubscriptionsAsync();

        Task<Subscription?> GetSubscriptionsByUsernameAsync(string twitterUsername);

        Task<bool> ChangeSubscriptionsByUsernameAsync(string twitterUsername, List<long> friends);

        Task<Subscription?> AddSubscriptionAsync(string twitterUsername);

        Task<Subscription?> RemoveSubscriptionAsync(string twitterUsername);
    }
}
