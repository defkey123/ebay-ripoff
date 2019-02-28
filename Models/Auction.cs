using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeltExam2Auctions.Models
{
    public class Auction
    {
        [Key]
        public int AuctionId {get;set;}

        [Required]
        [MinLength(2)]
        public string Title { get; set; }

        [Required]
        [MinLength(10)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [NotMapped]
        public TimeSpan TimeRemaining
        {
            get
            {
                return Date - DateTime.Now;
            }
        }


        [Required]
        public double StartingBid { get; set; }

        public int CreatorId { get; set; }
        public User Creator { get; set; }

        public List<Bid> Bids { get; set; }
    }

}