using System;
using Microsoft.EntityFrameworkCore;

namespace BeltExam2Auctions.Models
{
    public class AuctionsContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public AuctionsContext(DbContextOptions<AuctionsContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
    }
}
