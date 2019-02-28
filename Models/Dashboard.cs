using System.Collections.Generic;

namespace BeltExam2Auctions.Models
{
    public class Dashboard
    {
        public User CurrentUser { get; set; }
        public List<Auction> Auctions { get; set; }
    }
}