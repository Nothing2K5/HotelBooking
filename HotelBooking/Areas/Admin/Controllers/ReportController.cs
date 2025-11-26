using HotelBooking.Models;
using System.Web.Mvc;

namespace HotelBooking.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    public class ReportController : Controller
    {
        private readonly DatabaseDataContext _db;

        public ReportController()
        {
            _db = new DatabaseDataContext();
        }

        // GET: Admin/Report/Revenue
        public ActionResult Revenue()
        {
            return View();
        }

        //// GET: Admin/Report/GetRevenueReport - AJAX
        //[HttpGet]
        //public ActionResult GetRevenueReport(DateTime? startDate, DateTime? endDate)
        //{
        //    try
        //    {
        //        var start = startDate ?? DateTime.Now.AddMonths(-1);
        //        var end = endDate ?? DateTime.Now;

        //        // Total Revenue
        //        var totalRevenue = _db.Bookings
        //            .Where(b => b.Status == "paid" && b.CreatedAt >= start && b.CreatedAt <= end)
        //            .Sum(b => (decimal?)b.TotalAmount) ?? 0;

        //        // Total Paid Bookings
        //        var totalPaid = _db.Bookings
        //            .Count(b => b.Status == "paid" && b.CreatedAt >= start && b.CreatedAt <= end);

        //        // Total Cancelled Bookings
        //        var totalCancelled = _db.Bookings
        //            .Count(b => b.Status == "cancelled" && b.CreatedAt >= start && b.CreatedAt <= end);

        //        // Revenue by Hotel
        //        var revenueByHotel = _db.Bookings
        //            .Where(b => b.Status == "paid" && b.CreatedAt >= start && b.CreatedAt <= end)
        //            .GroupBy(b => b.Hotel.Name)
        //            .Select(g => new
        //            {
        //                HotelName = g.Key,
        //                Count = g.Count(),
        //                Revenue = g.Sum(b => b.TotalAmount)
        //            })
        //            .ToDictionary(x => x.HotelName, x => new { x.Count, x.Revenue });

        //        return Json(new
        //        {
        //            success = true,
        //            data = new
        //            {
        //                totalRevenue,
        //                totalPaid,
        //                totalCancelled,
        //                revenueByHotel
        //            }
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        //// GET: Admin/Report/GetBookingStats - AJAX
        //[HttpGet]
        //public ActionResult GetBookingStats(DateTime? startDate, DateTime? endDate)
        //{
        //    try
        //    {
        //        var start = startDate ?? DateTime.Now.AddMonths(-1);
        //        var end = endDate ?? DateTime.Now;

        //        var bookingsByStatus = _db.Bookings
        //            .Where(b => b.CreatedAt >= start && b.CreatedAt <= end)
        //            .GroupBy(b => b.Status)
        //            .Select(g => new
        //            {
        //                Status = g.Key,
        //                Count = g.Count()
        //            })
        //            .ToDictionary(x => x.Status, x => x.Count);

        //        return Json(new
        //        {
        //            success = true,
        //            data = bookingsByStatus
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}