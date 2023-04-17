namespace Twitter_Telegram.App.Services
{
    public interface ISubscriptionFacade
    {
        Task<bool> AddSubscriptionAsync(long userId, string username);

        Task<bool> RemoveSubscriptionAsync(long userId, string username);
    }
}
