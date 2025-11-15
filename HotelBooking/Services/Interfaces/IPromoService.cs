using HotelBooking.Models;

namespace HotelBooking.Services.Interfaces
{
    public interface IPromoService
    {
        //List<Promotion> GetAllPromotions();
        //Promotion GetPromotionByCode(string code);
        void CreatePromotion(Promotion promo);
        void UpdatePromotion(Promotion promo);
        void DeletePromotion(int id);
        //bool ValidatePromoCode(string code, int userId);
    }
}