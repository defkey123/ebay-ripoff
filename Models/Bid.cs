namespace BeltExam2Auctions.Models
{
    public class Bid
    {
        public int BidId { get; set; }
        public double Amount { get; set; }
        public int UserId { get; set; }
        public int AuctionId { get; set; }

        //navigation properties
        public User Bidder { get; set; }
        public Auction Auction { get; set; }
    }
}