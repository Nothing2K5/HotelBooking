using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Customer.Controllers
{
    // Cho phép xem khuyến mãi mà không cần đăng nhập (tùy nhu cầu của bạn)
    // Nếu muốn bắt buộc đăng nhập thì bỏ comment dòng dưới
    [Authorize(Roles = "customer")]
    public class PromotionController : Controller
    {
        private readonly DatabaseDataContext _db;

        public PromotionController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Customer/Promotion/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Customer/Promotion/GetPromotions - AJAX
        [HttpGet]
        public ActionResult GetPromotions(string query = "")
        {
            try
            {
                var today = DateTime.Now.Date;

                // Logic lọc:
                // 1. IsActive = true
                // 2. StartDate <= hôm nay <= EndDate
                // 3. (Tùy chọn) Tìm theo Mã hoặc Mô tả

                var promotions = _db.Promotions.AsQueryable();

                // Lọc cơ bản
                promotions = promotions.Where(p =>
                    p.IsActive == true &&
                    (p.StartDate == null || p.StartDate <= today) &&
                    (p.EndDate == null || p.EndDate >= today)
                );

                // Lọc theo từ khóa tìm kiếm
                if (!string.IsNullOrEmpty(query))
                {
                    query = query.ToLower().Trim();
                    promotions = promotions.Where(p =>
                        p.Code.ToLower().Contains(query) ||
                        p.Description.ToLower().Contains(query)
                    );
                }

                var result = promotions
                    .OrderBy(p => p.EndDate) // Ưu tiên mã sắp hết hạn hiển thị trước
                    .Select(p => new
                    {
                        p.Id,
                        p.Code,
                        p.Description,
                        p.Type, // 'percent' hoặc 'amount'
                        p.Value,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate
                    })
                    .ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}