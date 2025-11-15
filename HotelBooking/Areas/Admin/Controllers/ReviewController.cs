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

        // GET: Admin/Review/GetAllReviews - AJAX
        [HttpGet]
        public ActionResult GetAllReviews(int? hotelId)
        {
            try
            {
                var query = _db.Reviews.Where(r => r.DeletedAt == null);

                if (hotelId.HasValue)
                    query = query.Where(r => r.HotelId == hotelId.Value);

                var reviews = query
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.Id,
                        HotelName = r.Hotel.Name,
                        UserEmail = r.User.Email,
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

        // GET: Admin/Review/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var review = _db.Reviews.FirstOrDefault(r => r.Id == id);
                if (review == null)
                    return HttpNotFound();

                return View(review);
            }
            catch
            {
                return HttpNotFound();
            }
        }

        // POST: Admin/Review/DeleteReview - AJAX
        [HttpPost]
        public ActionResult DeleteReview(int id)
        {
            try
            {
                var review = _db.Reviews.FirstOrDefault(r => r.Id == id);
                if (review != null)
                {
                    review.DeletedAt = DateTime.Now;
                    _db.SubmitChanges();

                    return Json(new { success = true, message = "Xóa đánh giá thành công" });
                }
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}