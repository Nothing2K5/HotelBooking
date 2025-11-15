using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
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
        //[HttpGet]
        //public ActionResult GetAllBookings()
        //{
        //    try
        //    {
        //        var bookings = _db.Bookings
        //            .OrderByDescending(b => b.CreatedAt)
        //            .Select(b => new
        //            {
        //                b.Id,
        //                b.Code,
        //                CustomerName = b.User.Customers.FirstOrDefault().FullName,
        //                CustomerEmail = b.User.Email,
        //                HotelName = b.Hotel.Name,
        //                b.CheckInDate,
        //                b.CheckOutDate,
        //                b.Guests,
        //                b.TotalAmount,
        //                b.Status,
        //                b.CreatedAt
        //            })
        //            .ToList();

        //        return Json(new { success = true, data = bookings }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

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
                        b.CheckInDate,
                        b.CheckOutDate,
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