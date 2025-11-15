namespace HotelBooking.Models
{
    public partial class Hotel
    {
        // Business logic methods (nếu cần)
        public string GetFullAddress()
        {
            return $"{Address}, {City}, {Country}";
        }
    }
}