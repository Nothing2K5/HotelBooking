using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;
namespace HotelBooking.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("admin/booking")]
    public class BookingController : Controller
    {
        private readonly DatabaseDataContext _db;
        public BookingController()
        {
            _db = new DatabaseDataContext();
        }
        // GET: Admin/Booking/Index
        public ActionResult Index()
        {
            return View();
        }
        // GET: Admin/Booking/GetAllBookings - AJAX
        [HttpGet]
        public ActionResult GetAllBookings()
        {
            try
            {
                var rawBookings = _db.Bookings
                                .Join(_db.Customers,
                                b => b.UserId,
                                c => c.UserId,
                                (b, c) => new { b, c })
                                .Join(_db.Users,
                                bc => bc.c.UserId,
                                u => u.Id,
                                (bc, u) => new
                                {
                                    bc.b.Id,
                                    bc.b.Code,
                                    CustomerName = bc.c.FullName,
                                    CustomerEmail = u.Email,
                                    HotelName = bc.b.Hotel.Name,
                                    bc.b.CheckInDate,
                                    bc.b.CheckOutDate,
                                    bc.b.TotalAmount,
                                    bc.b.Guests,
                                    bc.b.Status,
                                    bc.b.CreatedAt
                                })
                                .OrderByDescending(x => x.CreatedAt)
                                .ToList();

                var bookings = rawBookings.Select(b => new
                {
                    b.Id,
                    b.Code,
                    b.CustomerName,
                    b.CustomerEmail,
                    b.HotelName,
                    CheckInDate = b.CheckInDate.ToString("MM/dd/yy"),
                    CheckOutDate = b.CheckOutDate.ToString("MM/dd/yy"),
                    b.TotalAmount,
                    b.Guests,
                    b.Status,
                    CreatedAt = b.CreatedAt.ToString("MM/dd/yy HH:mm")
                }).ToList();
                return Json(new { success = true, data = bookings }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        // GET: Admin/Booking/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var booking = _db.Bookings.FirstOrDefault(b => b.Id == id);
                if (booking == null)
                    return HttpNotFound();
                return View(booking);
            }
            catch
            {
                return HttpNotFound();
            }
        }
        // GET: Admin/Booking/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.BookingId = id;
            return View();
        }
        // GET: Admin/Booking/GetBookingById/5 - AJAX
        [HttpGet]
        public ActionResult GetBookingById(int id)
        {
            try
            {
                var booking = _db.Bookings
                .Where(b => b.Id == id)
                .Select(b => new
                {
                    b.Id,
                    b.Code,
                    b.Status,
                    b.Note,
                    // Format ngày cho modal edit
                    CheckInDate = b.CheckInDate.ToString("MM/dd/yy"),
                    CheckOutDate = b.CheckOutDate.ToString("MM/dd/yy"),
                    b.Guests
                })
                .FirstOrDefault();
                if (booking == null)
                    return Json(new { success = false, message = "Không tìm thấy booking" }, JsonRequestBehavior.AllowGet);
                return Json(new { success = true, data = booking }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetAllHotels()
        {
            try
            {
                var hotels = _db.Hotels
                .Where(h => h.IsActive == true) // Chỉ lấy khách sạn đang hoạt động
                                    .Select(h => new
                                    {
                                        h.Id,
                                        h.Name
                                    })
                .OrderBy(h => h.Name)
                .ToList();
                return Json(new { success = true, data = hotels }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        // POST: Admin/Booking/UpdateBooking - AJAX
        [HttpPost]
        public ActionResult UpdateBooking(Booking model)
        {
            try
            {
                var booking = _db.Bookings.FirstOrDefault(b => b.Id == model.Id);
                if (booking != null)
                {
                    booking.Status = model.Status;
                    booking.Note = model.Note;
                    booking.UpdatedAt = DateTime.Now;
                    _db.SubmitChanges();
                    return Json(new { success = true, message = "Cập nhật thành công!" });
                }
                return Json(new { success = false, message = "Không tìm thấy booking" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}