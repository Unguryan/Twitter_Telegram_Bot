using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twitter_Telegram.Telegram.Abstraction;

namespace Twitter_Telegram.Telegram.Services
{
    public class PollingService : PollingServiceBase<ReceiverService>
    {
        public PollingService(IServiceProvider serviceProvider, ILogger<PollingService> logger)
            : base(serviceProvider, logger)
        {
        }
    }
}
