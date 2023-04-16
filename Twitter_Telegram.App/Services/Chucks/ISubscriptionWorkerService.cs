namespace Twitter_Telegram.App.Services.Chucks
{
    public interface ISubscriptionWorkerService
    {
        Task<int> CheckSubscriptions(CancellationToken cancellationToken);
    }
}
