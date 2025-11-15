namespace HotelBooking.Models
{
    public partial class User
    {
        // Thêm helper methods nếu cần
        public bool IsAdmin()
        {
            return Role == "admin";
        }
    }
}