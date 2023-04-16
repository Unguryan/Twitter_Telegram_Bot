namespace Twitter_Telegram.App.Services.Telegram
{
    public interface IMessageReader
    {
        Task ReadUserMessageAsync(long userId, string message);
    }
}
