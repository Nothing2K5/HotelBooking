using HotelBooking.Models;
using HotelBooking.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Controllers
{
    public class SearchController : Controller
    {
        private readonly DatabaseDataContext _db;

        public SearchController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Search/Index
        public ActionResult Index()
        {
            return View();
        }

        // POST: Search/SearchHotels - AJAX
        [HttpPost]
        public ActionResult SearchHotels(HotelSearchVM model)
        {
            try
            {
                // Lấy giá trị location an toàn (nếu model == null hoặc Location null)
                var location = model?.Location?.Trim() ?? string.Empty;

                // Lấy danh sách Hotel phù hợp địa điểm (nếu location rỗng -> trả tất cả các hotel active)
                var query = _db.Hotels.Where(h => h.IsActive &&
                           (string.IsNullOrEmpty(location) ||
                            h.City.Contains(location) ||
                            h.Name.Contains(location) ||
                            h.Address.Contains(location)));

                // Lọc Hotel có ít nhất 1 phòng active (không kiểm tra booking theo ngày nữa)
                var availableHotels = query
                    .Where(h => h.Rooms.Any(r => r.IsActive == true))
                    .Select(h => new
                    {
                        h.Id,
                        h.Name,
                        h.Address,
                        h.City,
                        h.Country,
                        h.StarRating,
                        h.Description,
                        ImageUrl = h.HotelImages.Any() ? h.HotelImages.FirstOrDefault().Url : null,
                        // Giá thấp nhất trên các phòng active
                        MinPrice = h.Rooms.Where(r => r.IsActive == true).Any()
                                    ? h.Rooms.Where(r => r.IsActive == true).Min(r => (decimal?)r.PricePerNight)
                                    : 0,

                        // Tính rating từ Reviews (nếu có)
                        AvgRating = h.Bookings.SelectMany(b => b.Reviews).Any()
                                    ? h.Bookings.SelectMany(b => b.Reviews).Average(r => (decimal?)r.Rating)
                                    : null,

                        ReviewCount = h.Bookings.SelectMany(b => b.Reviews).Count(r => r.DeletedAt == null)
                    })
                    .ToList();

                return Json(new { success = true, hotels = availableHotels });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Search/GetAllHotels - Load tất cả khách sạn
        [HttpGet]
        public ActionResult GetAllHotels()
        {
            try
            {
                var hotels = _db.Hotels
                    .Where(h => h.IsActive)
                    .Select(h => new
                    {
                        h.Id,
                        h.Name,
                        h.Address,
                        h.City,
                        h.Country,
                        h.StarRating,
                        h.Description,
                        ImageUrl = h.HotelImages.Any() ? h.HotelImages.FirstOrDefault().Url : null,
                        MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => (decimal?)r.PricePerNight) : 0,

                        // Đi từ Hotel -> Bookings -> Reviews
                        AvgRating = h.Bookings.SelectMany(b => b.Reviews).Any()
                                    ? h.Bookings.SelectMany(b => b.Reviews).Average(r => (decimal?)r.Rating)
                                    : null,

                        ReviewCount = h.Bookings.SelectMany(b => b.Reviews).Count(r => r.DeletedAt == null)
                    })
                    .ToList();

                return Json(new { success = true, hotels = hotels }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
