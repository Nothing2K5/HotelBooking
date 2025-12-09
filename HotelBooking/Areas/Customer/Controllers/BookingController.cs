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
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            return user != null ? user.Id : 0;
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
                        RoomNumber = b.Room.RoomNumber, // DB Mới: Số phòng
                        RoomType = b.Room.Code,         // DB Mới: Loại phòng (VIP/Normal)
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
            ViewBag.HotelId = hotelId;
            ViewBag.RoomId = roomId;
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
                    room = new
                    {
                        //room.Name, // Lưu ý: Nếu DB bạn xóa cột Name ở bảng Room thì dùng room.Code hoặc room.RoomNumber
                        room.RoomNumber,
                        Type = room.Code,
                        room.PricePerNight,
                        room.Capacity
                    },
                    hotel = new { hotel.Name }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

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

                // LOGIC MỚI: Kiểm tra Overlap (Trùng lịch) trong bảng Bookings
                // Một phòng bị coi là bận nếu tồn tại booking nào đó (không bị hủy) mà thời gian giao nhau với lịch mới
                bool isRoomBusy = _db.Bookings.Any(b =>
                    b.RoomId == model.RoomId &&
                    b.Status != "cancelled" &&
                    // Logic giao nhau: (StartA < EndB) && (EndA > StartB)
                    (model.CheckInDate < b.CheckOutDate && model.CheckOutDate > b.CheckInDate)
                );

                if (isRoomBusy)
                    return Json(new { success = false, message = "Phòng này đã được đặt trong khoảng thời gian bạn chọn. Vui lòng chọn phòng khác." });

                var nights = (model.CheckOutDate - model.CheckInDate).Days;
                var subtotal = room.PricePerNight * nights;

                // Tạo Booking (Cấu trúc mới không có BookingItems)
                var booking = new Booking
                {
                    UserId = userId,
                    HotelId = model.HotelId,
                    RoomId = model.RoomId,
                    Code = "BK" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Status = "draft",
                    CheckInDate = model.CheckInDate,
                    CheckOutDate = model.CheckOutDate,
                    Guests = model.Guests,
                    PricePerNight = room.PricePerNight, // Cột mới
                    Nights = nights,                    // Cột mới
                    SubTotal = subtotal,                // Cột mới
                    TotalAmount = subtotal,             // Logic khuyến mãi tính sau
                    FreeCancellationDeadline = model.CheckInDate.AddDays(-3), // Deadline hủy = CheckIn - 3 ngày
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Note = model.Note
                };

                _db.Bookings.InsertOnSubmit(booking);
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
                // Logic Mới: Lấy thông tin phẳng (Flatten) vì 1 Booking = 1 Room
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
                        HotelName = b.Hotel.Name,
                        HotelAddress = b.Hotel.Address + ", " + b.Hotel.City,
                        // Thông tin phòng lấy trực tiếp
                        RoomInfo = new
                        {
                            RoomNumber = b.Room.RoomNumber,
                            RoomType = b.Room.Code,
                            Price = b.PricePerNight,
                            Nights = b.Nights,
                            SubTotal = b.SubTotal
                        }
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
                    // Trigger SQL sẽ tự động tích điểm

                    return Json(new { success = true, message = "Thanh toán thành công!" });
                }
                else
                {
                    // Trả sau thì chuyển sang confirmed
                    booking.Status = "confirmed";
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

                // Tính phí hủy
                decimal penaltyAmount = 0;
                // Nếu hiện tại đã vượt quá hạn hủy miễn phí
                if (booking.FreeCancellationDeadline.HasValue && DateTime.Now > booking.FreeCancellationDeadline.Value)
                {
                    penaltyAmount = booking.TotalAmount * 0.5m; // Phạt 50%
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

                var message = penaltyAmount == 0
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