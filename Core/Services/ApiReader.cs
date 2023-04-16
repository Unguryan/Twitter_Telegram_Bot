using Core.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace Core.Services
{
    public class ApiReader : IApiReader
    {
        public async Task<string> GetUserDataAsync(string username)
        {
            using (var client = new HttpClient())
            {
                //var token = await GetTokenAsync();

                var token = "AAAAAAAAAAAAAAAAAAAAAF8CmwEAAAAAHPy61k1nm4fvVxImkfwjUac1O%2Bk%3DCdGbnQ7IIdLeygEYHGaZTH87YGAhUeJ538gbQXfwNZgZnV7ziR";


                //var url = $"https://api.twitter.com/2/users/by/username/{username}";
                //var url = $"https://api.twitter.com/2/users/by?usernames={username}&user.fields=created_at,description";

                //var url = $"https://api.twitter.com/1.1/followers/list.json?cursor=-1&screen_name={username}&skip_status=true&include_user_entities=false";

                //gets user data
                //var url = $"https://api.twitter.com/1.1/users/show.json?screen_name={username}";

                //gets friend
                var url = $"https://api.twitter.com/1.1/friends/list.json?cursor=-1&screen_name={username}&skip_status=true&include_user_entities=false";
                
                //gets friend ids
                //var url = $"https://api.twitter.com/1.1/friends/ids.json?screen_name={username}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                client.DefaultRequestHeaders.Add("User-Agent", "C# App");

                var resp = await client.GetAsync(url);

                return await resp.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> GetTokenAsync()
        {
            using(var client = new HttpClient())
            {
                var url = $"https://api.twitter.com/oauth2/token?grant_type=client_credentials";

                var key = "NOKJ7sR3glOtzpIIS1izusULF";
                var secret = "h8FgLnrIuTaMluvppQ59cmhG0sJNooz2j9I2LRCkdy4X74SmFh";

                //var token = "AAAAAAAAAAAAAAAAAAAAAF8CmwEAAAAAHPy61k1nm4fvVxImkfwjUac1O%2Bk%3DCdGbnQ7IIdLeygEYHGaZTH87YGAhUeJ538gbQXfwNZgZnV7ziR";

                //dynamic basic = new System.Dynamic.ExpandoObject();

                //basic.username = key;
                //basic.password = secret;

                var basic = Base64Encode($"{key}:{secret}");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", basic);

                var bodyStr = "{\"grant_type\": \"client_credentials\"}";
                var body = new StringContent(bodyStr);

                var resp = await client.PostAsync(url, body);

                string token = string.Empty;

                if (!resp.IsSuccessStatusCode)
                {
                    return token;
                }

                var respStr = await resp.Content.ReadAsStringAsync();

                var obj = JObject.Parse(respStr);

                token = (string)obj["access_token"] ?? string.Empty;

                return token;
            }
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

       
    }
}
