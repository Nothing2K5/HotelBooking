using System.Configuration;

namespace HotelBooking.Models   // đặt đúng namespace giống designer.cs
{
    public partial class DatabaseDataContext
    {
        public DatabaseDataContext()
            : base(ConfigurationManager.ConnectionStrings["HotelBookingConnectionString"].ConnectionString)
        {
        }
    }
}
