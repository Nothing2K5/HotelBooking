using HotelBooking.Models;
using HotelBooking.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Customer.Controllers
{
    [Authorize(Roles = "customer")]
    public class BookingController : Controller
    {
        private readonly DatabaseDataContext _db;

        public BookingController()
        {
            _db = new DatabaseDataContext();
        }

        private int GetCurrentUserId()
        {
            var email = User.Identity.Name;
            return _db.Users.First(u => u.Email == email).Id;
        }

        // GET: Customer/Booking/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Customer/Booking/GetMyBookings - AJAX
        [HttpGet]
        public ActionResult GetMyBookings()
        {
            try
            {
                var userId = GetCurrentUserId();
                var bookings = _db.Bookings
                    .Where(b => b.UserId == userId && b.DeletedAt == null)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new
                    {
                        b.Id,
                        b.Code,
                        HotelName = b.Hotel.Name,
                        b.CheckInDate,
                        b.CheckOutDate,
                        b.Status,
                        b.TotalAmount
                    })
                    .ToList();

                return Json(new { success = true, data = bookings }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Customer/Booking/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var booking = _db.Bookings.FirstOrDefault(b => b.Id == id);
                if (booking == null || booking.UserId != GetCurrentUserId())
                    return HttpNotFound();

                return View(booking);
            }
            catch
            {
                return HttpNotFound();
            }
        }

        // GET: Customer/Booking/Create
        public ActionResult Create(int hotelId, int roomId)
        {
            return View();
        }

        // GET: Customer/Booking/GetRoomInfo - AJAX
        [HttpGet]
        public ActionResult GetRoomInfo(int roomId, int hotelId)
        {
            try
            {
                var room = _db.Rooms.FirstOrDefault(r => r.Id == roomId);
                var hotel = _db.Hotels.FirstOrDefault(h => h.Id == hotelId);

                if (room == null || hotel == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin" }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    room = new { room.Name, room.PricePerNight, room.Capacity },
                    hotel = new { hotel.Name }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Customer/Booking/ValidatePromo - AJAX
        //[HttpPost]
        //public ActionResult ValidatePromo(PromoCodeVM model)
        //{
        //    try
        //    {
        //        var promo = _db.Promotions.FirstOrDefault(p => p.Code == model.Code && p.IsActive);

        //        if (promo == null)
        //            return Json(new { success = false, message = "Mã không tồn tại" });

        //        var now = DateTime.Now;
        //        if (promo.StartDate.HasValue && now < promo.StartDate.Value)
        //            return Json(new { success = false, message = "Mã chưa có hiệu lực" });

        //        if (promo.EndDate.HasValue && now > promo.EndDate.Value)
        //            return Json(new { success = false, message = "Mã đã hết hạn" });

        //        if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value)
        //            return Json(new { success = false, message = "Mã đã hết lượt sử dụng" });

        //        decimal discountAmount = promo.Type == "amount" ? promo.Value : 0; // Simplified

        //        return Json(new { success = true, discountAmount = discountAmount });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Lỗi: " + ex.Message });
        //    }
        //}

        // POST: Customer/Booking/CreateBooking - AJAX
        [HttpPost]
        public ActionResult CreateBooking(BookingCreateVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                if (model.CheckOutDate <= model.CheckInDate)
                    return Json(new { success = false, message = "Ngày trả phòng phải sau ngày nhận phòng" });

                var userId = GetCurrentUserId();
                var room = _db.Rooms.FirstOrDefault(r => r.Id == model.RoomId);

                if (room == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng" });

                var nights = (model.CheckOutDate - model.CheckInDate).Days;
                var subtotal = room.PricePerNight * nights;

                var booking = new Booking
                {
                    UserId = userId,
                    HotelId = model.HotelId,
                    Code = "BK" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Status = "draft",
                    CheckInDate = model.CheckInDate,
                    CheckOutDate = model.CheckOutDate,
                    Guests = model.Guests,
                    TotalAmount = subtotal,
                    FreeCancellationDeadline = model.CheckInDate.AddDays(-2),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Note = model.Note
                };

                _db.Bookings.InsertOnSubmit(booking);
                _db.SubmitChanges();

                // Tạo BookingItem
                var bookingItem = new BookingItem
                {
                    BookingId = booking.Id,
                    RoomId = model.RoomId,
                    PricePerNight = room.PricePerNight,
                    Nights = nights,
                    Quantity = 1,
                    SubTotal = subtotal
                };
                _db.BookingItems.InsertOnSubmit(bookingItem);
                _db.SubmitChanges();

                return Json(new { success = true, bookingId = booking.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Customer/Booking/Payment/5
        public ActionResult Payment(int id)
        {
            ViewBag.BookingId = id;
            return View();
        }

        // GET: Customer/Booking/GetBookingInfo/5 - AJAX
        [HttpGet]
        public ActionResult GetBookingInfo(int id)
        {
            try
            {
                var booking = _db.Bookings
                    .Where(b => b.Id == id && b.UserId == GetCurrentUserId())
                    .Select(b => new
                    {
                        b.Code,
                        HotelName = b.Hotel.Name,
                        b.CheckInDate,
                        b.CheckOutDate,
                        b.TotalAmount
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

        // POST: Customer/Booking/ProcessPayment - AJAX
        [HttpPost]
        public ActionResult ProcessPayment(PaymentVM model)
        {
            try
            {
                var booking = _db.Bookings.FirstOrDefault(b => b.Id == model.BookingId && b.UserId == GetCurrentUserId());

                if (booking == null)
                    return Json(new { success = false, message = "Không tìm thấy booking" });

                if (model.PaymentMethod == "online")
                {
                    // Mô phỏng thanh toán online thành công
                    booking.Status = "paid";
                    booking.UpdatedAt = DateTime.Now;

                    var payment = new Payment
                    {
                        BookingId = model.BookingId,
                        Amount = booking.TotalAmount,
                        Status = "success",
                        PaidAt = DateTime.Now,
                        CreatedAt = DateTime.Now
                    };
                    _db.Payments.InsertOnSubmit(payment);
                    _db.SubmitChanges();

                    return Json(new { success = true, message = "Thanh toán thành công!" });
                }
                else
                {
                    booking.Status = "pending";
                    booking.UpdatedAt = DateTime.Now;
                    _db.SubmitChanges();

                    return Json(new { success = true, message = "Đặt phòng thành công! Vui lòng thanh toán tại khách sạn." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Customer/Booking/Cancel/5
        public ActionResult Cancel(int id)
        {
            try
            {
                var booking = _db.Bookings.FirstOrDefault(b => b.Id == id && b.UserId == GetCurrentUserId());
                if (booking == null)
                    return HttpNotFound();

                return View(booking);
            }
            catch
            {
                return HttpNotFound();
            }
        }
    }
}