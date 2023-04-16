using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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

        public async Task<List<long>?> GetUserFriendIdsByUsernameAsync(string username)
        {
            var url = string.Format(TwitterHelper.UserFriendIdsByUsernameUrl, username);
            var respStr = await SendRequestAsync(url);

            if (string.IsNullOrEmpty(respStr))
            {
                return null;
            }
            return ParseUserFriendIds(respStr);
        }

        public async Task<List<long>?> GetUserFriendsByUsernameAsync(string username)
        {
            var url = string.Format(TwitterHelper.UserIFriendsByUsernameUrl, username);
            var respStr = await SendRequestAsync(url);

            if (string.IsNullOrEmpty(respStr))
            {
                return null;
            }
            return ParseUserFriends(respStr);
        }

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
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                   new AuthenticationHeaderValue("Bearer", _options.Token);
                client.DefaultRequestHeaders.Add("User-Agent", ".NET Telegram Bot");

                var resp = await client.GetAsync(string.Format(url));

                if (!resp.IsSuccessStatusCode)
                {
                    return null;
                }

                return await resp.Content.ReadAsStringAsync();
            }
        }

        private TwitterUser ParseUserData(string data)
        {
            var jObj = JObject.Parse(data);

            var id = (long)jObj["id"];
            var userName = (string)jObj["screen_name"];

            return new TwitterUser()
            {
                UserId = id,
                Username = userName
            };
        }

        private List<long> ParseUserFriendIds(string data)
        {
            var jObj = JObject.Parse(data);

            var jArray = (JArray)jObj["ids"];

            return jArray.Select(j => long.Parse(j.ToString())).ToList();
        }

        private List<long> ParseUserFriends(string data)
        {
            var jObj = JObject.Parse(data);

            var jArray = (JArray)jObj["users"];

            return jArray.Select(j => long.Parse(j["id"].ToString())).ToList();
        }
    }
}
