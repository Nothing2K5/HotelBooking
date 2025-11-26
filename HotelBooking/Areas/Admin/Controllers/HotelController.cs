using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class HotelController : Controller
    {
        private readonly DatabaseDataContext _db;

        public HotelController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Admin/Hotel/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Hotel/GetAllHotels - AJAX
        [HttpGet]
        public ActionResult GetAllHotels()
        {
            try
            {
                var hotels = _db.Hotels
                    .Select(h => new
                    {
                        h.Id,
                        h.Name,
                        h.City,
                        h.Country,
                        h.StarRating,
                        h.IsActive
                    })
                    .ToList();

                return Json(new { success = true, data = hotels }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/Hotel/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Hotel/CreateHotel - AJAX
        [HttpPost]
        public ActionResult CreateHotel(Hotel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                model.IsActive = true;
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;

                _db.Hotels.InsertOnSubmit(model);
                _db.SubmitChanges();

                return Json(new { success = true, message = "Thêm khách sạn thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/Hotel/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.HotelId = id;
            return View();
        }

        // GET: Admin/Hotel/GetHotelById/5 - AJAX
        [HttpGet]
        public ActionResult GetHotelById(int id)
        {
            try
            {
                var hotel = _db.Hotels
                    .Where(h => h.Id == id)
                    .Select(h => new
                    {
                        h.Id,
                        h.Name,
                        h.Address,
                        h.City,
                        h.Country,
                        h.StarRating,
                        h.Description,
                        h.IsActive
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

        // POST: Admin/Hotel/UpdateHotel - AJAX
        [HttpPost]
        public ActionResult UpdateHotel(Hotel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                var hotel = _db.Hotels.FirstOrDefault(h => h.Id == model.Id);
                if (hotel == null)
                    return Json(new { success = false, message = "Không tìm thấy khách sạn" });

                if (!model.IsActive)
                {
                    bool hasActiveBooking = _db.Bookings.Any(b => b.HotelId == model.Id &&
                        (b.Status == "pending" ||
                         b.Status == "confirmed" ||
                         b.Status == "paid" ||
                         b.Status == "cancelled" ||
                         b.Status == "draft") &&
                        (b.CheckOutDate == null || b.CheckOutDate >= DateTime.Today));

                    if (hasActiveBooking)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Không thể ngừng hoạt động khách sạn vì vẫn còn phòng đang được đặt hoặc đang có khách lưu trú!"
                        });
                    }
                }

                hotel.Name = model.Name;
                hotel.Address = model.Address;
                hotel.City = model.City;
                hotel.Country = model.Country;
                hotel.StarRating = model.StarRating;
                hotel.Description = model.Description;
                hotel.IsActive = model.IsActive;
                hotel.UpdatedAt = DateTime.Now;

                _db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/Hotel/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var hotel = _db.Hotels.FirstOrDefault(h => h.Id == id);
                if (hotel == null)
                    return HttpNotFound();

                var images = _db.HotelImages
                    .Where(i => i.HotelId == id)
                    .ToList();

                var rooms = _db.Rooms
                    .Where(r => r.HotelId == id)
                    .ToList();

                var roomImages = _db.RoomImages.ToList();

                var reviews = _db.Reviews
                    .Where(r => r.HotelId == id && r.DeletedAt == null)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                double avgRating = reviews.Count > 0
                    ? reviews.Average(r => r.Rating)
                    : 0;

                ViewBag.HotelImages = images;
                ViewBag.Rooms = rooms;
                ViewBag.RoomImages = roomImages;
                ViewBag.Reviews = reviews;
                ViewBag.AvgRating = avgRating;

                return View(hotel);
            }
            catch
            {
                return HttpNotFound();
            }
        }


        // POST: Admin/Hotel/DeleteHotel - AJAX
        [HttpPost]
        public ActionResult DeleteHotel(int id)
        {
            try
            {
                var hotel = _db.Hotels.FirstOrDefault(h => h.Id == id);
                if (hotel != null)
                {
                    // Check if hotel has active bookings
                    var hasActiveBookings = _db.Bookings.Any(b => b.HotelId == id &&
                                                                  (b.Status == "paid" || b.Status == "confirmed"));

                    if (hasActiveBookings)
                        return Json(new { success = false, message = "Không thể xóa khách sạn có booking đang hoạt động" });

                    hotel.IsActive = false;
                    hotel.DeletedAt = DateTime.Now;
                    _db.SubmitChanges();

                    return Json(new { success = true, message = "Xóa thành công" });
                }
                return Json(new { success = false, message = "Không tìm thấy khách sạn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}