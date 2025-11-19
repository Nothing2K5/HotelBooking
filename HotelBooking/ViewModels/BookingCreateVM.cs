using System;
using System.ComponentModel.DataAnnotations;

namespace HotelBooking.ViewModels
{
    public class BookingCreateVM
    {
        public int HotelId { get; set; }
        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Range(1, 10)]
        public int Guests { get; set; }

        public string PromoCode { get; set; }
        public int? LoyaltyPointsToUse { get; set; }

        public string Note { get; set; }
    }
}