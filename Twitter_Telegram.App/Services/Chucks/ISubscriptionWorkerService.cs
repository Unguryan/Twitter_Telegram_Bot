using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.App.Services.Chucks
{
    public interface ISubscriptionWorkerService
    {
        Task<List<Subscription>> CheckSubscriptions(List<Subscription> subs, CancellationToken cancellationToken);
    }
}
