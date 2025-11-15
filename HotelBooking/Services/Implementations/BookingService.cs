using HotelBooking.Models;
using HotelBooking.Services.Interfaces;
using HotelBooking.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBooking.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly DatabaseDataContext _db;

        public BookingService()
        {
            _db = new DatabaseDataContext();
        }

        public List<Booking> GetUserBookings(int userId)
        {
            return _db.Bookings
                .Where(b => b.UserId == userId && b.DeletedAt == null)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
        }

        public Booking GetBookingById(int id)
        {
            return _db.Bookings.FirstOrDefault(b => b.Id == id && b.DeletedAt == null);
        }

        public Booking CreateBooking(BookingCreateVM model, int userId)
        {
            var booking = new Booking
            {
                UserId = userId,
                HotelId = model.HotelId,
                Code = GenerateBookingCode(),
                Status = "draft",
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                Guests = model.Guests,
                TotalAmount = CalculateTotalAmount(model),
                FreeCancellationDeadline = model.CheckInDate.AddDays(-2),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Note = model.Note
            };

            _db.Bookings.InsertOnSubmit(booking);
            _db.SubmitChanges();

            // Tạo BookingItem
            var room = _db.Rooms.FirstOrDefault(r => r.Id == model.RoomId);
            if (room != null)
            {
                var nights = (model.CheckOutDate - model.CheckInDate).Days;
                var bookingItem = new BookingItem
                {
                    BookingId = booking.Id,
                    RoomId = model.RoomId,
                    PricePerNight = room.PricePerNight,
                    Nights = nights,
                    Quantity = 1,
                    SubTotal = room.PricePerNight * nights
                };
                _db.BookingItems.InsertOnSubmit(bookingItem);
                _db.SubmitChanges();
            }

            return booking;
        }

        public void UpdateBooking(Booking booking)
        {
            var existingBooking = _db.Bookings.FirstOrDefault(b => b.Id == booking.Id);
            if (existingBooking != null)
            {
                existingBooking.CheckInDate = booking.CheckInDate;
                existingBooking.CheckOutDate = booking.CheckOutDate;
                existingBooking.Guests = booking.Guests;
                existingBooking.UpdatedAt = DateTime.Now;
                _db.SubmitChanges();
            }
        }

        public void CancelBooking(int bookingId, bool isFreeCancellation)
        {
            var booking = _db.Bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking != null)
            {
                booking.Status = "cancelled";
                booking.CancelledAt = DateTime.Now;

                if (!isFreeCancellation)
                {
                    booking.PenaltyAmount = booking.TotalAmount * 0.5m; // 50% phí
                }

                _db.SubmitChanges();
            }
        }

        public decimal CalculateTotalAmount(BookingCreateVM model)
        {
            var room = _db.Rooms.FirstOrDefault(r => r.Id == model.RoomId);
            if (room == null) return 0;

            var nights = (model.CheckOutDate - model.CheckInDate).Days;
            return room.PricePerNight * nights;
        }

        private string GenerateBookingCode()
        {
            return "BK" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}