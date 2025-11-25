using HotelBooking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace HotelBooking.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        public readonly DatabaseDataContext _db;
        // GET: Admin/Users
        public ActionResult Index()
        {
            return View();
        }
        public UsersController()
        {
            _db = new DatabaseDataContext();
        }
        // GET: Admin/Users/GetAllUsers?keyword=abc&role=admin
        [HttpGet]
        public ActionResult GetAllUsers(string keyword = null, string role = null)
        {
            try
            {
                // Lấy tất cả user + join thông tin tên và phone
                var users = _db.Users.Select(u => new
                {
                    id = u.Id,
                    email = u.Email,
                    password = u.Password,
                    role = u.Role,
                    isActive = u.IsActive,
                    fullName = u.Role == "admin"
                        ? _db.Admins.Where(a => a.UserId == u.Id).Select(a => a.FullName).FirstOrDefault()
                        : _db.Customers.Where(c => c.UserId == u.Id).Select(c => c.FullName).FirstOrDefault(),
                    phone = u.Role == "customer"
                        ? _db.Customers.Where(c => c.UserId == u.Id).Select(c => c.Phone).FirstOrDefault()
                        : (string)null
                }).AsQueryable();

                // === TÌM KIẾM ===
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = keyword.Trim().ToLower();
                    users = users.Where(u =>
                        u.email.ToLower().Contains(keyword) ||
                        (u.fullName != null && u.fullName.ToLower().Contains(keyword)) ||
                        (u.phone != null && u.phone.Contains(keyword))
                    );
                }

                // === LỌC THEO ROLE ===
                if (!string.IsNullOrWhiteSpace(role) && (role == "admin" || role == "customer"))
                {
                    users = users.Where(u => u.role == role);
                }

                // Chỉ trả về những field cần thiết (ẩn password nếu muốn bảo mật)
                var result = users.Select(u => new
                {
                    id = u.id,
                    email = u.email,
                    password = u.password, // bạn đang hiển thị → có thể đổi thành "••••••" ở View
                    role = u.role,
                    isActive = u.isActive
                }).ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Edit(int id)
        {
            ViewBag.UserId = id;
            return View();
        }

        public ActionResult GetUserById(int id)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var result = new
            {
                email = user.Email,
                role = user.Role,
                isActive = user.IsActive
            };
            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateUser() // model có thể là anonymous cũng được
        {
            var model = _db.Users.Context.GetChangeSet().Inserts.OfType<User>().FirstOrDefault();

            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == model.Id);
                if (user == null) return Json(new { success = false, message = "Không tìm thấy tài khoản" });

                // Kiểm tra email trùng (ngoại trừ chính nó)
                var exist = _db.Users.Any(u => u.Email == model.Email && u.Id != model.Id);
                if (exist) return Json(new { success = false, message = "Email đã được sử dụng!" });

                user.Email = model.Email;
                user.Role = model.Role;
                user.IsActive = model.IsActive;

                // Chỉ cập nhật mật khẩu nếu có nhập
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password; // bạn đang lưu plain text → mình giữ nguyên như yêu cầu
                }

                _db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/Users/DeleteUser - AJAX
        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Id == id);
                if (user == null)
                    return Json(new { success = false, message = "Không tìm thấy tài khoản" });
                user.IsActive = false; // Chuyển trạng thái thành không hoạt động
                user.DeletedAt = DateTime.Now;  
                return Json(new { success = true, message = "Xóa tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        // 1. Kiểm tra Id đã tồn tại chưa
        [HttpGet]
        public JsonResult CheckIdExists(int id)
        {
            bool exists = _db.Users.Any(u => u.Id == id);
            return Json(new { exists }, JsonRequestBehavior.AllowGet);
        }

        // 2. Kiểm tra Email đã tồn tại chưa
        [HttpGet]
        public JsonResult CheckEmailExists(string email)
        {
            bool exists = _db.Users.Any(u => u.Email == email.Trim());
            return Json(new { exists }, JsonRequestBehavior.AllowGet);
        }

        // 3. Kiểm tra Phone đã tồn tại trong bảng Customers (nếu là customer)
        [HttpGet]
        public JsonResult CheckPhoneExists(string phone)
        {
            bool exists = _db.Customers.Any(c => c.Phone == phone.Trim()); // ← dùng _db
            return Json(new { exists }, JsonRequestBehavior.AllowGet);
        }

        // 4. Action chính: Thêm User mới (Create)
        [HttpPost]
        public JsonResult CreateUser()
        {
            try
            {
                int id = int.Parse(Request["Id"] ?? "0");
                string email = (Request["Email"] ?? "").Trim();
                string password = Request["Password"]?.Trim();
                string role = (Request["Role"] ?? "").Trim();
                string fullName = (Request["FullName"] ?? "").Trim();
                string phone = (Request["Phone"] ?? "").Trim();

                if (id <= 0) return Json(new { success = false, message = "Mã Id không hợp lệ!" });
                if (string.IsNullOrEmpty(email)) return Json(new { success = false, message = "Email bắt buộc!" });
                if (string.IsNullOrEmpty(password)) return Json(new { success = false, message = "Mật khẩu bắt buộc!" });
                if (string.IsNullOrEmpty(fullName)) return Json(new { success = false, message = "Họ tên bắt buộc!" });
                if (string.IsNullOrEmpty(role)) return Json(new { success = false, message = "Chọn vai trò!" });

                // === DÙNG CHUNG 1 DATACONTEXT _db → TRÁNH LỖI TRÙNG ===
                if (_db.Users.Any(u => u.Id == id))
                    return Json(new { success = false, message = "Mã người dùng đã tồn tại!" });

                if (_db.Users.Any(u => u.Email == email))
                    return Json(new { success = false, message = "Email đã được sử dụng!" });

                // === 1. TẠO USER TRƯỚC ===
                var newUser = new User
                {
                    Email = email,
                    Password = password,
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _db.Users.InsertOnSubmit(newUser);
                _db.SubmitChanges(); // ← User đã vào DB thật

                // === 2. TẠO ADMIN HOẶC CUSTOMER ===
                if (role == "admin")
                {
                    _db.Admins.InsertOnSubmit(new Models.Admin
                    {
                        UserId = newUser.Id,
                        FullName = fullName
                    });
                }
                else if (role == "customer")
                {
                    if (string.IsNullOrEmpty(phone))
                        return Json(new { success = false, message = "Số điện thoại bắt buộc!" });

                    if (!Regex.IsMatch(phone, @"^0[3-9]\d{8}$"))
                        return Json(new { success = false, message = "Số điện thoại không hợp lệ!" });

                    if (_db.Customers.Any(c => c.Phone == phone))
                        return Json(new { success = false, message = "Số điện thoại đã tồn tại!" });

                    _db.Customers.InsertOnSubmit(new Models.Customer
                    {
                        UserId = newUser.Id,
                        FullName = fullName,
                        Phone = phone,
                        LoyaltyTierId = 1,
                        TotalPoints = 0
                    });
                }

                _db.SubmitChanges(); // ← Lưu Admin/Customer

                return Json(new { success = true, message = "Thêm người dùng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        public ActionResult Create()
        {
            return View();
        }

    }
}