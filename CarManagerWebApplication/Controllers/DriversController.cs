using CarManagerWebApplication.Models;
using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace CarManagerWebApplication.Controllers
{
    public class DriversController : Controller
    {
        private const string companyIdKey = "CompanyIdKey";
        // GET: Drivers

        //[UserAuthorizeCustom("driver")]
        //public ActionResult Index(string companyName)
        //{
        //    using (var db = new CarManagerDbEntities())
        //    {
        //        List<Driver> drivers = new List<Driver>();
        //        Guid? companyId = RoleService.GetCompanyId(companyName);
        //        if (!companyId.HasValue)
        //        {
        //            throw new Exception("Failed to get Drivers from Database\n Bad Company name");
        //        }
        //        try
        //        {
        //            var driverFound = RoleService.GetDriverId((int) WebSecurity.CurrentUserId);
        //            if (driverFound.HasValue)
        //            {
        //                drivers.Add(db.Drivers.Find(driverFound.Value));
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //            throw new Exception("Failed to get Drivers from Database", ex);
        //        }

        //        return View(drivers);
        //    }
        //}
        [Authorize]
        public ActionResult Index(string companyName)
        {
            Guid? companyId = RoleService.GetCompanyId(companyName);
            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyId.Value, WebSecurity.CurrentUserId);
            if (roleOfAction == UsersManager.RoleStatus.Anonymous)
            {
                return RedirectToAction("Index", "Home");
            }

            List<Driver> drivers= new List<Driver>();
            
            try
            {
                using (CarManagerDbEntities db = new CarManagerDbEntities())
                {
                    if (roleOfAction == UsersManager.RoleStatus.Admin)
                    {
                        var driversInCompany =
                            db.DriverToCompanies.Where(company => company.CompanyId == companyId.Value)
                                .Select(driver => driver.DriverId)
                                .ToList<Guid>();
                        drivers = db.Drivers.Where(driver => driversInCompany.Contains(driver.Id)).ToList<Driver>();
                    }
                    else
                    {
                        var driverFound = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                        if (driverFound.HasValue)
                        {
                            drivers.Add(db.Drivers.Find(driverFound.Value));
                        }
                    }
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Home");
            }

            drivers.Sort((driver1, driver2) => driver1.Name.CompareTo(driver2.Name));
            return View(drivers);
        }

        [HttpGet]
        public ActionResult Edit(string companyName, string id)
        {
            Guid? companyId = RoleService.GetCompanyId(companyName);
            if (!companyId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }
            UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyId.Value, WebSecurity.CurrentUserId);
            if (roleOfAction == UsersManager.RoleStatus.Anonymous)
            {
                return RedirectToAction("Index", "Home");
            }
            WrapDriverModel model = new WrapDriverModel();
            
            // if id is null then adding a new driver, else editing existing one
            if (id == null)
            {
                if (roleOfAction == UsersManager.RoleStatus.Admin)
                {
                    model.Driver = new Driver();
                    model.DoesDriverExist = false;
                    addToTempDate(companyIdKey, companyId.Value);
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {

                try
                {
                    Guid guid = new Guid(id);
                    if (roleOfAction == UsersManager.RoleStatus.Driver && (!RoleService.GetDriverId(WebSecurity.CurrentUserId).HasValue || RoleService.GetDriverId(WebSecurity.CurrentUserId).Value != guid))
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    using (CarManagerDbEntities db = new CarManagerDbEntities())
                    {
                        if (RoleService.DriverInCompany(guid, companyId.Value))
                        {
                            model.Driver = db.Drivers.Find(guid);
                        }
                        else
                        {
                            model.Driver = null;
                        }
                    }

                    if (model.Driver == null)
                    {
                        model.DoesDriverExist = false;
                        addToTempDate(companyIdKey,companyId.Value);
                        model.Message = string.Format("Driver with id {0} was not found in DB", id);
                    }

                    return View(model);
                }
                catch (Exception )
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        private void addToTempDate(string key, Guid value)
        {
            if (TempData.ContainsKey(key))
            {
                TempData.Remove(key);
            }
            TempData.Add(key, value);
        }

        [HttpPost]
        public ActionResult Save([Bind(Include = "Id,Name,FamilyName,Licence,ExperienceYears")] Driver driver)
        {
            WrapDriverModel model = new WrapDriverModel();

            try
            {
                using (CarManagerDbEntities db = new CarManagerDbEntities())
                {
                    Driver tempDriver;

                    if (driver.Id != new Guid())
                    {
                        tempDriver = db.Drivers.Find(driver.Id);
                    }
                    //else if (TempData.ContainsKey(companyIdKey))
                    //{

                    //    //Guid guid = Guid.NewGuid();
                    //    tempDriver = new Driver() {Id = Guid.NewGuid()};
                    //    driver.Id = tempDriver.Id;
                    //    db.Drivers.Add(tempDriver);
                    //    db.DriverToCompanies.Add(new DriverToCompany()
                    //    {
                    //        Approved = false,
                    //        CompanyId = (Guid) TempData[companyIdKey],
                    //        DriverId = driver.Id
                    //    });
                    //    TempData.Remove(companyIdKey);
                    //}
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    if (tempDriver == null)
                    {
                        model.Message = string.Format("Driver with id {0} was not found in DB", driver.Id);
                    }
                    else
                    {
                        tempDriver.Name = driver.Name;
                        tempDriver.FamilyName = driver.FamilyName;
                        tempDriver.Licence = driver.Licence;
                        tempDriver.ExperienceYears = driver.ExperienceYears;

                        db.SaveChanges();
                    }

                    model.Driver = driver;
                }

                return Json(model);
            }
            catch (Exception )
            {
                return RedirectToAction("Index", "Home");
            }

        }
    }
}