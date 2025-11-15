using HotelBooking.Models;
using HotelBooking.Services.Interfaces;
using System;
using System.Linq;

namespace HotelBooking.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly DatabaseDataContext _db;

        public PaymentService()
        {
            _db = new DatabaseDataContext();
        }

        public bool ProcessOnlinePayment(int bookingId, decimal amount)
        {
            // Mô phỏng thanh toán online thành công
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                Status = "success",
                PaidAt = DateTime.Now,
                CreatedAt = DateTime.Now
            };

            _db.Payments.InsertOnSubmit(payment);

            var booking = _db.Bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking != null)
            {
                booking.Status = "paid";
                booking.UpdatedAt = DateTime.Now;
            }

            _db.SubmitChanges();
            return true;
        }

        public bool ProcessDirectPayment(int bookingId)
        {
            var booking = _db.Bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking != null)
            {
                booking.Status = "pending";
                booking.UpdatedAt = DateTime.Now;
                _db.SubmitChanges();
                return true;
            }
            return false;
        }

        public void ProcessRefund(int bookingId, decimal amount)
        {
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = -amount, // Số âm để đánh dấu refund
                Status = "refunded",
                CreatedAt = DateTime.Now
            };

            _db.Payments.InsertOnSubmit(payment);
            _db.SubmitChanges();
        }
    }
}