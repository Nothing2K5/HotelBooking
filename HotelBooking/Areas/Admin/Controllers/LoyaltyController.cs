using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class LoyaltyController : Controller
    {
        private readonly DatabaseDataContext _db;

        public LoyaltyController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Admin/Loyalty/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Loyalty/GetAllTiers - AJAX
        [HttpGet]
        public ActionResult GetAllTiers()
        {
            try
            {
                var tiers = _db.LoyaltyTiers
                    .OrderBy(t => t.DiscountPercent)
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        t.DiscountPercent,
                        t.Multiplier,
                        t.MinPoints,
                        t.CreatedAt
                    })
                    .ToList();

                return Json(new { success = true, data = tiers }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/Loyalty/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Loyalty/CreateTier - AJAX
        [HttpPost]
        public ActionResult CreateTier(LoyaltyTier model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                // Check duplicate name
                if (_db.LoyaltyTiers.Any(t => t.Name == model.Name))
                    return Json(new { success = false, message = "Tên tier đã tồn tại" });

                model.CreatedAt = DateTime.Now;

                _db.LoyaltyTiers.InsertOnSubmit(model);
                _db.SubmitChanges();

                return Json(new { success = true, message = "Thêm loyalty tier thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/Loyalty/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.TierId = id;
            return View();
        }

        // GET: Admin/Loyalty/GetTierById/5 - AJAX
        [HttpGet]
        public ActionResult GetTierById(int id)
        {
            try
            {
                var tier = _db.LoyaltyTiers
                    .Where(t => t.Id == id)
                    .Select(t => new
                    {
                        t.Id,
                        t.Name,
                        t.DiscountPercent,
                        t.Multiplier,
                        t.MinPoints
                    })
                    .FirstOrDefault();

                if (tier == null)
                    return Json(new { success = false, message = "Không tìm thấy loyalty tier" }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, data = tier }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/Loyalty/UpdateTier - AJAX
        [HttpPost]
        public ActionResult UpdateTier(LoyaltyTier model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                var tier = _db.LoyaltyTiers.FirstOrDefault(t => t.Id == model.Id);
                if (tier != null)
                {
                    // Check if name changed and is duplicate
                    if (tier.Name != model.Name && _db.LoyaltyTiers.Any(t => t.Name == model.Name))
                        return Json(new { success = false, message = "Tên tier đã tồn tại" });

                    tier.Name = model.Name;
                    tier.DiscountPercent = model.DiscountPercent;
                    tier.Multiplier = model.Multiplier;
                    tier.MinPoints = model.MinPoints;

                    _db.SubmitChanges();

                    return Json(new { success = true, message = "Cập nhật thành công!" });
                }
                return Json(new { success = false, message = "Không tìm thấy loyalty tier" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/Loyalty/DeleteTier - AJAX
        [HttpPost]
        public ActionResult DeleteTier(int id)
        {
            try
            {
                var tier = _db.LoyaltyTiers.FirstOrDefault(t => t.Id == id);
                if (tier != null)
                {
                    // Check if tier is in use
                    if (_db.Customers.Any(c => c.LoyaltyTierId == id))
                        return Json(new { success = false, message = "Không thể xóa tier đang được sử dụng" });

                    _db.LoyaltyTiers.DeleteOnSubmit(tier);
                    _db.SubmitChanges();

                    return Json(new { success = true, message = "Xóa thành công" });
                }
                return Json(new { success = false, message = "Không tìm thấy loyalty tier" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}