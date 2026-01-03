using HotelBooking.Models;
using HotelBooking.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBooking.Services.Implementations
{
    public class HotelService : IHotelService
    {
        private readonly DatabaseDataContext _db;

        public HotelService()
        {
            _db = new DatabaseDataContext();
        }

        public List<Hotel> SearchHotels(string location, DateTime checkIn, DateTime checkOut)
        {
            return _db.Hotels
                .Where(h => h.IsActive &&
                       (h.City.Contains(location) || h.Address.Contains(location)))
                .ToList();
        }

        public Hotel GetHotelById(int id)
        {
            return _db.Hotels.FirstOrDefault(h => h.Id == id && h.IsActive);
        }

        public void CreateHotel(Hotel hotel)
        {
            hotel.IsActive = true;
            hotel.CreatedAt = DateTime.Now;
            hotel.UpdatedAt = DateTime.Now;
            _db.Hotels.InsertOnSubmit(hotel);
            _db.SubmitChanges();
        }

        public void UpdateHotel(Hotel hotel)
        {
            var existingHotel = _db.Hotels.FirstOrDefault(h => h.Id == hotel.Id);
            if (existingHotel != null)
            {
                existingHotel.Name = hotel.Name;
                existingHotel.Address = hotel.Address;
                existingHotel.City = hotel.City;
                existingHotel.Country = hotel.Country;
                existingHotel.StarRating = hotel.StarRating;
                existingHotel.Description = hotel.Description;
                existingHotel.UpdatedAt = DateTime.Now;
                _db.SubmitChanges();
            }
        }

        public void DeleteHotel(int id)
        {
            var hotel = _db.Hotels.FirstOrDefault(h => h.Id == id);
            if (hotel != null)
            {
                hotel.IsActive = false;
                hotel.DeletedAt = DateTime.Now;
                _db.SubmitChanges();
            }
        }
    }
}