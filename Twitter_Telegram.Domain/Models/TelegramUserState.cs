namespace Twitter_Telegram.Domain.Models
{
    public enum TelegramUserState
    {
        Start = 1,
        MainMenu = 2,
        AddNewSubscription = 3,
        AddNewSubscriptionConfirm = 4,
        SubscriptionList = 5,
        RemoveSubscription = 6,
        RemoveSubscriptionConfirm = 7,
    }
}
