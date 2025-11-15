using System.ComponentModel.DataAnnotations;

namespace HotelBooking.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}