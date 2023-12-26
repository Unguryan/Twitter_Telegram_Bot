using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.App.Services.Chucks
{
    public interface ISubscriptionWorkerService
    {
        Task<List<CheckSubscriptionResultViewModel>?> CheckSubscriptions(List<Subscription> subs, List<GetUsersInfoResultViewModel> users, CancellationToken cancellationToken);
    }
}
