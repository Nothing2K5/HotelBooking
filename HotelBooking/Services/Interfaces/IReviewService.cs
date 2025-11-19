using HotelBooking.Models;
using System.Collections.Generic;

namespace HotelBooking.Services.Interfaces
{
    public interface IReviewService
    {
        List<Review> GetHotelReviews(int hotelId);
        List<Review> GetUserReviews(int userId);
        Review GetReviewById(int id);
        void CreateReview(Review review);
        void UpdateReview(Review review);
        void DeleteReview(int id);
    }
}