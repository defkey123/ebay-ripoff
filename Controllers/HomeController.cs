using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using BeltExam2Auctions.Models;
using Microsoft.AspNetCore.Identity;

namespace BeltExam2Auctions.Controllers
{
    public class HomeController : Controller
    {
        private AuctionsContext dbContext;
        public HomeController(AuctionsContext context)
        {
            dbContext = context;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            return RedirectToAction("Auctions");
        }

        [HttpGet("login")]
        public IActionResult LoginReg()
        {
            int? userId = HttpContext.Session.GetInt32("logged_in_id");
            if (userId != null) return RedirectToAction("Auctions");

            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            // Check initial ModelState
            if (ModelState.IsValid)
            {
                // If a User exists with provided email
                if (dbContext.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");

                    return View("LoginReg");
                }
                PasswordHasher<User> hasher = new PasswordHasher<User>();

                user.Password = hasher.HashPassword(user, user.Password);
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("logged_in_id", user.UserId);
                return RedirectToAction("Auctions");
            }
            return View("LoginReg");
        }

        [HttpPost("confirmlogin")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.LoginEmail);
                // If no user exists with provided email
                if (userInDb is null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("LoginReg");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();

                // varify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.LoginPassword);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    return View("LoginReg");
                }
                else
                {
                    HttpContext.Session.SetInt32("logged_in_id", userInDb.UserId);
                    return RedirectToAction("Auctions");
                }
            }
            return View("LoginReg");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Auctions");
        }

        [HttpGet("home")]
        public IActionResult Auctions()
        {
            int? userId = HttpContext.Session.GetInt32("logged_in_id");
            if (userId is null) return RedirectToAction("LoginReg");

            var Auctions = dbContext.Auctions.ToList();
            foreach (Auction a in Auctions)
            {
                if (a.Date <= DateTime.Now)
                {
                    // transfer money to other user
                    dbContext.Auctions.Remove(a);
                    dbContext.SaveChanges();
                }
            }
            Dashboard dash = new Dashboard();
            dash.Auctions = dbContext.Auctions
                .Include(a => a.Creator)
                .Include(a => a.Bids)
                    .ThenInclude(b => b.Bidder)
                .ToList();
            dash.CurrentUser = dbContext.Users.FirstOrDefault(u => u.UserId == (int)userId);
            return View(dash);
        }

        [HttpGet("new")]
        public IActionResult NewAuction()
        {
            int? userId = HttpContext.Session.GetInt32("logged_in_id");
            if (userId is null) return RedirectToAction("LoginReg");

            return View();
        }

        [HttpPost("post_new_auction")]
        public IActionResult PostNewAuction(Auction auction)
        {
            int? userId = HttpContext.Session.GetInt32("logged_in_id");
            if (userId is null) return RedirectToAction("LoginReg");

            if (ModelState.IsValid)
            {
                int userIdInSession = HttpContext.Session.GetInt32("logged_in_id").Value;
                User userLoggedIn = dbContext.Users.FirstOrDefault(user => user.UserId == userIdInSession);
                auction.CreatorId = userIdInSession;
                auction.Creator = userLoggedIn;
                dbContext.Auctions.Add(auction);
                dbContext.SaveChanges();
                return RedirectToAction("Auctions");
            }
            return View("NewAuction");
        }

        [HttpGet("auction/{auctionId}")]
        public IActionResult ViewAuction(int auctionId)
        {
            int? userId = HttpContext.Session.GetInt32("logged_in_id");
            if (userId is null) return RedirectToAction("LoginReg");

            ViewBag.Name = HttpContext.Session.GetString("logged_in");
            ViewBag.loggedInId = HttpContext.Session.GetInt32("logged_in_id");

            Auction auction = dbContext.Auctions
                .Include(x => x.Creator)
                .Include(x => x.Bids)
                .ThenInclude(x => x.Bidder)
                .FirstOrDefault(a => a.AuctionId == auctionId);
            // ViewBag.CurrentHighestBidder = dbContext.Users.Where(u => u.UserId == viewingAuction.HighestBidderId).FirstOrDefault();

            ViewAuction thingy = new ViewAuction();
            thingy.CurrentUser = dbContext.Users.FirstOrDefault(u => u.UserId == userId);
            thingy.Auction = auction;
            thingy.Bid = new Bid();
            thingy.Bid.AuctionId = auction.AuctionId;
            thingy.Bid.UserId = (int)userId;
            return View(thingy);
        }

        [HttpPost("newbid")]
        public IActionResult NewBid(Bid bid)
        {
            int? userId = HttpContext.Session.GetInt32("logged_in_id");
            if (userId is null) return RedirectToAction("LoginReg");

            ViewBag.Name = HttpContext.Session.GetString("logged_in");
            User userLoggedIn = dbContext.Users.FirstOrDefault(user => user.UserId == userId);

            Auction auction = dbContext.Auctions
                .Include(x => x.Creator)
                .Include(x => x.Bids)
                .ThenInclude(x => x.Bidder)
                .FirstOrDefault(a => a.AuctionId == bid.AuctionId);

            if (ModelState.IsValid)
            {
                if (bid.Amount > userLoggedIn.CurrentWallet)
                {
                    ModelState.AddModelError("Bid", "You don't have enough money to bid!");
                    return View("ViewAuction", auction);
                }
                // if (bid < auction.CurrentHighestBid)
                // {
                //     ModelState.AddModelError("Bid", "Your bid is too low!");
                //     return View("ViewAuction", auction);
                // }
                
                dbContext.Bids.Add(bid);
                dbContext.SaveChanges();
                return RedirectToAction("Auctions");
            }
            // ViewBag.CurrentHighestBidder = dbContext.Users.Where(u => u.UserId == auction.HighestBidderId).FirstOrDefault();
            return View("ViewAuction", auction);
        }
    }
}