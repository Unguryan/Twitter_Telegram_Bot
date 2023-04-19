using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Twitter_Telegram.App.Repositories;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.EF_Core.Context;
using Twitter_Telegram.EF_Core.Models;

namespace Twitter_Telegram.EF_Core.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly TwitterContext _context;
        private readonly IMapper _mapper;

        public SubscriptionRepository(TwitterContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<Subscription>> GetSubscriptionsAsync()
        {
            var dbos = await _context.Subscriptions.ToListAsync();
            var res = new List<Subscription>();

            foreach (var dbo in dbos)
            {
                var item = Convert(dbo);
                if (item != null)
                {
                    res.Add(item);
                }
            }

            return res;
            //return await _context.Subscriptions.Select(u => Convert(u)).ToListAsync();
        }

        public async Task<Subscription?> GetSubscriptionsByUsernameAsync(string twitterUsername)
        {
            var sub = await _context.Subscriptions.FirstOrDefaultAsync(u => u.Username == twitterUsername);

            return Convert(sub);
        }

        public async Task<Subscription?> AddSubscriptionAsync(string twitterUsername)
        {
            var subRes = await _context.Subscriptions.AddAsync(new SubscriptionDbo()
            {
                Username = twitterUsername,
                FriendsJson = string.Empty,
                IsInit = false,
                FriendsCount = null,
                LastTimeChecked = null
            });

            var res = await _context.SaveChangesAsync();

            if (res > 0)
            {
                return Convert(subRes.Entity);
            }

            return null;
        }
        public async Task<bool> ChangeSubscriptionLastTimeCheckAsync(string twitterUsername)
        {
            var sub = await _context.Subscriptions.FirstOrDefaultAsync(u => u.Username == twitterUsername);

            sub.LastTimeChecked = DateTime.Now;

            var res = await _context.SaveChangesAsync();
            return res > 0;
        }

        public async Task<bool> ChangeSubscriptionsByUsernameAsync(string twitterUsername, int friendsCount, List<long> friends)
        {
            var sub = await _context.Subscriptions.FirstOrDefaultAsync(u => u.Username == twitterUsername);

            sub.FriendsJson = JsonConvert.SerializeObject(friends);
            sub.LastTimeChecked = DateTime.Now;
            sub.FriendsCount = friendsCount;

            if (!sub.IsInit)
            {
                sub.IsInit = true;
            }

            var res = await _context.SaveChangesAsync();
            return res > 0;
        }

        public async Task<Subscription?> RemoveSubscriptionAsync(string twitterUsername)
        {
            var sub = await _context.Subscriptions.FirstOrDefaultAsync(u => u.Username == twitterUsername);
            var subRes = Convert(sub);

            _context.Subscriptions.Remove(sub);

            var res = await _context.SaveChangesAsync();

            if (res > 0)
            {
                return subRes;
            }

            return null;
        }

        private Subscription? Convert(SubscriptionDbo? dbo)
        {
            if(dbo == null)
            {
                return null;
            }

            var res = _mapper.Map<Subscription>(dbo);
            res.Friends = JsonConvert.DeserializeObject<List<long>>(dbo.FriendsJson) ?? new List<long>();

            return res;
        }

        
    }
}
