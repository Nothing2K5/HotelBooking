namespace HotelBooking.ViewModels
{
    public class CancelBookingVM
    {
        public int BookingId { get; set; }
        public string Reason { get; set; }
        public bool IsFreeCancellation { get; set; }
    }
}