using HotelBooking.Models;
using HotelBooking.Services.Interfaces;
using System;
using System.Linq;

namespace HotelBooking.Services.Implementations
{
    public class PromoService : IPromoService
    {
        private readonly DatabaseDataContext _db;

        public PromoService()
        {
            _db = new DatabaseDataContext();
        }

        //public List<Promotion> GetAllPromotions()
        //{
        //    return _db.Promotions.Where(p => p.IsActive && p.DeletedAt == null).ToList();
        //}

        //public Promotion GetPromotionByCode(string code)
        //{
        //    return _db.Promotions.FirstOrDefault(p => p.Code == code && p.IsActive);
        //}

        public void CreatePromotion(Promotion promo)
        {
            promo.IsActive = true;
            promo.CreatedAt = DateTime.Now;
            promo.UpdatedAt = DateTime.Now;
            promo.UsedCount = 0;
            _db.Promotions.InsertOnSubmit(promo);
            _db.SubmitChanges();
        }

        public void UpdatePromotion(Promotion promo)
        {
            var existing = _db.Promotions.FirstOrDefault(p => p.Id == promo.Id);
            if (existing != null)
            {
                existing.Description = promo.Description;
                existing.Type = promo.Type;
                existing.Value = promo.Value;
                existing.StartDate = promo.StartDate;
                existing.EndDate = promo.EndDate;
                existing.UsageLimit = promo.UsageLimit;
                existing.UpdatedAt = DateTime.Now;
                _db.SubmitChanges();
            }
        }

        public void DeletePromotion(int id)
        {
            var promo = _db.Promotions.FirstOrDefault(p => p.Id == id);
            if (promo != null)
            {
                promo.IsActive = false;
                promo.DeletedAt = DateTime.Now;
                _db.SubmitChanges();
            }
        }

        //public bool ValidatePromoCode(string code, int userId)
        //{
        //    var promo = GetPromotionByCode(code);
        //    if (promo == null) return false;

        //    var now = DateTime.Now;
        //    if (promo.StartDate.HasValue && now < promo.StartDate.Value) return false;
        //    if (promo.EndDate.HasValue && now > promo.EndDate.Value) return false;
        //    if (promo.UsageLimit.HasValue && promo.UsedCount >= promo.UsageLimit.Value) return false;

        //    return true;
        //}
    }
}