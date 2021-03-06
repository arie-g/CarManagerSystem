﻿using CarManagerWebApplication.Models;
using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace CarManagerWebApplication.Controllers
{
    public class RPMLimitController : Controller
    {
        // GET: RPMRestriction
        public ActionResult Index(string companyName)
        {
            List<WrapRPMLimit> model = new List<WrapRPMLimit>();

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

                    //throw new Exception("Can't Add Time Restriction. Authorization Problem");
                }

                try
                {
                    using (CarManagerDbEntities db = new CarManagerDbEntities())
                    {
                        var driversIds = new List<Guid>();
                        if (roleOfAction == UsersManager.RoleStatus.Driver)
                        {
                            Guid? driverIdGuid = RoleService.GetDriverId(WebSecurity.CurrentUserId);
                            if (driverIdGuid.HasValue)
                            {
                                driversIds.Add(driverIdGuid.Value);
                            }
                        }
                        else
                        {
                            driversIds =
                           (from driverToCompany in db.DriverToCompanies
                            where driverToCompany.CompanyId == companyGuid.Value
                            select driverToCompany.DriverId).ToList();

                        }

                        var ctdIds =
                            (from ctd in db.CarsToDrivers
                             where driversIds.Contains(ctd.DriverId)
                             select ctd.Id).ToList();

                        var rpmLimits =
                            (from res in db.Role_RPMLimit
                             where ctdIds.Contains(res.CarToDriverId)
                             select res).ToList();

                        foreach (Role_RPMLimit rest in rpmLimits)
                        {
                            model.Add(new WrapRPMLimit()
                            {
                                Car = rest.CarsToDriver.Car,
                                Driver = rest.CarsToDriver.Driver,
                                RPMLimit = rest,
                            });
                        }
                    }
                }
                catch (Exception )
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        public ActionResult Add(string companyName, string carId, string driverId)
        {
            AddRPMLimitModel model = new AddRPMLimitModel();

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
        public ActionResult Save(AddRPMLimitModel model)
        {
            int addedCount = 0;

            if (model.SelectedDrivers == null || model.SelectedDrivers.Count == 0 ||
                model.SelectedCars == null || model.SelectedCars.Count == 0)
            {
                Response.StatusCode = 406; // Or any other proper status code.
                Response.Write("Driver or Car are not selected");
                return null;
                //throw new Exception("Driver or Car are not selected");
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
                                    var limits = db.Role_RPMLimit.Where(limit => limit.CarToDriverId == ctd.Id).ToList();

                                    if (limits.Count == 0)
                                    {
                                        var limit = new Role_RPMLimit
                                        {
                                            CarToDriverId = ctd.Id,
                                            MaxtBrokePunishmentLimit = model.MaxtBrokePunishmentLimit,
                                            MaxtBrokeWarningLimit = model.MaxtBrokeWarningLimit,
                                            PunishmentLimit = model.PunishmentLimit,
                                            WarningLimit = model.WarningLimit,

                                        };
                                        db.Role_RPMLimit.Add(limit);
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

            return Json(string.Format("Added {0} new RPM limit.", addedCount));
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
                    List<Role_RPMLimit> limits = db.Role_RPMLimit.Where(limit => limit.CarToDriverId == guid).ToList();

                    if (limits.Count == 0)
                    {
                        return RedirectToAction("Index", "Home");

                    }
                    else if (limits.Count > 1)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        Role_RPMLimit limit = limits[0];
                        db.Role_RPMLimit.Remove(limit);
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
    }
}