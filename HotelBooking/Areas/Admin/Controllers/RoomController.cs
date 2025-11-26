using HotelBooking.Models;
using System;
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
        public ActionResult CreateRoom(Room model)
        {
            try
            {
                if (Request.ContentType.Contains("application/json"))
                {

                }

                if (model.HotelId <= 0 || string.IsNullOrEmpty(model.Code) || string.IsNullOrEmpty(model.Name))
                    return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin bắt buộc" });

                bool codeExists = _db.Rooms.Any(r => r.HotelId == model.HotelId && r.Code == model.Code.Trim().ToUpper());
                if (codeExists)
                    return Json(new { success = false, message = "Mã phòng đã tồn tại trong khách sạn này!" });

                model.Code = model.Code.Trim().ToUpper();
                model.Name = model.Name.Trim();
                model.Description = string.IsNullOrEmpty(model.Description) ? null : model.Description.Trim();
                model.IsActive = true;
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;

                _db.Rooms.InsertOnSubmit(model);
                _db.SubmitChanges();

                return Json(new { success = true, message = "Thêm phòng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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
                var room = _db.Rooms
                    .Where(r => r.Id == id)
                    .Select(r => new
                    {
                        r.Id,
                        r.HotelId,
                        r.Code,
                        r.Name,
                        r.Description,
                        r.Capacity,
                        r.PricePerNight,
                        r.TotalRooms,

                    })
                    .FirstOrDefault();

                if (room == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng" }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = room }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/Room/UpdateRoom - AJAX
        [HttpPost]
        public ActionResult UpdateRoom(Room model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                var room = _db.Rooms.FirstOrDefault(r => r.Id == model.Id);
                if (room != null)
                {
                    room.HotelId = model.HotelId;
                    room.Code = model.Code;
                    room.Name = model.Name;
                    room.Description = model.Description;
                    room.Capacity = model.Capacity;
                    room.PricePerNight = model.PricePerNight;
                    room.TotalRooms = model.TotalRooms;
                    room.UpdatedAt = DateTime.Now;

                    _db.SubmitChanges();

                    return Json(new { success = true, message = "Cập nhật thành công!" });
                }
                return Json(new { success = false, message = "Không tìm thấy phòng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
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
            try
            {
                var room = _db.Rooms.FirstOrDefault(r => r.Id == id);
                if (room == null)
                    return HttpNotFound();

                return View(room);
            }
            catch
            {
                return HttpNotFound();
            }
        }
    }
}