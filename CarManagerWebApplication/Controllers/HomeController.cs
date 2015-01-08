using CarManagerWebApplication.Models;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace CarManagerWebApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (!UsersManager.isAnonymus(WebSecurity.CurrentUserId))
            {
                return RedirectToAction("Authorized");
            }
            else
            {
                ViewBag.Message = "Welcome to CarManager";

                return View();    
            }
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Welcome to CarManager Solution";

        //    return View();
        //}

        public ActionResult Contact()
        {
            ViewBag.Message = "For more details connect: CarManager@gmail.com";

            return View();
        }
        [Authorize]
        public ActionResult Authorized()
        {
            UserSum model = RoleService.GetDriverSumByUserId(WebSecurity.CurrentUserId);
            
            return View(model);
        }
    }
}
