using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class ReviewController : Controller
    {
        private readonly DatabaseDataContext _db;

        public ReviewController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Admin/Review/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Review/GetAllReviews (AJAX)
        [HttpGet]
        public ActionResult GetAllReviews()
        {
            try
            {
                var reviews = _db.Reviews
                    .Where(r => r.DeletedAt == null)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.Id,
                        r.BookingId,
                        r.UserId,
                        r.Rating,
                        r.Title,
                        r.Content,
                        r.CreatedAt
                    })
                    .ToList();

                return Json(new { success = true, data = reviews }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/Review/Details
        public ActionResult Details(int id)
        {
            var review = _db.Reviews.FirstOrDefault(r => r.Id == id);
            if (review == null)
                return HttpNotFound();

            return View(review);
        }

        // POST: Admin/Review/DeleteReview (AJAX)
        [HttpPost]
        public ActionResult DeleteReview(int id)
        {
            try
            {
                var review = _db.Reviews.FirstOrDefault(r => r.Id == id);
                if (review == null)
                    return Json(new { success = false, message = "Không tìm thấy đánh giá" });

                review.DeletedAt = DateTime.Now;
                _db.SubmitChanges();

                return Json(new { success = true, message = "Xóa đánh giá thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
