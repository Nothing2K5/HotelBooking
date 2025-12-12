using HotelBooking.Models;
using HotelBooking.ViewModels;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace HotelBooking.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseDataContext _db;

        public AccountController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register - AJAX
        [HttpPost]
        public ActionResult Register(RegisterVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                // Check email exists
                if (_db.Users.Any(u => u.Email == model.Email))
                    return Json(new { success = false, message = "Email đã tồn tại" });

                // Create user
                var user = new User
                {
                    Email = model.Email,
                    Password = model.Password,
                    Role = "customer",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                _db.Users.InsertOnSubmit(user);
                _db.SubmitChanges();

                // Create customer
                var customer = new Customer
                {
                    UserId = user.Id,
                    FullName = model.FullName,
                    Phone = model.Phone,
                    TotalPoints = 0
                };
                _db.Customers.InsertOnSubmit(customer);
                _db.SubmitChanges();

                // Auto login
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1, user.Email,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(30),
                    false,
                    user.Role,
                    FormsAuthentication.FormsCookiePath
                );
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(authCookie);

                return Json(new { success = true, message = "Đăng ký thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login - AJAX
        [HttpPost]
        public ActionResult Login(LoginVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                var user = _db.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

                if (user == null)
                    return Json(new { success = false, message = "Email hoặc mật khẩu không đúng" });

                if (!(user.IsActive == true))
                    return Json(new { success = false, message = "Tài khoản đã bị khóa" });

                // Tạo authentication ticket với Role
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1, // version
                    user.Email, // name
                    DateTime.Now, // issueDate
                    DateTime.Now.AddMinutes(30), // expiration
                    model.RememberMe, // isPersistent
                    user.Role, // userData - QUAN TRỌNG
                    FormsAuthentication.FormsCookiePath
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(authCookie);

                // Redirect URL theo role
                string redirectUrl = user.Role == "admin"
                    ? Url.Action("Index", "Hotel", new { area = "Admin" })
                    : Url.Action("Index", "Home");

                return Json(new { success = true, redirectUrl = redirectUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Account/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword - AJAX
        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                    return Json(new { success = false, message = "Email không tồn tại trong hệ thống" });

                if (user.IsActive != true)
                    return Json(new { success = false, message = "Tài khoản này đang bị khóa" });

                // 1. Tạo mật khẩu mới ngẫu nhiên (Giả lập việc gửi email)
                Random rand = new Random();
                string newPassword = "Pass" + rand.Next(1000, 9999).ToString();

                // 2. Cập nhật vào DB
                user.Password = newPassword;
                _db.SubmitChanges();

                // 3. Trả về JSON chứa mật khẩu mới để hiển thị (Thay vì gửi email thật)
                return Json(new { success = true, message = "Thành công! Mật khẩu mới của bạn là: " + newPassword });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Account/Logout - AJAX
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Json(new { success = true });
        }
    }
}