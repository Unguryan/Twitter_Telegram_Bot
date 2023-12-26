namespace Twitter_Telegram.App.Services
{
    public interface ITwitterSettingsWriter
    {
        Task UpdateTwitterToken(string updatedToken);
    }
}
