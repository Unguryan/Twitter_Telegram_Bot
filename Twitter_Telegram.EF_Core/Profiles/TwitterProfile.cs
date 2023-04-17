using AutoMapper;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.EF_Core.Models;

namespace Twitter_Telegram.EF_Core.Profiles
{
    public class TwitterProfile : Profile
    {
        public TwitterProfile()
        {
            CreateMap<TelegramUserDbo, TelegramUser>().ReverseMap();
            CreateMap<SubscriptionDbo, Subscription>().ReverseMap();
        }
    }
}
