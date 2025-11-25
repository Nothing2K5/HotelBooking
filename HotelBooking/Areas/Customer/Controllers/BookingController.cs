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
                // Lấy đầy đủ thông tin cần thiết cho View
                var booking = _db.Bookings
                    .Where(b => b.Id == id && b.UserId == GetCurrentUserId())
                    .Select(b => new
                    {
                        b.Id,
                        b.Code,
                        b.Status,
                        b.CheckInDate,
                        b.CheckOutDate,
                        b.Guests,
                        b.TotalAmount,
                        b.Note,
                        b.FreeCancellationDeadline,
                        b.PenaltyAmount,
                        // Thông tin khách sạn phẳng hóa
                        HotelName = b.Hotel.Name,
                        HotelAddress = b.Hotel.Address + ", " + b.Hotel.City,
                        // Lấy danh sách phòng (quan trọng)
                        BookingItems = b.BookingItems.Select(bi => new
                        {
                            RoomName = bi.Room.Name,
                            Price = bi.PricePerNight,
                            Nights = bi.Nights,
                            Quantity = bi.Quantity,
                            SubTotal = bi.SubTotal
                        }).ToList()
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

        // POST: Customer/Booking/CancelBooking - AJAX
        [HttpPost]
        public ActionResult CancelBooking(CancelBookingVM model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var booking = _db.Bookings.FirstOrDefault(b => b.Id == model.BookingId && b.UserId == userId);

                if (booking == null)
                    return Json(new { success = false, message = "Không tìm thấy booking" });

                if (booking.Status == "cancelled")
                    return Json(new { success = false, message = "Booking đã được hủy trước đó" });

                if (booking.Status == "completed")
                    return Json(new { success = false, message = "Không thể hủy booking đã hoàn thành" });

                // Calculate penalty
                decimal penaltyAmount = 0;
                if (!model.IsFreeCancellation)
                {
                    penaltyAmount = booking.TotalAmount * 0.5m; // 50% penalty
                }

                booking.Status = "cancelled";
                booking.CancelledAt = DateTime.Now;
                booking.PenaltyAmount = penaltyAmount;
                booking.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrEmpty(model.Reason))
                {
                    booking.Note = (booking.Note ?? "") + "\n[Lý do hủy: " + model.Reason + "]";
                }

                _db.SubmitChanges();

                var message = model.IsFreeCancellation
                    ? "Hủy booking thành công! Toàn bộ số tiền sẽ được hoàn lại."
                    : $"Hủy booking thành công! Phí hủy: {penaltyAmount:N0} VNĐ. Số tiền hoàn lại: {(booking.TotalAmount - penaltyAmount):N0} VNĐ";

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}