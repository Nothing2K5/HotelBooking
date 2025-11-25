using HotelBooking.Models;
using System.Collections.Generic;

namespace HotelBooking.Services.Interfaces
{
    public interface ILoyaltyService
    {
        List<LoyaltyTier> GetAllTiers();
        LoyaltyTier GetTierById(int id);
        void CreateTier(LoyaltyTier tier);
        void UpdateTier(LoyaltyTier tier);
        void DeleteTier(int id);
        int GetCustomerPoints(int userId);
        //void AddPoints(int userId, int points, int? bookingId, string reason);
    }
}