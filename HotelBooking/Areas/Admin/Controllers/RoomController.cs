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
            return View();
        }

        // GET: Admin/Room/GetAllRooms - AJAX
        [HttpGet]
        public ActionResult GetAllRooms(int? hotelId)
        {
            try
            {
                var query = _db.Rooms.AsQueryable();

                if (hotelId.HasValue)
                    query = query.Where(r => r.HotelId == hotelId.Value);

                var rooms = query
                    .Select(r => new
                    {
                        r.Id,
                        HotelName = r.Hotel.Name,
                        r.HotelId,
                        r.Name,
                        r.Capacity,
                        r.PricePerNight,
                        r.TotalRooms,
                        r.IsActive
                    })
                    .ToList();

                return Json(new { success = true, data = rooms }, JsonRequestBehavior.AllowGet);
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
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

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
                        r.TaxIncluded
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
                    room.TaxIncluded = model.TaxIncluded;
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