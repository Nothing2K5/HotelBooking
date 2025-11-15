namespace HotelBooking.Services.Interfaces
{
    public interface IPaymentService
    {
        bool ProcessOnlinePayment(int bookingId, decimal amount);
        bool ProcessDirectPayment(int bookingId);
        void ProcessRefund(int bookingId, decimal amount);
    }
}