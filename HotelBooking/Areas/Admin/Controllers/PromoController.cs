using HotelBooking.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace HotelBooking.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class PromoController : Controller
    {
        private readonly DatabaseDataContext _db;

        public PromoController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Admin/Promo/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Promo/GetAllPromos - AJAX
        [HttpGet]
        public ActionResult GetAllPromos()
        {
            try
            {
                var promos = _db.Promotions
                    .Select(p => new
                    {
                        p.Id,
                        p.Code,
                        p.Description,
                        p.Type,
                        p.Value,
                        p.StartDate,
                        p.EndDate,
                        /*p.UsageLimit,
                        p.UsedCount,*/
                        p.IsActive
                    })
                    .ToList();

                return Json(new { success = true, data = promos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/Promo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Promo/CreatePromo - AJAX
        [HttpPost]
        public ActionResult CreatePromo(Promotion model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                // Check duplicate code
                if (_db.Promotions.Any(p => p.Code == model.Code))
                    return Json(new { success = false, message = "Mã khuyến mãi đã tồn tại" });

                model.IsActive = true;
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;
                /*model.UsedCount = 0;*/

                _db.Promotions.InsertOnSubmit(model);
                _db.SubmitChanges();

                return Json(new { success = true, message = "Thêm khuyến mãi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/Promo/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.PromoId = id;
            return View();
        }

        // GET: Admin/Promo/GetPromoById/5 - AJAX
        [HttpGet]
        public ActionResult GetPromoById(int id)
        {
            try
            {
                var promoEntity = _db.Promotions.FirstOrDefault(p => p.Id == id);
                if (promoEntity == null)
                    return Json(new { success = false, message = "Không tìm thấy" }, JsonRequestBehavior.AllowGet);

                var promo = new
                {
                    promoEntity.Id,
                    promoEntity.Code,
                    promoEntity.Description,
                    promoEntity.Type,
                    promoEntity.Value,

                    StartDate = promoEntity.StartDate.HasValue
                        ? promoEntity.StartDate.Value.ToString("yyyy-MM-dd")
                        : null,

                    EndDate = promoEntity.EndDate.HasValue
                        ? promoEntity.EndDate.Value.ToString("yyyy-MM-dd")
                        : null,

                    /*UsageLimit = promoEntity.UsageLimit ?? 0,
                    PerUserLimit = promoEntity.PerUserLimit ?? 0,*/
                    promoEntity.IsActive
                };

                return Json(new { success = true, data = promo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/Promo/UpdatePromo - AJAX
        [HttpPost]
        public ActionResult UpdatePromo(Promotion model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

                var promo = _db.Promotions.FirstOrDefault(p => p.Id == model.Id);
                if (promo == null)
                    return Json(new { success = false, message = "Không tìm thấy khuyến mãi" });

                // Kiểm tra trùng mã nếu thay đổi
                if (promo.Code != model.Code && _db.Promotions.Any(p => p.Code == model.Code && p.Id != model.Id))
                    return Json(new { success = false, message = "Mã khuyến mãi đã tồn tại" });

                /*// === RÀNG BUỘC QUAN TRỌNG: Không cho Tạm dừng nếu đang có hiệu lực và đã được dùng ===
                if ((bool)!model.IsActive) // đang cố tắt
                {
                    bool isCurrentlyActive = promo.StartDate <= DateTime.Today &&
                                           (!promo.EndDate.HasValue || promo.EndDate >= DateTime.Today);

                    if (isCurrentlyActive && promo.UsedCount > 0)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Không thể tạm dừng khuyến mãi vì đang trong thời gian áp dụng và đã có khách hàng sử dụng!"
                        });
                    }
                }*/

                // === Cập nhật các trường ===
                promo.Code = model.Code.Trim().ToUpper();
                promo.Description = model.Description?.Trim();
                promo.Type = model.Type;
                promo.Value = model.Value;
                promo.StartDate = model.StartDate;
                promo.EndDate = model.EndDate;
                /*promo.UsageLimit = model.UsageLimit;
                promo.PerUserLimit = model.PerUserLimit;*/
                promo.IsActive = model.IsActive;           
                promo.UpdatedAt = DateTime.Now;

                _db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật khuyến mãi thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // POST: Admin/Promo/DeletePromo - AJAX
        [HttpPost]
        public ActionResult DeletePromo(int id)
        {
            try
            {
                var promo = _db.Promotions.FirstOrDefault(p => p.Id == id);
                if (promo != null)
                {
                    promo.IsActive = false;
                    promo.DeletedAt = DateTime.Now;
                    _db.SubmitChanges();

                    return Json(new { success = true, message = "Xóa thành công" });
                }
                return Json(new { success = false, message = "Không tìm thấy khuyến mãi" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/Promo/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var promo = _db.Promotions.FirstOrDefault(p => p.Id == id);
                if (promo == null)
                    return HttpNotFound();

                return View(promo);
            }
            catch
            {
                return HttpNotFound();
            }
        }
    }
}