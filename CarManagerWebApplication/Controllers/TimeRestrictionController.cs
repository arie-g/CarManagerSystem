using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CarManagerWebApplication.Models;
using Dal;
using WebMatrix.WebData;
using Role_TimeRestriction = Dal.Role_TimeRestriction;

namespace CarManagerWebApplication.Controllers
{
    public class TimeRestrictionController : Controller
    {

        // GET: CarsToDriver
        public ActionResult Index(string companyName, string carId, string driverId)
        {
            List<WrapTimeRestriction> timeRestrictions = new List<WrapTimeRestriction>();

            Guid? companyGuid = RoleService.GetCompanyId(companyName);
            List<Guid> carsToDriversId;

            if (!companyGuid.HasValue)
            {
                return RedirectToAction("Index", "Home");
//                throw new Exception("Can't Add Time Restriction. Bad Company ID");
            }
            else
            {
                try
                {
                    UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyGuid.Value,
                        WebSecurity.CurrentUserId);
                    if (roleOfAction == UsersManager.RoleStatus.Anonymous)
                    {
                        return RedirectToAction("Index", "Home");

                        //throw new Exception("Can't Add Time Restriction. Authorization Problem");
                    }

                    using (CarManagerDbEntities db = new CarManagerDbEntities())
                    {

                        List<Guid> carIdsInCompany =
                            db.Cars.Where(car => car.CompanyId == companyGuid.Value)
                                .Select(u => u.Id).ToList<Guid>();

                        if (carId != null)
                        {
                            Guid guid = new Guid(carId);
                            if (driverId == null)
                            {
                                if (roleOfAction == UsersManager.RoleStatus.Driver)
                                {
                                    Guid? driverGuid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                                    if (driverGuid.HasValue)
                                    {
                                        carsToDriversId = (from ctd in db.CarsToDriverViews
                                            where
                                                ctd.CarId == guid && carIdsInCompany.Contains(ctd.CarId) &&
                                                ctd.DriverId == driverGuid.Value
                                            select ctd.Id).ToList();
                                    }
                                    else
                                    {
                                        return RedirectToAction("Index", "Home");
                                    }
                                }
                                else
                                {
                                    carsToDriversId = (from ctd in db.CarsToDriverViews
                                        where ctd.CarId == guid && carIdsInCompany.Contains(ctd.CarId)
                                        select ctd.Id).ToList();
                                }
                            }
                            else
                            {
                                Guid driverIdGuid = new Guid(driverId);
                                if (roleOfAction == UsersManager.RoleStatus.Driver)
                                {
                                    Guid? driverGuid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                                    if (!driverGuid.HasValue || driverGuid.Value != driverIdGuid)
                                    {
                                        return RedirectToAction("Index", "Home");
                                    }

                                }
                                carsToDriversId = (from ctd in db.CarsToDriverViews
                                    where
                                        ctd.CarId == guid && carIdsInCompany.Contains(ctd.CarId) &&
                                        ctd.DriverId == driverIdGuid
                                    select ctd.Id).ToList();
                            }
                        }
                        else if (driverId != null)
                        {
                            Guid driverIdGuid = new Guid(driverId);
                            if (roleOfAction == UsersManager.RoleStatus.Driver)
                            {
                                Guid? driverGuid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                                if (!driverGuid.HasValue || driverGuid.Value != driverIdGuid)
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }

                            carsToDriversId = (from ctd in db.CarsToDriverViews
                                where ctd.DriverId == driverIdGuid && carIdsInCompany.Contains(ctd.CarId)
                                select ctd.Id).ToList();
                        }
                        else
                        {
                            if (roleOfAction == UsersManager.RoleStatus.Driver)
                            {
                                Guid? driverGuid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                                if (driverGuid.HasValue)
                                {
                                    carsToDriversId = (from ctd in db.CarsToDriverViews
                                        where
                                            carIdsInCompany.Contains(ctd.CarId) &&
                                            ctd.DriverId == driverGuid.Value
                                        select ctd.Id).ToList();
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                            }
                            else
                            {
                                carsToDriversId = (from ctd in db.CarsToDriverViews
                                    where carIdsInCompany.Contains(ctd.CarId)
                                    select ctd.Id).ToList();
                            }
                        }
                        foreach (Role_TimeRestriction restriction in db.Role_TimeRestriction)
                        {
                            if (carsToDriversId.Contains(restriction.CarToDriverId))
                            {
                                var carToDriver = db.CarsToDrivers.Find(restriction.CarToDriverId);
                                Car car = db.Cars.Find(carToDriver.CarId);
                                Driver driver = db.Drivers.Find(carToDriver.DriverId);
                                
                                timeRestrictions.Add(new WrapTimeRestriction()
                                {Car = car, Driver = driver, TimeRestriction = restriction});
                            }
                        }
                    }
                }
                catch
                    (Exception )
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(timeRestrictions);
        }

    [HttpPost]
        public ActionResult Remove(string companyName, string id)
        {
            try
            {
                Guid guid = new Guid(id);
                Guid? companyGuid = RoleService.GetCompanyId(companyName);
                if (!companyGuid.HasValue)
                {
                    return Content(Boolean.FalseString);
                }

                UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyGuid.Value, WebSecurity.CurrentUserId);
                if (roleOfAction != UsersManager.RoleStatus.Admin)
                {
                    return Content(Boolean.FalseString);
                }
                using (CarManagerDbEntities db = new CarManagerDbEntities())
                {
                    //CarsToDriver ctd = db.CarsToDrivers.Find(guid);
                    List<Role_TimeRestriction> restrictions = db.Role_TimeRestriction.Where(restriction => restriction.CarToDriverId == guid).ToList() ;

                    if (restrictions.Count == 0)
                    {
                        return RedirectToAction("Index", "Home");

                    }
                    else if (restrictions.Count > 1)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                       Role_TimeRestriction restriction = restrictions[0];
                        db.Role_TimeRestriction.Remove(restriction);
                        db.SaveChanges();
                    }
                }

                return Content(Boolean.TrueString);
            }
            catch (Exception )
            {
                return Content(Boolean.FalseString);
            }
        }

