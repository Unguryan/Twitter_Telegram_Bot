using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Twitter_Telegram.EF_Core.Models
{
    public class TelegramUserDbo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public int State { get; set; }

        public bool IsActive { get; set; }

        public List<string> UserNames { get; set; }
    }
}
