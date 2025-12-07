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
            return View("");
        }

        // GET: Admin/Room/GetAllRooms - AJAX     
        [HttpGet]
        public ActionResult GetAllRooms(string keyword = null)
        {
            try
            {
                var query = from r in _db.Rooms
                            join h in _db.Hotels on r.HotelId equals h.Id
                            where r.IsActive == true
                            select new
                            {
                                r.Id,
                                HotelName = h.Name,
                                r.Name,
                                r.Capacity,
                                r.PricePerNight,
                                r.TotalRooms,
                                r.IsActive
                            };

                if (!string.IsNullOrEmpty(keyword))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(x => x.HotelName.ToLower().Contains(keyword));
                }

                var result = query.OrderBy(x => x.HotelName).ThenBy(x => x.Name).ToList();

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

        // GET: Admin/Room/GetHotelsForDropdown - AJAX
        [HttpGet]
        public ActionResult GetHotelsForDropdown()
        {
            try
            {
                var hotels = _db.Hotels
                    .Where(h => h.IsActive)
                    .Select(h => new
                    {
                        h.Id,
                        h.Name
                    })
                    .ToList();

                return Json(new { success = true, data = hotels }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/Room/CreateRoom - AJAX
        [HttpPost]
        public ActionResult CreateRoom(Room model, List<string> ImageUrls = null)
        {
            try
            {
                // === 1. Validate bắt buộc ===
                if (model.HotelId <= 0 ||
                    string.IsNullOrWhiteSpace(model.Code) ||
                    string.IsNullOrWhiteSpace(model.Name) ||
                    model.Capacity <= 0 ||
                    model.PricePerNight <= 0 ||
                    model.TotalRooms <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng điền đầy đủ các trường bắt buộc." });
                }

                // === 2. Kiểm tra trùng Code trong cùng khách sạn ===
                bool codeExists = _db.Rooms.Any(r =>
                    r.HotelId == model.HotelId &&
                    r.Code == model.Code.Trim().ToUpper());

                if (codeExists)
                    return Json(new { success = false, message = "Mã phòng đã tồn tại trong khách sạn này!" });

                // === 3. Chuẩn hóa dữ liệu ===
                model.Code = model.Code.Trim().ToUpper();
                model.Name = model.Name.Trim();
                model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
                model.IsActive = true;
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;

                // === 4. Thêm phòng mới ===
                _db.Rooms.InsertOnSubmit(model);
                _db.SubmitChanges(); // ← Lúc này model.Id đã được sinh tự động

                // === 5. Thêm hình ảnh (nếu có) ===
                if (ImageUrls != null && ImageUrls.Any(u => !string.IsNullOrWhiteSpace(u)))
                {
                    var validUrls = ImageUrls
                        .Where(u => !string.IsNullOrWhiteSpace(u))
                        .Select(u => u.Trim())
                        .Distinct()
                        .ToList();

                    var roomImages = validUrls.Select(url => new RoomImage
                    {
                        RoomId = model.Id,
                        Url = url,
                        AltText = $"{model.Name} - Hình ảnh phòng" // bạn có thể để null hoặc sinh tự động
                    }).ToList();

                    _db.RoomImages.InsertAllOnSubmit(roomImages);
                    _db.SubmitChanges();
                }

                // === 6. Trả kết quả thành công ===
                return Json(new
                {
                    success = true,
                    message = ImageUrls != null && ImageUrls.Any()
                        ? "Thêm phòng và hình ảnh thành công!"
                        : "Thêm phòng thành công!"
                });
            }
            catch (Exception ex)
            {
                // Có thể ghi log ở đây nếu cần
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // GET: Admin/Room/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.RoomId = id;
            return View();
        }

        // GET: Admin/Room/GetRoomById/5 - AJAX
        [HttpGet]
        public ActionResult GetRoomById(int id)
        {
            try
            {
                var room = _db.Rooms.FirstOrDefault(r => r.Id == id && r.IsActive == true);
                if (room == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng" }, JsonRequestBehavior.AllowGet);

                var images = _db.RoomImages
                    .Where(i => i.RoomId == id)
                    .Select(i => new
                    {
                        i.Id,
                        i.Url,
                        AltText = i.AltText ?? "Hình ảnh phòng"
                    })
                    .ToList();

                var data = new
                {
                    room.Id,
                    room.HotelId,
                    room.Code,
                    room.Name,
                    room.Description,
                    room.Capacity,
                    room.PricePerNight,
                    room.TotalRooms,
                    Images = images
                };

                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/Room/UpdateRoom - AJAX
        [HttpPost]
        public ActionResult UpdateRoom(Room model, List<string> NewImageUrls = null)
        {
            try
            {
                if (model.Id <= 0 || model.HotelId <= 0 ||
                    string.IsNullOrWhiteSpace(model.Code) ||
                    string.IsNullOrWhiteSpace(model.Name) ||
                    model.Capacity <= 0 || model.PricePerNight <= 0 || model.TotalRooms <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin bắt buộc." });
                }

                var room = _db.Rooms.FirstOrDefault(r => r.Id == model.Id);
                if (room == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng!" });

                // Kiểm tra trùng Code (trừ chính nó)
                bool codeExists = _db.Rooms.Any(r =>
                    r.Id != model.Id &&
                    r.HotelId == model.HotelId &&
                    r.Code == model.Code.Trim().ToUpper());

                if (codeExists)
                    return Json(new { success = false, message = "Mã phòng đã tồn tại trong khách sạn này!" });

                room.HotelId = model.HotelId;
                room.Code = model.Code.Trim().ToUpper();
                room.Name = model.Name.Trim();
                room.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
                room.Capacity = model.Capacity;
                room.PricePerNight = model.PricePerNight;
                room.TotalRooms = model.TotalRooms;
                room.UpdatedAt = DateTime.Now;

                // Thêm ảnh mới
                if (NewImageUrls != null && NewImageUrls.Any(u => !string.IsNullOrWhiteSpace(u)))
                {
                    var validUrls = NewImageUrls
                        .Where(u => !string.IsNullOrWhiteSpace(u))
                        .Select(u => u.Trim())
                        .Where(u => u.StartsWith("http"))
                        .Distinct()
                        .ToList();

                    if (validUrls.Any())
                    {
                        var newImages = validUrls.Select(url => new RoomImage
                        {
                            RoomId = room.Id,
                            Url = url,
                            AltText = $"{room.Name} - Hình ảnh phòng"
                        }).ToList();

                        _db.RoomImages.InsertAllOnSubmit(newImages);
                    }
                }

                _db.SubmitChanges();

                return Json(new
                {
                    success = true,
                    message = (NewImageUrls != null && NewImageUrls.Any())
                        ? "Cập nhật phòng và thêm ảnh thành công!"
                        : "Cập nhật phòng thành công!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        // 3. Action xóa ảnh (thêm mới)
        [HttpPost]
        public ActionResult DeleteRoomImage(int id)
        {
            try
            {
                var image = _db.RoomImages.FirstOrDefault(i => i.Id == id);
                if (image == null)
                    return Json(new { success = false, message = "Không tìm thấy ảnh" });

                _db.RoomImages.DeleteOnSubmit(image);
                _db.SubmitChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Room/DeleteRoom - AJAX
        [HttpPost]
        public ActionResult DeleteRoom(int id)
        {
            try
            {
                var room = _db.Rooms.FirstOrDefault(r => r.Id == id);
                if (room != null)
                {
                    // Check if room has active bookings
                    var hasActiveBookings = _db.BookingItems.Any(bi => bi.RoomId == id &&
                                                                       (bi.Booking.Status == "paid" || bi.Booking.Status == "confirmed"));

                    if (hasActiveBookings)
                        return Json(new { success = false, message = "Không thể xóa phòng có booking đang hoạt động" });

                    room.IsActive = false;
                    room.DeletedAt = DateTime.Now;
                    _db.SubmitChanges();

                    return Json(new { success = true, message = "Xóa thành công" });
                }
                return Json(new { success = false, message = "Không tìm thấy phòng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/Room/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.RoomId = id;
            return View();
        }

        // GET: Admin/Room/GetRoomDetails?id=5
        [HttpGet]
        public ActionResult GetRoomDetails(int id)
        {
            try
            {
                var room = _db.Rooms
                    .Where(r => r.Id == id && r.IsActive == true)
                    .Select(r => new
                    {
                        r.Id,
                        r.Code,
                        r.Name,
                        r.Description,
                        r.Capacity,
                        r.PricePerNight,
                        r.TotalRooms,
                        Hotel = new
                        {
                            r.Hotel.Name,
                            r.Hotel.Address,
                            r.Hotel.City
                        }
                    })
                    .FirstOrDefault();

                if (room == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng hoặc đã bị xóa." }, JsonRequestBehavior.AllowGet);

                var images = _db.RoomImages
                    .Where(i => i.RoomId == id)
                    .Select(i => new
                    {
                        i.Url,
                        AltText = i.AltText ?? "Hình ảnh phòng"
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    Room = room,
                    RoomImages = images
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}