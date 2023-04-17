using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Twitter_Telegram.App.Repositories;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.EF_Core.Context;
using Twitter_Telegram.EF_Core.Models;

namespace Twitter_Telegram.EF_Core.Repositories
{
    public class TelegramRepository : ITelegramRepository
    {
        private readonly TwitterContext _context;
        private readonly IMapper _mapper;

        public TelegramRepository(TwitterContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<TelegramUser>> GetUsersAsync()
        {
            var dbos = await _context.Users.ToListAsync();
            var res = new List<TelegramUser>();

            foreach (var dbo in dbos)
            {
                var item = Convert(dbo);
                if (item != null)
                {
                    res.Add(item);
                }
            }

            return res;

            //return await _context.Users.Select(u => Convert(u)).ToListAsync();
        }

        public async Task<List<TelegramUser>> GetUsersWithSubscriptionAsync(string userName)
        {
            var dbos = await _context.Users.ToListAsync();
            var res = new List<TelegramUser>();

            foreach (var dbo in dbos.Where(u => u.UsernamesJson.Contains(userName)))
            {
                var item = Convert(dbo);
                if (item != null)
                {
                    res.Add(item);
                }
            }

            return res;

            //return _context.Users.AsEnumerable().Where(u => u.UsernamesJson.Contains(userName))
            //    .Select(u => Convert(u)).ToList();
        }

        public async Task<TelegramUser?> GetUserByIdAsync(long userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            return Convert(user);
        }

        public async Task<TelegramUser?> AddUserAsync(long userId)
        {
            var userRes = await _context.Users.AddAsync(new TelegramUserDbo()
            {
                Id = userId,
                IsActive = false,
                State = 2,
                UsernamesJson = string.Empty,
                UserTempData = null
            });

            var res = await _context.SaveChangesAsync();

            if(res > 0)
            {
                return Convert(userRes.Entity);
            }

            return null;
        }

        public async Task<TelegramUser?> ChangeUserStateAsync(long userId, TelegramUserState state)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            user.State = (int)state;

            var res = await _context.SaveChangesAsync();

            if (res > 0)
            {
                return Convert(user);
            }

            return null;
        }

        public async Task<bool> ChangeUserIsActiveByIdAsync(long userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            user.IsActive = true;

            var res = await _context.SaveChangesAsync();
            return res > 0;
        }

        public async Task<TelegramUser?> ChangeUserTempDataAsync(long userId, string tempData)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            user.UserTempData = tempData;

            var res = await _context.SaveChangesAsync();

            if (res > 0)
            {
                return Convert(user);
            }

            return null;
        }

        public async Task<bool> AddSubscriptionToUserAsync(long userId, string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var userNames = JsonConvert.DeserializeObject<List<string>>(user?.UsernamesJson ?? string.Empty) ?? new List<string>();

            userNames.Add(username);

            user.UsernamesJson = JsonConvert.SerializeObject(userNames);

            var res = await _context.SaveChangesAsync();
            return res > 0;
        }

        public async Task<bool> RemoveSubscriptionFromUserAsync(long userId, string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var userNames = JsonConvert.DeserializeObject<List<string>>(user?.UsernamesJson ?? string.Empty) ?? new List<string>();

            userNames.Remove(username);

            user.UsernamesJson = JsonConvert.SerializeObject(userNames);

            var res = await _context.SaveChangesAsync();
            return res > 0;
        }

        private TelegramUser? Convert(TelegramUserDbo? dbo)
        {
            if (dbo == null)
            {
                return null;
            }

            var res = _mapper.Map<TelegramUser>(dbo);
            res.Usernames = JsonConvert.DeserializeObject<List<string>>(dbo.UsernamesJson) ?? new List<string>();

            return res;
        }
    }
}
