using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Customer.Controllers
{
    [Authorize(Roles = "customer")]
    public class LoyaltyController : Controller
    {
        private readonly DatabaseDataContext _db;

        public LoyaltyController()
        {
            _db = new DatabaseDataContext();
        }

        private int GetCurrentUserId()
        {
            var email = User.Identity.Name;
            return _db.Users.First(u => u.Email == email).Id;
        }

        // GET: Customer/Loyalty/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Customer/Loyalty/GetLoyaltyInfo - AJAX
        [HttpGet]
        public ActionResult GetLoyaltyInfo()
        {
            try
            {
                var userId = GetCurrentUserId();
                var customer = _db.Customers
                    .Where(c => c.UserId == userId)
                    .Select(c => new
                    {
                        c.TotalPoints,
                        CurrentTier = c.LoyaltyTier != null ? new
                        {
                            c.LoyaltyTier.Name,
                            c.LoyaltyTier.DiscountPercent,
                            c.LoyaltyTier.Multiplier
                        } : null
                    })
                    .FirstOrDefault();

                var allTiers = _db.LoyaltyTiers
                    .OrderBy(t => t.DiscountPercent)
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        t.DiscountPercent,
                        t.Multiplier
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        customer.TotalPoints,
                        currentTier = customer.CurrentTier,
                        allTiers
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}