        public ActionResult Add(string companyName, string carId, string driverId)
        {
            AddTimeRestrictionModel model = new AddTimeRestrictionModel();
            
            Guid? companyGuid = RoleService.GetCompanyId(companyName);
            
            if (!companyGuid.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyGuid.Value, WebSecurity.CurrentUserId);
                if (roleOfAction != UsersManager.RoleStatus.Admin)
                {
                    return RedirectToAction("Index", "Home");

                    //throw new Exception("Can't Add Time Restriction. Authorization Problem");
                }

                try
                {
                    using (CarManagerDbEntities db = new CarManagerDbEntities())
                    {
                        var drivers = 
                            (from driverToCompany in db.DriverToCompanies
                            where driverToCompany.CompanyId == companyGuid.Value
                            select driverToCompany.Driver).ToList();

                        var cars = new List<Car>();

                        foreach (Driver driver in drivers)
                        {
                            var tempCars =
                            (from carsToDriver in db.CarsToDrivers
                             where carsToDriver.DriverId == driver.Id
                             select carsToDriver.Car).ToList();

                            cars.AddRange(tempCars);
                        }

                        cars = cars.Distinct().ToList();

                        model.Cars = cars;
                        model.Drivers = drivers;
                    }
                }
                catch (Exception )
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Save(AddTimeRestrictionModel model)
        {
            int addedCount = 0;

            if (model.SelectedDrivers == null || model.SelectedDrivers.Count == 0 ||
                model.SelectedCars == null || model.SelectedCars.Count == 0)
            {
                Response.StatusCode = 406; // Or any other proper status code.
                Response.Write("Driver or Car are not selected");
                return null;
            }
            else
            {
                try
                {
                    using (CarManagerDbEntities db = new CarManagerDbEntities())
                    {
                        foreach (Guid driverGuid in model.SelectedDrivers)
                        {
                            foreach (Guid carGuid in model.SelectedCars)
                            {
                                var ctds = db.CarsToDrivers.Where(ctd => ctd.DriverId == driverGuid && ctd.CarId == carGuid).ToList();

                                if (ctds.Count > 1)
                                {
                                    Driver driver = db.Drivers.Find(driverGuid);
                                    Car car = db.Cars.Find(carGuid);

                                    return RedirectToAction("Index", "Home");
                                }
                                else if (ctds.Count == 1)
                                {
                                    var ctd = ctds.First();
                                    var rests = db.Role_TimeRestriction.Where(limit => limit.CarToDriverId == ctd.Id).ToList();

                                    if (rests.Count == 0)
                                    {
                                        var timeRestriction = new Role_TimeRestriction()
                                        {
                                            CarToDriverId = ctd.Id,
                                            StartTime = model.StartTime,
                                            EndTime = model.EndTime
                                        };
                                        db.Role_TimeRestriction.Add(timeRestriction);
                                        addedCount++;
                                    }
                                }
                            }
                        }

                        db.SaveChanges();
                    }
                }
                catch (Exception )
                {
                    Response.StatusCode = 406; // Or any other proper status code.
                    Response.Write("Exception occured.");
                    return null;
                }
            }
            return Json(string.Format("Added {0} new time restrictions.", addedCount));
        }
    }
}