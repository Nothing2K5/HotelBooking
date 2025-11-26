using HotelBooking.Models;
using System.Web.Mvc;

namespace HotelBooking.Controllers
{
    public class RoomController : Controller
    {
        private readonly DatabaseDataContext _db;

        public RoomController()
        {
            _db = new DatabaseDataContext();
        }

        //// GET: Room/Details/5
        //public ActionResult Details(int id)
        //{
        //    try
        //    {
        //        var room = _db.Rooms.FirstOrDefault(r => r.Id == id && r.IsActive);
        //        if (room == null)
        //            return HttpNotFound();

        //        return View(room);
        //    }
        //    catch
        //    {
        //        return HttpNotFound();
        //    }
        //}
    }
}