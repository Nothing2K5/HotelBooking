using HotelBooking.Models;
using HotelBooking.ViewModels;
using System.Collections.Generic;

namespace HotelBooking.Services.Interfaces
{
    public interface IBookingService
    {
        List<Booking> GetUserBookings(int userId);
        Booking GetBookingById(int id);
        Booking CreateBooking(BookingCreateVM model, int userId);
        void UpdateBooking(Booking booking);
        void CancelBooking(int bookingId, bool isFreeCancellation);
        decimal CalculateTotalAmount(BookingCreateVM model);
    }
}