using HotelBooking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult GetAllUsers()
        {
            try
            {
                var users = _db.Users
                    .Select(u => new
                    {
                        id = u.Id,
                        email = u.Email,
                        role = u.Role,
                        password = u.Password,
                        isActive = u.IsActive
                    })
                    .ToList();

                return Json(new { success = true, data = users }, JsonRequestBehavior.AllowGet);
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
        public ActionResult UpdateUser(UserModel model) // model có thể là anonymous cũng được
        {
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

    }
}