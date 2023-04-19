using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.Domain.Config;
using Twitter_Telegram.Domain.Models;

namespace Twitter_Telegram.Infrastructure.Services
{
    public class ApiReader : IApiReader
    {
        private readonly TwitterOptions _options;

        public ApiReader(IOptions<TwitterOptions> options)
        {
            _options = options.Value;
        }

        public async Task<List<long>?> GetUserFriendIdsByUsernameAsync(string username, int count)
        {
            if (count <= 5000)
            {
                var url = string.Format(TwitterHelper.UserFriendIdsByUsernameUrl, username);
                var respStr = await SendRequestAsync(url);

                if (string.IsNullOrEmpty(respStr))
                {
                    return null;
                }
                return ParseUserFriendIds(respStr);
            }

            long nextCursor = 0;

            var res = new List<long>();

            do
            {
                var url = string.Format(TwitterHelper.UserFriendIdsByUsernameUrl, username);

                url += nextCursor != 0 ? $"&cursor={nextCursor}" : string.Empty;

                var respStr = await SendRequestAsync(url);

                if (string.IsNullOrEmpty(respStr))
                {
                    break;
                }

                var data = ParseUserFriendIdsWithNextCursor(respStr, out nextCursor);

                res.AddRange(data);
            }
            while (nextCursor != 0);

            return res;
        }

        //public async Task<List<long>?> GetUserFriendsByUsernameAsync(string username)
        //{
        //    var url = string.Format(TwitterHelper.UserIFriendsByUsernameUrl, username);
        //    var respStr = await SendRequestAsync(url);

        //    if (string.IsNullOrEmpty(respStr))
        //    {
        //        return null;
        //    }
        //    return ParseUserFriends(respStr);
        //}

        public async Task<TwitterUser?> GetUserInfoByUserIdAsync(string userId)
        {
            var url = string.Format(TwitterHelper.UserInfoByIdUrl, userId);
            var respStr = await SendRequestAsync(url);

            if (string.IsNullOrEmpty(respStr))
            {
                return null;
            }
            return ParseUserData(respStr);
        }

        public async Task<TwitterUser?> GetUserInfoByUsernameAsync(string username)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var url = string.Format(TwitterHelper.UserInfoByUsernameUrl, username);
            var respStr = await SendRequestAsync(url);

            if (string.IsNullOrEmpty(respStr))
            {
                return null;
            }
            return ParseUserData(respStr);
        }

        private async Task<string?> SendRequestAsync(string url)
        {
            while (true)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                       new AuthenticationHeaderValue("Bearer", _options.Token);
                    client.DefaultRequestHeaders.Add("User-Agent", ".NET Telegram Bot");

                    var resp = await client.GetAsync(string.Format(url));

                    if (!resp.IsSuccessStatusCode)
                    {
                        if (resp.ReasonPhrase == "Too Many Requests")
                        {
                            await Task.Delay(TimeSpan.FromMinutes(15));
                        }
                        if (resp.ReasonPhrase == "Not Found")
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return await resp.Content.ReadAsStringAsync();
                    }
                }
            }

        }

        private TwitterUser ParseUserData(string data)
        {
            var jObj = JObject.Parse(data);

            var id = (long)jObj["id"];
            var userName = (string)jObj["screen_name"];
            var friendsCount = (int)jObj["friends_count"];

            return new TwitterUser()
            {
                UserId = id,
                Username = userName,
                FriendsCount = friendsCount
            };
        }

        private List<long> ParseUserFriendIds(string data)
        {
            var jObj = JObject.Parse(data);

            var jArray = (JArray)jObj["ids"];

            return jArray.Select(j => long.Parse(j.ToString())).ToList();
        }

        private List<long> ParseUserFriendIdsWithNextCursor(string data, out long nextCursor)
        {
            var jObj = JObject.Parse(data);

            var jArray = (JArray)jObj["ids"];
            nextCursor = (long)jObj["next_cursor"];

            return jArray.Select(j => long.Parse(j.ToString())).ToList();
        }
    }
}
