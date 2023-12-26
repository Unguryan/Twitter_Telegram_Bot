using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text;
using Twitter_Telegram.App.Services;
using Twitter_Telegram.Domain.Config;
using Twitter_Telegram.Domain.Models;
using Twitter_Telegram.Domain.ViewModels;

namespace Twitter_Telegram.Infrastructure.Services
{
    public class ApiReaderV2 : IApiReaderV2
    {
        private readonly ILogger _logger;

        public ApiReaderV2(ILogger<ApiReader> logger)
        {
            _logger = logger;
        }

        public async Task<GetUserFriendIdsResultViewModel> GetUserFriendIdsByUsernameAsync(string username, int count)
        {
            //if (count <= 5000)
            //{
            //    var url = string.Format(TwitterHelper.UserFriendIdsWithCountByUsernameUrlV2, username, count);
            //    var respStr = await SendRequestAsync(url);

            //    if (!respStr.Item1)
            //    {
            //        if (respStr.Item2.Contains("You have exceeded the MONTHLY quota for Requests"))
            //        {
            //            return new GetUserFriendIdsResultViewModel()
            //            {
            //                IsOut = true,
            //                IsFound = false,
            //                FriendIds = null
            //            };
            //        }
            //    }

            //    if (string.IsNullOrEmpty(respStr.Item2))
            //    {
            //        return new GetUserFriendIdsResultViewModel()
            //        {
            //            IsOut = false,
            //            IsFound = false,
            //            FriendIds = null
            //        };
            //    }


            //    var resList = ParseUserFriendIds(respStr.Item2);

            //    return new GetUserFriendIdsResultViewModel()
            //    {
            //        IsFound = true,
            //        IsOut = false,
            //        FriendIds = resList
            //    };
            //}

            long nextCursor = 0;

            var res = new List<long>();

            do
            {
                var url = string.Format(TwitterHelper.UserFriendIdsByUsernameUrlV2, username, count);

                url += nextCursor != 0 ? $"&cursor={nextCursor}" : string.Empty;

                var respStr = await SendRequestAsync(url);

                if (string.IsNullOrEmpty(respStr.Item2))
                {
                    if(res.Count == 0)
                    {
                        return new GetUserFriendIdsResultViewModel()
                        {
                            IsFound = false,
                            IsOut = false,
                            IsAuthError = false,
                            FriendIds = null
                        };
                    }

                    break;
                }

                if (respStr.Item2.Contains("You have exceeded the MONTHLY quota for Requests"))
                {
                    return new GetUserFriendIdsResultViewModel()
                    {
                        IsOut = true,
                        IsFound = false,
                        IsAuthError = false,
                        FriendIds = null
                    };
                }

                var data = ParseUserFriendIdsWithNextCursor(respStr.Item2, out nextCursor);

                if(data == null)
                {
                    return new GetUserFriendIdsResultViewModel()
                    {
                        IsOut = false,
                        IsFound = false,
                        IsAuthError = true,
                        FriendIds = null
                    };
                }
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

        public async Task<List<GetUsersInfoResultViewModel>> GetUserInfoByUserIdAsync(List<string> userId)
        {
            var queries = PrepareUrl(userId);
            var result = new List<GetUsersInfoResultViewModel>();

            foreach (var query in queries)
            {
                var url = string.Format(TwitterHelper.UserInfoByIdUrlV2, query);
                var res = await GetUsersByUrlAsync(url);
                result.Add(res);
            }

            return result;

            //var url = string.Format(TwitterHelper.UserInfoByIdUrlV2, query);
            //return await GetUsersByUrlAsync(url);
        }

        public async Task<List<GetUsersInfoResultViewModel>> GetUserInfoByUsernameAsync(List<string> username)
        {
            var queries = PrepareUrl(username);
            var result = new List<GetUsersInfoResultViewModel>();

            foreach (var query in queries)
            {
                var url = string.Format(TwitterHelper.UserInfoByUsernameUrlV2, query);
                var res = await GetUsersByUrlAsync(url);
                result.Add(res);
            }

            return result;
            //var sb = new StringBuilder();
            //username.ForEach(s => sb.Append(s + "%2C"));
            //var url = string.Format(TwitterHelper.UserInfoByUsernameUrlV2, sb.ToString());
            //return await GetUsersByUrlAsync(url);

        }

        private async Task<GetUsersInfoResultViewModel> GetUsersByUrlAsync(string url)
        {
            var respStr = await SendRequestAsync(url);

            if (!respStr.Item1)
            {
                if(respStr.Item2.Contains("You have exceeded the MONTHLY quota for Requests"))
                {
                    return new GetUsersInfoResultViewModel()
                    {
                        IsOut = true,
                        IsFound = false,
                        TwitterUsers = null
                    };
                }
            }

            if (string.IsNullOrEmpty(respStr.Item2))
            {
                return new GetUsersInfoResultViewModel()
                {
                    IsOut = false,
                    IsFound = false,
                    TwitterUsers = null
                };
            }

            var users = ParseUserData(respStr.Item2);
            return new GetUsersInfoResultViewModel()
            {
                IsOut = false,
                IsFound = true,
                TwitterUsers = users
            };
        }

        private List<string> PrepareUrl(List<string> data)
        {
            if (data.Count <= 100)
            {
                var sb = new StringBuilder();
                data.ForEach(s => sb.Append(s + "%2C"));
                return new List<string>() { sb.ToString() };
            }
            else
            {
                var resultUrls = new List<string>();

                string[] buffer;
                var source = data.ToArray();

                for (int i = 0; i < data.Count; i += 100)
                {
                    var lenght = data.Count - i < 100 ? data.Count - i : 100;
                    buffer = new string[lenght];
                    Array.Copy(source, i, buffer, 0, lenght);

                    var sb = new StringBuilder();

                    foreach (var str in buffer)
                    {
                        
                        sb.Append(str + "%2C");
                    }

                    resultUrls.Add(sb.ToString());
                }
                return resultUrls;
            }
        }

        private async Task<(bool,string?)> SendRequestAsync(string url)
        {
            while (true)
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(url),
                        Headers =
                            {
                                //{ "X-RapidAPI-Key", "32e064ac12mshcdf633ea2be37ffp15daeejsnb920ca2a8456" },
                                { "X-RapidAPI-Key", "KEY" },
                                { "X-RapidAPI-Host", "twitter135.p.rapidapi.com" },
                            },
                    };

                    using (var response = await client.SendAsync(request))
                    {
                        var body = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}:" + $"ERROR Read: {url}");
                            _logger.LogWarning($"{DateTime.Now.ToShortTimeString()}:" + $"ERROR {body}");

                            if(body.Contains("You have exceeded the rate limit per second for your plan"))
                            {
                                await Task.Delay(TimeSpan.FromSeconds(15));
                                continue;
                            }

                            return (false, body);
                        }
                        else
                        {
                            _logger.LogInformation($"{DateTime.Now.ToShortTimeString()}:" + $"Success Read: {url}");
                            return (true, body);
                        }
                    }
                }
            }

        }

        private List<TwitterUser> ParseUserData(string data)
        {
            var result = new List<TwitterUser>();
            var jArray = JArray.Parse(data);
            foreach (var jObj in jArray)
            {
                var id = (long)jObj["id"];
                var userName = (string)jObj["screen_name"];
                var friendsCount = (int)jObj["friends_count"];

                var userToSave = new TwitterUser()
                {
                    UserId = id,
                    Username = userName,
                    FriendsCount = friendsCount
                };

                result.Add(userToSave);
            }

            //var jObj = JObject.Parse(data);

            //var id = (long)jObj["id"];
            //var userName = (string)jObj["screen_name"];
            //var friendsCount = (int)jObj["friends_count"];

            return result;
        }

        private List<long> ParseUserFriendIds(string data)
        {
            var jObj = JObject.Parse(data);

            var jArray = (JArray)jObj["ids"];

            return jArray.Select(j => long.Parse(j.ToString())).ToList();
        }

        private List<long>? ParseUserFriendIdsWithNextCursor(string data, out long nextCursor)
        {
            var jObj = JObject.Parse(data);

            if (jObj.ContainsKey("error") || jObj.ContainsKey("errors"))
            {
                nextCursor = 0;
                return null;
            }

            var jArray = (JArray)jObj["ids"];
            nextCursor = (long)jObj["next_cursor"];

            return jArray.Select(j => long.Parse(j.ToString())).ToList();
        }
    }
}
