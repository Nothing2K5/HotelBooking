using System;

namespace HotelBooking.ViewModels
{
    public class BookingDetailsVM
    {
        public int BookingId { get; set; }
        public string BookingCode { get; set; }
        public string HotelName { get; set; }
        public string RoomName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Guests { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public bool CanCancel { get; set; }
        public bool IsFreeCancellation { get; set; }
        public decimal PenaltyAmount { get; set; }
    }
}