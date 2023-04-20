using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.Domain.Config;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.Infrastructure.Services
{
    public class ApiReader : IApiReader
    {
        private readonly TwitterOptions _options;
        private readonly ILogger _logger;

        public ApiReader(IOptions<TwitterOptions> options, ILogger<ApiReader> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<GetUserFriendIdsResultViewModel> GetUserFriendIdsByUsernameAsync(string username, int count)
        {
            if (count <= 5000)
            {
                var url = string.Format(TwitterHelper.UserFriendIdsByUsernameUrl, username);
                var respStr = await SendRequestAsync(url);

                if (string.IsNullOrEmpty(respStr))
                {
                    return new GetUserFriendIdsResultViewModel()
                    {
                        IsOut = false,
                        IsFound = false,
                        FriendIds = null
                    };
                }

                if(respStr == "Too Many Requests")
                {
                    return new GetUserFriendIdsResultViewModel()
                    {
                        IsOut = true,
                        IsFound = false,
                        FriendIds = null
                    };
                }

                var resList = ParseUserFriendIds(respStr);

                return new GetUserFriendIdsResultViewModel()
                {
                    IsFound = true,
                    IsOut = false,
                    FriendIds = resList
                };
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

                if (respStr == "Too Many Requests")
                {
                    return new GetUserFriendIdsResultViewModel()
                    {
                        IsOut = true,
                        IsFound = false,
                        FriendIds = null
                    };
                }

                var data = ParseUserFriendIdsWithNextCursor(respStr, out nextCursor);

                res.AddRange(data);
            }
            while (nextCursor != 0);

            return new GetUserFriendIdsResultViewModel()
            {
                IsFound = true,
                IsOut = false,
                FriendIds = res
            };
        }

        public async Task<GetUserInfoResultViewModel> GetUserInfoByUserIdAsync(string userId)
        {
            var url = string.Format(TwitterHelper.UserInfoByIdUrl, userId);
            var respStr = await SendRequestAsync(url);

            if (string.IsNullOrEmpty(respStr))
            {
                return new GetUserInfoResultViewModel()
                {
                    IsOut = false,
                    IsFound = false,
                    TwitterUser = null
                };
            }

            if (respStr == "Too Many Requests")
            {
                return new GetUserInfoResultViewModel()
                {
                    IsOut = true,
                    IsFound = false,
                    TwitterUser = null
                };
            }

            var user = ParseUserData(respStr);
            return new GetUserInfoResultViewModel()
            {
                IsOut = false,
                IsFound = true,
                TwitterUser = user
            };
        }

        public async Task<GetUserInfoResultViewModel> GetUserInfoByUsernameAsync(string username)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var url = string.Format(TwitterHelper.UserInfoByUsernameUrl, username);
            var respStr = await SendRequestAsync(url);

            if (string.IsNullOrEmpty(respStr))
            {
                return new GetUserInfoResultViewModel()
                {
                    IsOut = false,
                    IsFound = false,
                    TwitterUser = null
                };
            }

            if (respStr == "Too Many Requests")
            {
                return new GetUserInfoResultViewModel()
                {
                    IsOut = true,
                    IsFound = false,
                    TwitterUser = null
                };
            }

            var user = ParseUserData(respStr);
            return new GetUserInfoResultViewModel()
            {
                IsOut = false,
                IsFound = true,
                TwitterUser = user
            };
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
                            _logger.LogError($"{DateTime.Now.ToShortTimeString()}:" + $"To Many Requests\n{url}");
                            return "Too Many Requests";
                        }
                        if (resp.ReasonPhrase == "Not Found")
                        {
                            _logger.LogError($"{DateTime.Now.ToShortTimeString()}:" + $"NotFound\n{url}");
                            return null;
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}:" + $"Success Read: {url}");
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
