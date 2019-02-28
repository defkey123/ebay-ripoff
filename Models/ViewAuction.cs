namespace BeltExam2Auctions.Models
{
    public class ViewAuction
    {
        public Auction Auction { get; set; }
        public Bid Bid { get; set; }
        public User CurrentUser { get; set; }
    }
}