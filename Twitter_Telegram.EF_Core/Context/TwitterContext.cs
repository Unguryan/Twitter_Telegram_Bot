using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
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

            //var userNamesConverter = new ValueConverter<List<string>, string>(
            //i => JsonConvert.SerializeObject(i),
            //s => JsonConvert.DeserializeObject<List<string>>(s) ?? new List<string>());

            //var friendsConverter = new ValueConverter<List<long>, string>(
            //i => JsonConvert.SerializeObject(i),
            //s => JsonConvert.DeserializeObject<List<long>>(s) ?? new List<long>());

            //modelBuilder.Entity<TelegramUserDbo>().Property(x => x.Usernames).HasConversion(userNamesConverter);
            //modelBuilder.Entity<SubscriptionDbo>().Property(x => x.Friends).HasConversion(friendsConverter);
        }
    }
}
