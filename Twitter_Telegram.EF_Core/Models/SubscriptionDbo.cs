using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twitter_Telegram.EF_Core.Models
{
    public class SubscriptionDbo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Username { get; set; }

        public List<long> Friends { get; set; }
    }
}
