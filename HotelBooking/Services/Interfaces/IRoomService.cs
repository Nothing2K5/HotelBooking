using HotelBooking.Models;

namespace HotelBooking.Services.Interfaces
{
    public interface IRoomService
    {
        //List<Room> GetAvailableRooms(int hotelId, DateTime checkIn, DateTime checkOut);
        //Room GetRoomById(int id);
        void CreateRoom(Room room);
        void UpdateRoom(Room room);
        void DeleteRoom(int id);
    }
}