using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Controllers
{
    public class HotelController : Controller
    {
        private readonly DatabaseDataContext _db;

        public HotelController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Hotel/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.HotelId = id;
            return View();
        }

        // GET: Hotel/GetDetails/5 - AJAX
        [HttpGet]
        public ActionResult GetDetails(int id)
        {
            try
            {
                var hotel = _db.Hotels
                    .Where(h => h.Id == id && h.IsActive)
                    .Select(h => new
                    {
                        h.Id,
                        h.Name,
                        h.Address,
                        h.City,
                        h.Country,
                        h.StarRating,
                        h.Description,
                        Images = h.HotelImages.Select(img => new
                        {
                            img.Url,
                            img.AltText
                        }).ToList()
                    })
                    .FirstOrDefault();

                if (hotel == null)
                    return Json(new { success = false, message = "Không tìm thấy khách sạn" }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = hotel }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Hotel/GetRooms/5 - AJAX
        [HttpGet]
        public ActionResult GetRooms(int id)
        {
            try
            {
                var rooms = _db.Rooms
                    .Where(r => r.HotelId == id && r.IsActive == true)
                    .Select(r => new
                    {
                        r.Id,
                        r.Name,
                        r.Description,
                        r.Capacity,
                        r.PricePerNight,
                        r.TotalRooms
                    })
                    .ToList();

                return Json(new { success = true, rooms = rooms }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Hotel/GetReviews/5 - AJAX
        [HttpGet]
        public ActionResult GetReviews(int id)
        {
            try
            {
                var reviews = _db.Reviews
                    .Where(r => r.HotelId == id && r.DeletedAt == null)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.Rating,
                        r.Content,
                        UserEmail = r.User.Email,
                        r.CreatedAt
                    })
                    .ToList();

                var avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

                return Json(new
                {
                    success = true,
                    reviews = reviews,
                    avgRating = avgRating
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}