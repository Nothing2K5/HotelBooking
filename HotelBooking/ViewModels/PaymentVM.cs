namespace HotelBooking.ViewModels
{
    public class PaymentVM
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // "online" hoặc "direct"
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }
}