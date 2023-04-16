namespace Twitter_Telegram.Telegram.Abstraction
{
    public interface IReceiverService
    {
        Task ReceiveAsync(CancellationToken stoppingToken);
    }
}
