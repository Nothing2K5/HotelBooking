using HotelBooking.Models;
using HotelBooking.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBooking.Services.Implementations
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly DatabaseDataContext _db;

        public LoyaltyService()
        {
            _db = new DatabaseDataContext();
        }

        public List<LoyaltyTier> GetAllTiers()
        {
            return _db.LoyaltyTiers.OrderBy(t => t.DiscountPercent).ToList();
        }

        public LoyaltyTier GetTierById(int id)
        {
            return _db.LoyaltyTiers.FirstOrDefault(t => t.Id == id);
        }

        public void CreateTier(LoyaltyTier tier)
        {
            tier.CreatedAt = DateTime.Now;
            _db.LoyaltyTiers.InsertOnSubmit(tier);
            _db.SubmitChanges();
        }

        public void UpdateTier(LoyaltyTier tier)
        {
            var existing = _db.LoyaltyTiers.FirstOrDefault(t => t.Id == tier.Id);
            if (existing != null)
            {
                existing.Name = tier.Name;
                existing.DiscountPercent = tier.DiscountPercent;
                existing.Multiplier = tier.Multiplier;
                _db.SubmitChanges();
            }
        }

        public void DeleteTier(int id)
        {
            var tier = _db.LoyaltyTiers.FirstOrDefault(t => t.Id == id);
            if (tier != null)
            {
                _db.LoyaltyTiers.DeleteOnSubmit(tier);
                _db.SubmitChanges();
            }
        }

        public int GetCustomerPoints(int userId)
        {
            var customer = _db.Customers.FirstOrDefault(c => c.UserId == userId);
            return customer?.TotalPoints ?? 0;
        }

        public void AddPoints(int userId, int points, int? bookingId, string reason)
        {
            var loyaltyPoint = new LoyaltyPoint
            {
                UserId = userId,
                BookingId = bookingId,
                Points = points,
                Reason = reason,
                CreatedAt = DateTime.Now
            };
            _db.LoyaltyPoints.InsertOnSubmit(loyaltyPoint);

            var customer = _db.Customers.FirstOrDefault(c => c.UserId == userId);
            if (customer != null)
            {
                customer.TotalPoints += points;
            }

            _db.SubmitChanges();
        }
    }
}