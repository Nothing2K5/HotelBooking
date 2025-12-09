using HotelBooking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class RoomController : Controller
    {
        private readonly DatabaseDataContext _db;
        public RoomController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Admin/Room/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Room/GetAllRooms - AJAX (ĐÃ SỬA THEO DB MỚI)
        [HttpGet]
        public ActionResult GetAllRooms(string keyword = null)
        {
            try
            {
                var query = from r in _db.Rooms
                            join h in _db.Hotels on r.HotelId equals h.Id
                            where r.IsActive == true && r.DeletedAt == null
                            select new
                            {
                                r.Id,
                                HotelName = h.Name,
                                r.Code,
                                r.RoomNumber,           // ← Dùng RoomNumber thay Name
                                r.Description,
                                r.Capacity,
                                r.PricePerNight,
                                r.Status,               // Có thể là "available", "maintenance",...
                                r.IsActive
                            };

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(x =>
                        x.HotelName.ToLower().Contains(keyword) ||
                        x.RoomNumber.ToLower().Contains(keyword) ||
                        x.Code.ToLower().Contains(keyword));
                }

                var result = query
                    .OrderBy(x => x.HotelName)
                    .ThenBy(x => x.RoomNumber)
                    .ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/Room/Create
        public ActionResult Create()
        {
            return View();
        }

        // GET: Admin/Room/GetHotelsForDropdown
        [HttpGet]
        public ActionResult GetHotelsForDropdown()
        {
            try
            {
                var hotels = _db.Hotels
                    .Where(h => h.IsActive)
                    .Select(h => new { h.Id, h.Name })
                    .OrderBy(h => h.Name)
                    .ToList();
                return Json(new { success = true, data = hotels }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/Room/CreateRoom - AJAX (CẬP NHẬT THEO DB MỚI)
        [HttpPost]
        public ActionResult CreateRoom(Room model, List<string> ImageUrls = null)
        {
            try
            {
                if (model.HotelId <= 0 ||
                    string.IsNullOrWhiteSpace(model.Code) ||
                    string.IsNullOrWhiteSpace(model.RoomNumber) ||
                    model.Capacity <= 0 ||
                    model.PricePerNight <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng điền đầy đủ các trường bắt buộc." });
                }

                // Kiểm tra trùng RoomNumber trong cùng khách sạn
                bool exists = _db.Rooms.Any(r =>
                    r.HotelId == model.HotelId &&
                    r.RoomNumber.Trim().ToLower() == model.RoomNumber.Trim().ToLower());

                if (exists)
                    return Json(new { success = false, message = "Số phòng đã tồn tại trong khách sạn này!" });

                // Chuẩn hóa
                model.Code = model.Code.Trim().ToUpper();
                model.RoomNumber = model.RoomNumber.Trim();
                model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
                model.Status = "available";
                model.IsActive = true;
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;

                _db.Rooms.InsertOnSubmit(model);
                _db.SubmitChanges();

                // Thêm ảnh
                if (ImageUrls != null && ImageUrls.Any(u => !string.IsNullOrWhiteSpace(u)))
                {
                    var validUrls = ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()).Distinct();
                    var images = validUrls.Select(url => new RoomImage
                    {
                        RoomId = model.Id,
                        Url = url,
                        AltText = $"Phòng {model.RoomNumber}"
                    }).ToList();

                    _db.RoomImages.InsertAllOnSubmit(images);
                    _db.SubmitChanges();
                }

                return Json(new { success = true, message = "Thêm phòng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/Room/Edit/5 + GetRoomById (CẬP NHẬT)
        [HttpGet]
        public ActionResult GetRoomById(int id)
        {
            try
            {
                var room = _db.Rooms.FirstOrDefault(r => r.Id == id && r.IsActive == true);
                if (room == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng" }, JsonRequestBehavior.AllowGet);

                var images = _db.RoomImages.Where(i => i.RoomId == id)
                    .Select(i => new { i.Id, i.Url, AltText = i.AltText ?? "Hình phòng" })
                    .ToList();

                var data = new
                {
                    room.Id,
                    room.HotelId,
                    room.Code,
                    room.RoomNumber,
                    room.Description,
                    room.Capacity,
                    room.PricePerNight,
                    room.Status,
                    Images = images
                };

                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/Room/UpdateRoom (CẬP NHẬT)
        [HttpPost]
        public ActionResult UpdateRoom(Room model, List<string> NewImageUrls = null)
        {
            try
            {
                var room = _db.Rooms.FirstOrDefault(r => r.Id == model.Id);
                if (room == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng!" });

                // Kiểm tra trùng RoomNumber (trừ chính nó)
                bool exists = _db.Rooms.Any(r =>
                    r.Id != model.Id &&
                    r.HotelId == model.HotelId &&
                    r.RoomNumber.Trim().ToLower() == model.RoomNumber.Trim().ToLower());

                if (exists)
                    return Json(new { success = false, message = "Số phòng đã tồn tại!" });

                room.HotelId = model.HotelId;
                room.Code = model.Code.Trim().ToUpper();
                room.RoomNumber = model.RoomNumber.Trim();
                room.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
                room.Capacity = model.Capacity;
                room.PricePerNight = model.PricePerNight;
                room.Status = model.Status;
                room.UpdatedAt = DateTime.Now;

                // Thêm ảnh mới
                if (NewImageUrls != null && NewImageUrls.Any())
                {
                    var newImages = NewImageUrls
                        .Where(u => !string.IsNullOrWhiteSpace(u))
                        .Select(u => u.Trim())
                        .Distinct()
                        .Select(url => new RoomImage
                        {
                            RoomId = room.Id,
                            Url = url,
                            AltText = $"Phòng {room.RoomNumber}"
                        }).ToList();

                    _db.RoomImages.InsertAllOnSubmit(newImages);
                }

                _db.SubmitChanges();
                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Xóa ảnh, Xóa phòng (soft delete), Details → giữ nguyên logic, chỉ sửa nhỏ
        [HttpPost]
        public ActionResult DeleteRoomImage(int id)
        {
            try
            {
                var img = _db.RoomImages.FirstOrDefault(i => i.Id == id);
                if (img != null)
                {
                    _db.RoomImages.DeleteOnSubmit(img);
                    _db.SubmitChanges();
                }
                return Json(new { success = true });
            }
            catch { return Json(new { success = false }); }
        }

        [HttpPost]
        public ActionResult DeleteRoom(int id)
        {
            try
            {
                var room = _db.Rooms.FirstOrDefault(r => r.Id == id);
                if (room != null)
                {
                    // Kiểm tra có booking đang active không (nếu cần)
                    room.IsActive = false;
                    room.DeletedAt = DateTime.Now;
                    _db.SubmitChanges();
                    return Json(new { success = true, message = "Đã xóa phòng" });
                }
                return Json(new { success = false, message = "Không tìm thấy" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult Edit(int id) => View();
        public ActionResult Details(int id) => View();
    }
}