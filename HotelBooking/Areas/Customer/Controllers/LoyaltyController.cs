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
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);

                // Lấy customer kèm LoyaltyTierId và TotalPoints
                var customer = _db.Customers
                    .Where(c => c.UserId == userId)
                    .Select(c => new
                    {
                        c.TotalPoints,
                        c.FullName,
                        c.LoyaltyTierId
                    })
                    .FirstOrDefault();

                // Lấy tất cả tiers có MinPoints (ORDER BY MinPoints)
                var allTiers = _db.LoyaltyTiers
                    .OrderBy(t => t.MinPoints)
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        t.DiscountPercent,
                        t.Multiplier,
                        t.MinPoints
                    })
                    .ToList();

                // Nếu không có customer thì trả mặc định
                int totalPoints = customer?.TotalPoints ?? 0;

                // Xác định current tier:
                object currentTierObj = null;
                if (customer != null)
                {
                    // ưu tiên dùng LoyaltyTierId nếu có
                    if (customer.LoyaltyTierId != null)
                    {
                        var ct = allTiers.FirstOrDefault(t => t.Id == customer.LoyaltyTierId);
                        if (ct != null)
                        {
                            currentTierObj = ct;
                        }
                    }

                    // nếu vẫn chưa có currentTier, tìm theo điểm (last tier có MinPoints <= totalPoints)
                    if (currentTierObj == null && allTiers.Any())
                    {
                        var tierByPoints = allTiers.LastOrDefault(t => t.MinPoints <= totalPoints);
                        if (tierByPoints != null)
                        {
                            currentTierObj = tierByPoints;
                        }
                    }
                }

                // Trả về JSON hợp lệ
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        UserName = customer?.FullName ?? user?.Email ?? "Thành viên",
                        UserEmail = user?.Email,
                        TotalPoints = totalPoints,
                        currentTier = currentTierObj,
                        allTiers = allTiers
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

                // Vì bảng LoyaltyPoints đã bị bỏ, ta truy xuất lịch sử dựa trên các Booking đã thanh toán (paid)
                // Công thức giả định: 100.000 VNĐ = 1 điểm (làm tròn xuống)
                var history = _db.Bookings
                    .Where(b => b.UserId == userId && b.Status == "paid") // Chỉ lấy đơn đã thanh toán
                    .OrderByDescending(b => b.UpdatedAt)
                    .AsEnumerable() // Chuyển xử lý về client side LINQ (để dùng .ToString trên DateTime)
                    .Select(b => new
                    {
                        // Trả về chuỗi đã format để frontend hiển thị trực tiếp
                        CreatedAt = (b.UpdatedAt != DateTime.MinValue ? b.UpdatedAt : (DateTime?)b.CheckOutDate)
                                        .HasValue
                                        ? (b.UpdatedAt != DateTime.MinValue ? b.UpdatedAt : (DateTime?)b.CheckOutDate).Value.ToString("dd/MM/yyyy HH:mm")
                                        : string.Empty,

                        // Tính điểm: floor(TotalAmount / 100000)
                        Points = (int)Math.Floor((decimal)(b.TotalAmount / 100000m)),

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