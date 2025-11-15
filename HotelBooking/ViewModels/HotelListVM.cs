using HotelBooking.Models;
using System.Collections.Generic;

namespace HotelBooking.ViewModels
{
    public class HotelListVM
    {
        public List<Hotel> Hotels { get; set; }
        public HotelSearchVM SearchCriteria { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}