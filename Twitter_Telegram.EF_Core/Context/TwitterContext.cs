using Microsoft.EntityFrameworkCore;
using Twitter_Telegram.EF_Core.Models;

namespace Twitter_Telegram.EF_Core.Context
{
    public class TwitterContext : DbContext
    {
        public TwitterContext(DbContextOptions<TwitterContext> options) : base(options)
        {
        }

        public DbSet<TelegramUserDbo> Users { get; set; }

        public DbSet<SubscriptionDbo> Subscriptions { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=TwitterApp.db");

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TelegramUserDbo>().ToTable("Users");
            modelBuilder.Entity<SubscriptionDbo>().ToTable("Subscriptions");
        }
    }
}
