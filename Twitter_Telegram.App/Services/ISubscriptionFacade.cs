using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.App.Services
{
    public interface ISubscriptionFacade
    {
        Task<bool> AddSubscriptionToUserAsync(long userId, TwitterUser userToAdd);

        Task<bool> RemoveSubscriptionFromUserAsync(long userId, TwitterUser userToAdd);
    }
}
