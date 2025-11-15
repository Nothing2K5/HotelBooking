using HotelBooking.Models;
using HotelBooking.Services.Interfaces;
using System;
using System.Linq;

namespace HotelBooking.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly DatabaseDataContext _db;

        public RoomService()
        {
            _db = new DatabaseDataContext();
        }

        //public List<Room> GetAvailableRooms(int hotelId, DateTime checkIn, DateTime checkOut)
        //{
        //    return _db.Rooms
        //        .Where(r => r.HotelId == hotelId && r.IsActive)
        //        .ToList(); // Simplified - nên check RoomInventory
        //}

        //public Room GetRoomById(int id)
        //{
        //    return _db.Rooms.FirstOrDefault(r => r.Id == id && r.IsActive);
        //}

        public void CreateRoom(Room room)
        {
            room.IsActive = true;
            room.CreatedAt = DateTime.Now;
            room.UpdatedAt = DateTime.Now;
            _db.Rooms.InsertOnSubmit(room);
            _db.SubmitChanges();
        }

        public void UpdateRoom(Room room)
        {
            var existingRoom = _db.Rooms.FirstOrDefault(r => r.Id == room.Id);
            if (existingRoom != null)
            {
                existingRoom.Name = room.Name;
                existingRoom.Description = room.Description;
                existingRoom.Capacity = room.Capacity;
                existingRoom.PricePerNight = room.PricePerNight;
                existingRoom.TotalRooms = room.TotalRooms;
                existingRoom.UpdatedAt = DateTime.Now;
                _db.SubmitChanges();
            }
        }

        public void DeleteRoom(int id)
        {
            var room = _db.Rooms.FirstOrDefault(r => r.Id == id);
            if (room != null)
            {
                room.IsActive = false;
                room.DeletedAt = DateTime.Now;
                _db.SubmitChanges();
            }
        }
    }
}