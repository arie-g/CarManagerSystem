using CarManagerWebApplication.Models;
using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace CarManagerWebApplication.Controllers
{
    public class CarsToDriverController : Controller
    {
        // GET: CarsToDriver
        public ActionResult Index(string companyName, string carId, string driverId)
        {
            List<CarsToDriverView> carsToDrivers = new List<CarsToDriverView>();
            Guid? companyId = RoleService.GetCompanyId(companyName);
            if (companyId.HasValue)
            {
                UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyId.Value, WebSecurity.CurrentUserId);
                if (roleOfAction == UsersManager.RoleStatus.Anonymous)
                {
                    return RedirectToAction("Index", "Home");
                }

                try
                {
                    using (CarManagerDbEntities db = new CarManagerDbEntities())
                    {
                        if (roleOfAction == UsersManager.RoleStatus.Driver)
                        {
                            Guid? driverGuid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                            if (driverGuid.HasValue)

                            {
                                List<CarsToDriverView> list = new List<CarsToDriverView>(); 
                                foreach (CarsToDriverView carsToDriverView in db.CarsToDriverViews)
                                {
                                    if (carsToDriverView.DriverId == driverGuid.Value)
                                    {
                                        Car car = db.Cars.Find(carsToDriverView.CarId);
                                        if (car.CompanyId == companyId)
                                        {
                                            list.Add(carsToDriverView);
                                        }
                                    }
                                }
                                carsToDrivers = list;
                            }
                        }
                        else if (carId != null)
                        {
                            Guid guid = new Guid(carId);
                            if (db.Cars.Find(guid).CompanyId == companyId)
                            {
                                carsToDrivers =
                                    db.CarsToDriverViews.Where(carToDriver => carToDriver.CarId == guid).ToList();
                            }
                        }
                        else if (driverId != null)
                        {
                            Guid guid = new Guid(driverId);
                            List<CarsToDriverView> list = new List<CarsToDriverView>();
                            foreach (CarsToDriverView carsToDriverView in db.CarsToDriverViews)
                            {
                                if (carsToDriverView.DriverId == guid)
                                {
                                    Car car = db.Cars.Find(carsToDriverView.CarId);
                                    if (car.CompanyId == companyId)
                                    {
                                        list.Add(carsToDriverView);
                                    }
                                }
                            }
                            carsToDrivers = list;
                        }
                        else
                        {
                            List<CarsToDriverView> list = new List<CarsToDriverView>();
                            foreach (CarsToDriverView carsToDriverView in db.CarsToDriverViews)
                            {
                                Car car = db.Cars.Find(carsToDriverView.CarId);
                                if (car.CompanyId == companyId)
                                {
                                    list.Add(carsToDriverView);
                                }
                            }
                            carsToDrivers = list;
                        }
                    }
                }
                catch (Exception )
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View(carsToDrivers);
        }

        [HttpPost]
        public ActionResult Remove(string companyName, string id)
        {
            Guid? companyId = RoleService.GetCompanyId(companyName);
            if (companyId.HasValue)
            {
                UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyId.Value, WebSecurity.CurrentUserId);
                if (roleOfAction == UsersManager.RoleStatus.Anonymous)
                {
                    return RedirectToAction("Index", "Home");
                }

                try
                {
                    Guid guid = new Guid(id);

                    using (CarManagerDbEntities db = new CarManagerDbEntities())
                    {
                        //CarsToDriver ctd = db.CarsToDrivers.Find(guid);
                        List<CarsToDriver> carToDrivers =
                            db.CarsToDrivers.Where(carToDriver => carToDriver.Id == guid).ToList();

                        if (carToDrivers.Count == 0)
                        {
                            return RedirectToAction("Index");
                        }
                        else if (carToDrivers.Count > 1)
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            if (roleOfAction == UsersManager.RoleStatus.Driver)
                            {
                                Guid? driverGuid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                                if (!driverGuid.HasValue || driverGuid.Value != carToDrivers.First().DriverId)
                                {
                                    return Content(Boolean.TrueString);
                                }
                            }
                            CarsToDriver ctd = carToDrivers[0];
                            db.CarsToDrivers.Remove(ctd);
                            db.SaveChanges();
                        }
                    }

                    return Content(Boolean.TrueString);
                }
                catch (Exception)
                {
                    return Content(Boolean.FalseString);
                }
            }
            else
            {
                    return Content(Boolean.FalseString);
            }
        }

        public ActionResult AddCarToDriver(string companyName, string carId, string driverId)
        {
            AddCarsToDriverModel model = new AddCarsToDriverModel();
            model.PossibleDrivers = new List<SelectListItem>();
            model.PossibleCars = new List<SelectListItem>();
            Guid? companyId = RoleService.GetCompanyId(companyName);
            if (companyId.HasValue)
            {
                UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyId.Value, WebSecurity.CurrentUserId);
                if (roleOfAction == UsersManager.RoleStatus.Anonymous)
                {
                    return RedirectToAction("Index", "Home");
                }

                try
                {
                    using (CarManagerDbEntities db = new CarManagerDbEntities())
                    {
                        if (carId != null)
                        {
                            Guid guid = new Guid(carId);
                            Car car = db.Cars.Find(guid);
                            if (car.CompanyId == companyId.Value)
                            {
                                SelectListItem dataToAdd = new SelectListItem()
                                {
                                    Value = car.Id.ToString(),
                                    Text = string.Format("{0} {1}", car.Number, car.Model)
                                };
                                model.PossibleCars.Add(dataToAdd);
                            }
                        }
                        else
                        {
                            foreach (Car car in db.Cars)
                            {
                                if (car.CompanyId == companyId.Value)
                                {
                                    SelectListItem dataToAdd = new SelectListItem()
                                    {
                                        Value = car.Id.ToString(),
                                        Text = string.Format("{0} {1}", car.Number, car.Model)
                                    };
                                    model.PossibleCars.Add(dataToAdd);
                                }
                            }
                        }

                        if (driverId != null)
                        {
                            Guid guid = new Guid(driverId);


                            if (roleOfAction == UsersManager.RoleStatus.Driver)
                            {
                                Guid? driverIdGuid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                                if (!driverIdGuid.HasValue || driverIdGuid.Value != guid)
                                {
                                    return RedirectToAction("Index");
                                }
                            }

                            Driver driver = db.Drivers.Find(guid);
                            if (
                                db.DriverToCompanies.Any(
                                    dtc => dtc.CompanyId == companyId.Value && dtc.DriverId == driver.Id))
                            {
                                SelectListItem dataToAdd = new SelectListItem()
                                {
                                    Value = driver.Id.ToString(),
                                    Text = string.Format("{0} {1} {2}", driver.Name, driver.FamilyName, driver.Licence)
                                };
                                model.PossibleDrivers.Add(dataToAdd);
                            }
                        }
                        else
                        {
                            if (roleOfAction == UsersManager.RoleStatus.Admin)
                            {
                                foreach (Driver driver in db.Drivers)
                                {
                                    if (
                                        db.DriverToCompanies.Any(
                                            dtc => dtc.CompanyId == companyId.Value && dtc.DriverId == driver.Id))
                                    {

                                        SelectListItem dataToAdd = new SelectListItem()
                                        {
                                            Value = driver.Id.ToString(),
                                            Text =
                                                string.Format("{0} {1} {2}", driver.Name, driver.FamilyName,
                                                    driver.Licence)
                                        };
                                        model.PossibleDrivers.Add(dataToAdd);
                                    }
                                }
                            }
                            else
                            {
                                Guid? guid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                                if (guid.HasValue)
                                {
                                    Driver driver = db.Drivers.Find(guid.Value);
                                    if (
                                        db.DriverToCompanies.Any(
                                            dtc => dtc.CompanyId == companyId.Value && dtc.DriverId == driver.Id))
                                    {
                                        SelectListItem dataToAdd = new SelectListItem()
                                        {
                                            Value = driver.Id.ToString(),
                                            Text =
                                                string.Format("{0} {1} {2}", driver.Name, driver.FamilyName,
                                                    driver.Licence)
                                        };
                                        model.PossibleDrivers.Add(dataToAdd);
                                    }
                                }
                                else
                                {
                                    return RedirectToAction("Index");
                                }
                            }
                        }

                    }
                }
                catch (Exception )
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Save(string carId, string driverId, bool offlineRideAllowed)
        {
            try
            {
                CarsToDriver carsToDriver = new CarsToDriver() { Id = Guid.NewGuid(), CarId = new Guid(carId), DriverId = new Guid(driverId), offlineRideAllowed =  offlineRideAllowed};

                using (CarManagerDbEntities db = new CarManagerDbEntities())
                {
                    var ctdList = db.CarsToDrivers.Where(ctd => carsToDriver.DriverId == ctd.DriverId && carsToDriver.CarId == ctd.CarId).ToList();

                    if (ctdList.Count > 0)
                    {
                        throw new Exception("Uknown Company");
                    }
                    else
                    {
                        db.CarsToDrivers.Add(carsToDriver);
                        db.SaveChanges();
                    }
                }

                return Json("success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}