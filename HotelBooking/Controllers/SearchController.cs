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
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                if (model.CheckOutDate <= model.CheckInDate)
                    return Json(new { success = false, message = "Ngày trả phòng phải sau ngày nhận phòng" });

                var hotels = _db.Hotels
                    .Where(h => h.IsActive &&
                           (h.City.Contains(model.Location) ||
                            h.Name.Contains(model.Location) ||
                            h.Address.Contains(model.Location)))
                    .Select(h => new
                    {
                        h.Id,
                        h.Name,
                        h.Address,
                        h.City,
                        h.Country,
                        h.StarRating,
                        h.Description,
                        ImageUrl = h.HotelImages.Any() ? h.HotelImages.FirstOrDefault().Url : null
                    })
                    .ToList();

                return Json(new { success = true, hotels = hotels });
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
                        MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.PricePerNight) : 0,
                        AvgRating = h.Reviews.Any() ? h.Reviews.Average(r => (decimal?)r.Rating) : null,
                        ReviewCount = h.Reviews.Count(r => r.DeletedAt == null)
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