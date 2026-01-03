using HotelBooking.Models;
using HotelBooking.Services.Interfaces;

namespace HotelBooking.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly DatabaseDataContext _db;

        //public ReportService()
        //{
        //    _db = new DatabaseDataContext();
        //}

        //public decimal GetRevenueByDateRange(DateTime startDate, DateTime endDate)
        //{
        //    return _db.Bookings
        //        .Where(b => b.Status == "paid" &&
        //                   b.CreatedAt >= startDate &&
        //                   b.CreatedAt <= endDate)
        //        .Sum(b => (decimal?)b.TotalAmount) ?? 0;
        //}

        //public Dictionary<string, decimal> GetRevenueByHotel(DateTime startDate, DateTime endDate)
        //{
        //    return _db.Bookings
        //        .Where(b => b.Status == "paid" &&
        //                   b.CreatedAt >= startDate &&
        //                   b.CreatedAt <= endDate)
        //        .GroupBy(b => b.Hotel.Name)
        //        .ToDictionary(g => g.Key, g => g.Sum(b => b.TotalAmount));
        //}

        //public Dictionary<string, int> GetBookingStatsByStatus(DateTime startDate, DateTime endDate)
        //{
        //    return _db.Bookings
        //        .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
        //        .GroupBy(b => b.Status)
        //        .ToDictionary(g => g.Key, g => g.Count());
        //}
    }
}