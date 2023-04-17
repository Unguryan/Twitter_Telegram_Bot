namespace Twitter_Telegram.Domain.Models
{
    public enum TelegramUserState
    {
        Start,
        MainMenu,
        AddNewSubscription,
        AddNewSubscriptionConfirm,
        SubscriptionList,
        RemoveSubscription,
        RemoveSubscriptionConfirm,
    }
}
