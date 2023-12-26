using Microsoft.Extensions.Configuration;
using Twitter_Telegram.App.Services;

namespace Twitter_Telegram.Infrastructure.Services
{
    public class TwitterSettingsWriter : ITwitterSettingsWriter
    {
        private readonly IConfiguration _configuration;
        public TwitterSettingsWriter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task UpdateTwitterToken(string updatedToken)
        {
            _configuration["TwitterOptions:Token"] = updatedToken;

            var t = _configuration["TwitterOptions:Token"];
        }
    }
}
