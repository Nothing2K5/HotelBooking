using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace HotelBooking
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // QUAN TRỌNG: Parse Role từ Cookie
        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                try
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                    if (ticket != null && !ticket.Expired)
                    {
                        string role = ticket.UserData; // Lấy role từ UserData

                        // Tạo GenericPrincipal với role
                        GenericIdentity identity = new GenericIdentity(ticket.Name, "Forms");
                        GenericPrincipal principal = new GenericPrincipal(identity, new string[] { role });

                        // Set User với role
                        Context.User = principal;
                    }
                }
                catch
                {
                    // Invalid ticket
                    FormsAuthentication.SignOut();
                }
            }
        }
    }
}