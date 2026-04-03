using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Domains.Models;

namespace WebApplication1.Domains
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        private DbSet<User> Users { get; set; }
        private DbSet<Transaction> Transactions { get; set; }
        private DbSet<Card> Cards { get; set; }
        private DbSet<Account> Accounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseNpgsql("Name=DefaultConnection");

    }
}
