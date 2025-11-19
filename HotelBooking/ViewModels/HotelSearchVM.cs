using System;
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.ViewModels
{
    public class HotelSearchVM
    {
        [Required(ErrorMessage = "Vui lòng nhập địa điểm")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày check-in")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày check-out")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 10, ErrorMessage = "Số khách từ 1-10")]
        public int Guests { get; set; } = 1;
    }
}