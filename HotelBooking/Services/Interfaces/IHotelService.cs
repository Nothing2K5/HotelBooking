using HotelBooking.Models;
using System;
using System.Collections.Generic;

namespace HotelBooking.Services.Interfaces
{
    public interface IHotelService
    {
        List<Hotel> SearchHotels(string location, DateTime checkIn, DateTime checkOut);
        Hotel GetHotelById(int id);
        void CreateHotel(Hotel hotel);
        void UpdateHotel(Hotel hotel);
        void DeleteHotel(int id);
    }
}