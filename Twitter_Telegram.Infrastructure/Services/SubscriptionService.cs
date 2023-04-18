using Twitter_Telegram.App.Repositories;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.Infrastructure.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<List<Subscription>> GetSubscriptionsAsync()
        {
            return await _subscriptionRepository.GetSubscriptionsAsync();
        }

        public async Task<Subscription?> GetSubscriptionsByUsernameAsync(string twitterUsername)
        {
            return await _subscriptionRepository.GetSubscriptionsByUsernameAsync(twitterUsername);
        }

        public async Task<Subscription?> AddSubscriptionAsync(string twitterUsername)
        {
            if (string.IsNullOrEmpty(twitterUsername))
            {
                return null;
            }

            var sub = await _subscriptionRepository.GetSubscriptionsByUsernameAsync(twitterUsername);

            if(sub != null)
            {
                return sub;
            }

            return await _subscriptionRepository.AddSubscriptionAsync(twitterUsername);
        }

        public async Task<bool> ChangeSubscriptionsByUsernameAsync(string twitterUsername, List<long> friends)
        {
            var sub = await _subscriptionRepository.GetSubscriptionsByUsernameAsync(twitterUsername);

            if (sub == null)
            {
                return false;
            }

            return await _subscriptionRepository.ChangeSubscriptionsByUsernameAsync(twitterUsername, friends);
        }

        public async Task<Subscription?> RemoveSubscriptionAsync(string twitterUsername)
        {
            var sub = await _subscriptionRepository.GetSubscriptionsByUsernameAsync(twitterUsername);

            if (sub == null)
            {
                return null;
            }

            return await _subscriptionRepository.RemoveSubscriptionAsync(twitterUsername);
        }
    }
}
