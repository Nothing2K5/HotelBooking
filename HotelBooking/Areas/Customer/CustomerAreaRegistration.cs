using System.Web.Mvc;

namespace HotelBooking.Areas.Customer
{
    public class CustomerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Customer";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Customer_default",
                "Customer/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "HotelBooking.Areas.Customer.Controllers" } // tùy chọn nhưng tốt khi có
            );
        }
    }
}