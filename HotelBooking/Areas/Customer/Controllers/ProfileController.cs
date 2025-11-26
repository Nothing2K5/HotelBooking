using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace HotelBooking.Areas.Customer.Controllers
{
    [Authorize(Roles = "customer")]
    public class ProfileController : Controller
    {
        private readonly DatabaseDataContext _db;

        public ProfileController()
        {
            _db = new DatabaseDataContext();
        }

        private int GetCurrentUserId()
        {
            var email = User.Identity.Name;
            return _db.Users.First(u => u.Email == email).Id;
        }

        // GET: Customer/Profile/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Customer/Profile/GetProfile - AJAX
        [HttpGet]
        public ActionResult GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var customer = _db.Customers
                    .Where(c => c.UserId == userId)
                    .Select(c => new
                    {
                        Email = c.User.Email,
                        c.FullName,
                        c.Phone
                    })
                    .FirstOrDefault();

                return Json(new { success = true, data = customer }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Customer/Profile/GetLoyaltyInfo - AJAX
        [HttpGet]
        public ActionResult GetLoyaltyInfo()
        {
            try
            {
                var userId = GetCurrentUserId();
                var customer = _db.Customers.FirstOrDefault(c => c.UserId == userId);

                return Json(new
                {
                    success = true,
                    points = customer.TotalPoints,
                    tierName = customer.LoyaltyTier?.Name ?? "Regular"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Customer/Profile/UpdateProfile - AJAX
        //[HttpPost]
        //public ActionResult UpdateProfile(ProfileEditVM model)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

        //        var userId = GetCurrentUserId();
        //        var customer = _db.Customers.FirstOrDefault(c => c.UserId == userId);

        //        if (customer != null)
        //        {
        //            customer.FullName = model.FullName;
        //            customer.Phone = model.Phone;
        //            _db.SubmitChanges();

        //            return Json(new { success = true, message = "Cập nhật thành công!" });
        //        }

        //        return Json(new { success = false, message = "Không tìm thấy thông tin" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Lỗi: " + ex.Message });
        //    }
        //}

        // GET: Customer/Profile/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: Customer/Profile/ChangePassword - AJAX
        [HttpPost]
        public ActionResult ChangePassword(string oldPassword, string newPassword)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);

                if (user.Password != oldPassword)
                    return Json(new { success = false, message = "Mật khẩu cũ không đúng" });

                user.Password = newPassword;
                _db.SubmitChanges();

                return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Customer/Profile/Delete
        public ActionResult Delete()
        {
            return View();
        }

        // POST: Customer/Profile/DeleteAccount - AJAX
        [HttpPost]
        public ActionResult DeleteAccount(string password)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);

                if (user.Password != password)
                    return Json(new { success = false, message = "Mật khẩu không đúng" });

                // Check active bookings
                var hasActiveBookings = _db.Bookings.Any(b => b.UserId == userId &&
                                                              b.Status != "cancelled" &&
                                                              b.Status != "completed");
                if (hasActiveBookings)
                    return Json(new { success = false, message = "Không thể xóa tài khoản khi còn booking đang hoạt động" });

                user.IsActive = false;
                _db.SubmitChanges();

                FormsAuthentication.SignOut();
                return Json(new { success = true, message = "Xóa tài khoản thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}