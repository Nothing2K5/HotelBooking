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

        // GET: Admin/Admin/Booking/GetAllBookings - AJAX (ĐÃ SỬA ĐÚNG THEO DB MỚI)
        [HttpGet]
        public ActionResult GetAllBookings()
        {
            try
            {
                var bookings = _db.Bookings
                    .Where(b => b.DeletedAt == null) // Soft delete
                    .Join(_db.Users,
                        b => b.UserId,
                        u => u.Id,
                        (b, u) => new { b, u })
                    .Join(_db.Customers,
                        bu => bu.u.Id,           // UserId trong Customers trỏ về Users.Id
                        c => c.UserId,
                        (bu, c) => new { bu.b, bu.u, c })
                    .Select(x => new
                    {
                        x.b.Id,
                        x.b.Code,
                        CustomerName = x.c.FullName ?? "Chưa có tên",
                        CustomerEmail = x.u.Email,
                        CustomerPhone = x.c.Phone ?? "N/A",
                        HotelName = x.b.Hotel.Name,
                        RoomNumber = x.b.Room.RoomNumber,                    //Code phong
                        x.b.CheckInDate,
                        x.b.CheckOutDate,
                        x.b.Guests,
                        x.b.TotalAmount,
                        x.b.Status,
                        x.b.CreatedAt,
                        x.b.CancelledAt
                    })
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList();

                // Replace the switch expression with a standard switch statement for C# 7.3 compatibility
                var result = bookings.Select(b =>
                {
                    string statusText;
                    switch (b.Status)
                    {
                        case "paid":
                            statusText = "Paid";
                            break;
                        case "confirmed":
                            statusText = "Confirmed";
                            break;
                        case "pending":
                            statusText = "Pending";
                            break;
                        case "cancelled":
                            statusText = "Cancelled";
                            break;
                        case "draft":
                            statusText = "Draft";
                            break;
                        default:
                            statusText = "Unknown";
                            break;
                    }
                    return new
                    {
                        b.Id,
                        b.Code,
                        b.CustomerName,
                        b.CustomerEmail,
                        b.CustomerPhone,
                        b.HotelName,
                        b.RoomNumber,
                        CheckInDate = b.CheckInDate.ToString("MM/dd/yy"),
                        CheckOutDate = b.CheckOutDate.ToString("MM/dd/yy"),
                        b.Guests,
                        TotalAmount = b.TotalAmount,
                        Status = statusText,
                        CreatedAt = b.CreatedAt.ToString("MM/dd/yy HH:mm"),
                        IsCancelled = b.CancelledAt != null
                    };
                }).ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/Booking/GetAllHotels - giữ nguyên, đang đúng
        [HttpGet]
        public ActionResult GetAllHotels()
        {
            
            try
            {
                var hotels = _db.Hotels
                    .Where(h => h.IsActive == true)
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

        // Các action bị comment vẫn giữ nguyên comment như yêu cầu
        //// GET: Admin/Booking/Details/5
        //public ActionResult Details(int id)
        //{
        //    try
        //    {
        //        var booking = _db.Bookings.FirstOrDefault(b => b.Id == id);
        //        if (booking == null)
        //            return HttpNotFound();
        //        return View(booking);
        //    }
        //    catch
        //    {
        //        return HttpNotFound();
        //    }
        //}

            //// GET: Admin/Booking/Edit/5
            //public ActionResult Edit(int id)
            //{
            //    ViewBag.BookingId = id;
            //    return View();
            //}

            //// GET: Admin/Booking/GetBookingById/5 - AJAX
            //[HttpGet]
            //public ActionResult GetBookingById(int id)
            //{
            //    try
            //    {
            //        var booking = _db.Bookings
            //            .Where(b => b.Id == id)
            //            .Select(b => new
            //            {
            //                b.Id,
            //                b.Code,
            //                b.Status,
            //                b.Note,
            //                CheckInDate = b.CheckInDate.ToString("yyyy-MM-dd"),
            //                CheckOutDate = b.CheckOutDate.ToString("yyyy-MM-dd"),
            //                b.Guests
            //            })
            //            .FirstOrDefault();
            //        if (booking == null)
            //            return Json(new { success = false, message = "Không tìm thấy booking" }, JsonRequestBehavior.AllowGet);
            //        return Json(new { success = true, data = booking }, JsonRequestBehavior.AllowGet);
            //    }
            //    catch (Exception ex)
            //    {
            //        return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            //    }
            //}

            //// POST: Admin/Booking/UpdateBooking - AJAX
            //[HttpPost]
            //public ActionResult UpdateBooking(Booking model)
            //{
            //    try
            //    {
            //        var booking = _db.Bookings.FirstOrDefault(b => b.Id == model.Id);
            //        if (booking != null)
            //        {
            //            booking.Status = model.Status;
            //            booking.Note = model.Note;
            //            booking.UpdatedAt = DateTime.Now;
            //            _db.SubmitChanges();
            //            return Json(new { success = true, message = "Cập nhật thành công!" });
            //        }
            //        return Json(new { success = false, message = "Không tìm thấy booking" });
            //    }
            //    catch (Exception ex)
            //    {
            //        return Json(new { success = false, message = "Lỗi: " + ex.Message });
            //    }
            //}
    }
    }