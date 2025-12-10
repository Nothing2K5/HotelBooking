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
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            return user != null ? user.Id : 0;
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

        // --- BỔ SUNG: API Lấy lịch sử điểm ---
        [HttpGet]
        public ActionResult GetPointsHistory()
        {
            try
            {
                var userId = GetCurrentUserId();

                // Vì bảng LoyaltyPoints đã bị bỏ, ta truy xuất lịch sử dựa trên các Booking đã thanh toán (Paid)
                // Công thức giả định: 100.000 VNĐ = 1 điểm (có thể nhân hệ số Tier nếu muốn phức tạp hơn)
                var history = _db.Bookings
                    .Where(b => b.UserId == userId && b.Status == "paid") // Chỉ lấy đơn đã thanh toán
                    .OrderByDescending(b => b.UpdatedAt)
                    .AsEnumerable() // Chuyển xử lý về Client để tính toán phức tạp nếu cần
                    .Select(b => new
                    {
                        CreatedAt = b.UpdatedAt != DateTime.MinValue ? b.UpdatedAt : b.CheckOutDate,
                        // Logic tính điểm hiển thị lại (cần khớp với logic lúc cộng điểm)
                        Points = (int)(b.TotalAmount / 100000),
                        Reason = "Tích điểm từ đơn đặt phòng",
                        BookingCode = b.Code
                    })
                    .ToList();

                return Json(new { success = true, data = history }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}