using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Twitter_Telegram.Telegram
{
    public static class DIExtensions
    {
        public static T GetConfiguration<T>(this IServiceProvider serviceProvider)
            where T : class
        {
            var o = serviceProvider.GetService<IOptions<T>>();
            if (o is null)
                throw new ArgumentNullException(nameof(T));

            return o.Value;
        }
    }
}
