using HotelBooking.Models;
using HotelBooking.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelBooking.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly DatabaseDataContext _db;

        public ReviewService()
        {
            _db = new DatabaseDataContext();
        }

        public List<Review> GetHotelReviews(int hotelId)
        {
            return _db.Reviews
                .Where(r => r.HotelId == hotelId && r.DeletedAt == null)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public List<Review> GetUserReviews(int userId)
        {
            return _db.Reviews
                .Where(r => r.UserId == userId && r.DeletedAt == null)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        public Review GetReviewById(int id)
        {
            return _db.Reviews.FirstOrDefault(r => r.Id == id && r.DeletedAt == null);
        }

        public void CreateReview(Review review)
        {
            review.CreatedAt = DateTime.Now;
            review.UpdatedAt = DateTime.Now;
            _db.Reviews.InsertOnSubmit(review);
            _db.SubmitChanges();
        }

        public void UpdateReview(Review review)
        {
            var existing = _db.Reviews.FirstOrDefault(r => r.Id == review.Id);
            if (existing != null)
            {
                existing.Rating = review.Rating;
                existing.Title = review.Title;
                existing.Content = review.Content;
                existing.UpdatedAt = DateTime.Now;
                _db.SubmitChanges();
            }
        }

        public void DeleteReview(int id)
        {
            var review = _db.Reviews.FirstOrDefault(r => r.Id == id);
            if (review != null)
            {
                review.DeletedAt = DateTime.Now;
                _db.SubmitChanges();
            }
        }
    }
}