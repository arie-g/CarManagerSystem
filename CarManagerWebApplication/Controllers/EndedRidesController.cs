using CarManagerWebApplication.Models;
using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using WebMatrix.WebData;

namespace CarManagerWebApplication.Controllers
{
    public class EndedRidesController : Controller
    {
        // GET: Rides
        public ActionResult Index(string companyName, string id)
        {
            WrapEndedRidesModel model = new WrapEndedRidesModel();
            Guid? companyGuid = RoleService.GetCompanyId(companyName);

            if (!companyGuid.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyGuid.Value, WebSecurity.CurrentUserId);
                if (roleOfAction == UsersManager.RoleStatus.Anonymous)
                {
                    return RedirectToAction("Index", "Home");

                }

                try
                {
                    Guid guid = new Guid(id);

                    using (CarManagerDbEntities db = new CarManagerDbEntities())
                    {
                        model.EndedRides =
                            db.EndedRides.Where<EndedRide>(endedRide => endedRide.DriverID == guid).ToList<EndedRide>();
                        model.Driver = db.Drivers.Find(guid);
                    }
                    if (roleOfAction == UsersManager.RoleStatus.Driver)
                    {
                        Guid? driverGuid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                        if (model.Driver == null || !driverGuid.HasValue || driverGuid.Value != model.Driver.Id)
                        {

                            return RedirectToAction("Index", "Home");

                        }
                    }
                  

                    return View(model);
                }
                catch (Exception )
                {
                    return RedirectToAction("Index", "Home");
                }

            }
        }

        public ActionResult RideDetails(string companyName, string id)
        {
            KeyValuePair<RideInfoSum, List<DrivePackage>> model = new KeyValuePair<RideInfoSum, List<DrivePackage>>(); 
            Guid? companyGuid = RoleService.GetCompanyId(companyName);

            if (!companyGuid.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyGuid.Value, WebSecurity.CurrentUserId);
                if (roleOfAction == UsersManager.RoleStatus.Anonymous)
                {
                    return RedirectToAction("Index", "Home");
                }
                Guid guid = new Guid(id);
                using (CarManagerDbEntities db = new CarManagerDbEntities())
                {

                    if (roleOfAction == UsersManager.RoleStatus.Driver)
                    {
                        Guid? driverId = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                        var ride = db.Rides.Find(guid);
                        if (ride == null || !driverId.HasValue || driverId.Value != ride.DriverID)
                        {
                            return RedirectToAction("Index", "Home");

                        }
                    }

                    var rideInfo = RoleService.RideInfoSummarize(guid);
                    var packageList =
                        db.DrivePackages.Where(package => package.RideId == guid).ToList<Dal.DrivePackage>();
                    model = new KeyValuePair<RideInfoSum, List<DrivePackage>>(rideInfo, packageList);
                }


                return View(model);
            }
        }
    }
}