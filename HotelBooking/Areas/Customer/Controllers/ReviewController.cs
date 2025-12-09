using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Customer.Controllers
{
    [Authorize(Roles = "customer")]
    public class ReviewController : Controller
    {
        private readonly DatabaseDataContext _db;

        public ReviewController()
        {
            _db = new DatabaseDataContext();
        }

        private int GetCurrentUserId()
        {
            var email = User.Identity.Name;
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            return user != null ? user.Id : 0;
        }

        // GET: Customer/Review/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Customer/Review/GetMyReviews - AJAX
        [HttpGet]
        public ActionResult GetMyReviews()
        {
            try
            {
                var userId = GetCurrentUserId();
                // SỬA: Reviews không còn trực tiếp nối với Hotel. Phải qua Booking.
                var reviews = _db.Reviews
                    .Where(r => r.UserId == userId && r.DeletedAt == null)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.Id,
                        // Update: Lấy tên khách sạn thông qua Booking
                        HotelName = r.Booking.Hotel.Name,
                        r.Rating,
                        r.Title,
                        r.Content,
                        r.CreatedAt,
                        CanEdit = System.Data.Linq.SqlClient.SqlMethods.DateDiffDay(r.CreatedAt, DateTime.Now) <= 30
                    })
                    .ToList();

                return Json(new { success = true, data = reviews }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Customer/Review/Create
        public ActionResult Create(int bookingId)
        {
            ViewBag.BookingId = bookingId;
            return View();
        }

        // GET: Customer/Review/GetBookingInfo - AJAX
        [HttpGet]
        public ActionResult GetBookingInfo(int bookingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                // Query này vẫn đúng vì nó query từ Bookings (bảng này vẫn giữ HotelId)
                var booking = _db.Bookings
                    .Where(b => b.Id == bookingId && b.UserId == userId)
                    .Select(b => new
                    {
                        b.Id,
                        b.HotelId,
                        HotelName = b.Hotel.Name,
                        // Kiểm tra xem đã có review nào link với booking này chưa
                        HasReview = _db.Reviews.Any(r => r.BookingId == bookingId)
                    })
                    .FirstOrDefault();

                if (booking == null)
                    return Json(new { success = false, message = "Không tìm thấy booking" }, JsonRequestBehavior.AllowGet);

                if (booking.HasReview)
                    return Json(new { success = false, message = "Bạn đã đánh giá booking này rồi" }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = booking }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Customer/Review/CreateReview - AJAX
        [HttpPost]
        public ActionResult CreateReview(Review model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                // Check if already reviewed
                if (_db.Reviews.Any(r => r.BookingId == model.BookingId))
                    return Json(new { success = false, message = "Bạn đã đánh giá booking này rồi" });

                var userId = GetCurrentUserId();
                model.UserId = userId;
                // Không cần gán model.HotelId vì cột này đã bị xóa trong DB
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;

                _db.Reviews.InsertOnSubmit(model);
                _db.SubmitChanges();

                return Json(new { success = true, message = "Thêm đánh giá thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Customer/Review/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.ReviewId = id;
            return View();
        }

        // GET: Customer/Review/GetReviewById/5 - AJAX
        [HttpGet]
        public ActionResult GetReviewById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                // SỬA: Điều chỉnh đường dẫn lấy thông tin Hotel
                var review = _db.Reviews
                    .Where(r => r.Id == id && r.UserId == userId)
                    .Select(r => new
                    {
                        r.Id,
                        // r.HotelId, -> Cột này không còn tồn tại
                        // Lấy tên khách sạn qua Booking
                        HotelName = r.Booking.Hotel.Name,
                        r.Rating,
                        r.Title,
                        r.Content,
                        r.CreatedAt,
                        DaysSinceCreated = System.Data.Linq.SqlClient.SqlMethods.DateDiffDay(r.CreatedAt, DateTime.Now)
                    })
                    .FirstOrDefault();

                if (review == null)
                    return Json(new { success = false, message = "Không tìm thấy đánh giá" }, JsonRequestBehavior.AllowGet);

                if (review.DaysSinceCreated > 30)
                    return Json(new { success = false, message = "Chỉ có thể chỉnh sửa đánh giá trong vòng 30 ngày" }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = review }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Customer/Review/UpdateReview - AJAX
        [HttpPost]
        public ActionResult UpdateReview(Review model)
        {
            try
            {
                // Logic update giữ nguyên vì chỉ update nội dung, không đụng đến quan hệ bảng
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                var userId = GetCurrentUserId();
                var review = _db.Reviews.FirstOrDefault(r => r.Id == model.Id && r.UserId == userId);

                if (review == null)
                    return Json(new { success = false, message = "Không tìm thấy đánh giá" });

                // Check 30 days limit
                if ((DateTime.Now - review.CreatedAt).Days > 30)
                    return Json(new { success = false, message = "Chỉ có thể chỉnh sửa đánh giá trong vòng 30 ngày" });

                review.Rating = model.Rating;
                review.Title = model.Title;
                review.Content = model.Content;
                review.UpdatedAt = DateTime.Now;

                _db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật đánh giá thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Customer/Review/DeleteReview - AJAX
        [HttpPost]
        public ActionResult DeleteReview(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var review = _db.Reviews.FirstOrDefault(r => r.Id == id && r.UserId == userId);

                if (review == null)
                    return Json(new { success = false, message = "Không tìm thấy đánh giá" });

                // Check 30 days limit
                if ((DateTime.Now - review.CreatedAt).Days > 30)
                    return Json(new { success = false, message = "Chỉ có thể xóa đánh giá trong vòng 30 ngày" });

                review.DeletedAt = DateTime.Now;
                _db.SubmitChanges();

                return Json(new { success = true, message = "Xóa đánh giá thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}