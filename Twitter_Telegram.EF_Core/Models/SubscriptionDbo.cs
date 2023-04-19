using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Twitter_Telegram.EF_Core.Models
{
    public class SubscriptionDbo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Username { get; set; }

        public string FriendsJson { get; set; }

        public bool IsInit { get; set; }

        public int? FriendsCount { get; set; }

        public DateTime? LastTimeChecked { get; set; }
    }
}
