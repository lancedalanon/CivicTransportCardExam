using Microsoft.EntityFrameworkCore;
using CivicTransportCard.Models;

#pragma warning disable CS1591
namespace CivicTransportCard.Data
{
    public class CardContext : DbContext
    {
        public CardContext(DbContextOptions<CardContext> options)
            : base(options) { }

        public DbSet<Card> Cards { get; set; } = null!;
    }
}
#pragma warning restore CS1591