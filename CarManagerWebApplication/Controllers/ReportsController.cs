//using CarManagerWebApplication.Models;
//using Dal;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace CarManagerWebApplication.Controllers
//{
//    public class ReportsController : Controller
//    {
//        // GET: Reports

//        public ActionResult Index(string companyName)
//        {
//            WrapReportModel model = new WrapReportModel();

//            Guid? companyGuid = RoleService.GetCompanyId(companyName);
//            companyGuid = RoleService.GetCompanyId("Urban Kibutz");
//            if (!companyGuid.HasValue)
//            {
//                throw new Exception("Can't Add Engine Temp. Bad Company ID");
//            }
//            else
//            {
//                //List<Dal.Driver> drivers = new List<Dal.Driver>();
//                //List<Dal.Car> Cars = new List<Dal.Car>();
//                //DateTime startDate = new DateTime();
//                //DateTime endDate = new DateTime();
//                //bool allRides = true;
//                //Double Price = 0;

//                //List<ReportModel> RM = RoleService.CreateReport(drivers, Cars, startDate, endDate, allRides, Price);

//                try
//                {
//                    using (CarManagerDbEntities db = new CarManagerDbEntities())
//                    {
//                        var drivers =
//                            (from driverToCompany in db.DriverToCompanies
//                             where driverToCompany.CompanyId == companyGuid.Value
//                             select driverToCompany.Driver).ToList();

//                        var cars = new List<Car>();

//                        foreach (Driver driver in drivers)
//                        {
//                            var tempCars =
//                            (from carsToDriver in db.CarsToDrivers
//                             where carsToDriver.DriverId == driver.Id
//                             select carsToDriver.Car).ToList();

//                            cars.AddRange(tempCars);
//                        }

//                        cars = cars.Distinct().ToList();

//                        model.Cars = cars;
//                        model.Drivers = drivers;
//                    }
//                }
//                ////UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyGuid.Value, WebSecurity.CurrentUserId);
//                ////if (roleOfAction != UsersManager.RoleStatus.Admin)
//                ////{
//                ////    return RedirectToAction("Index", "EngineTempLimit");

//////    //throw new Exception("Can't Add Time Restriction. Authorization Problem");
//                ////}

////try
//                //{
//                //    using (CarManagerDbEntities db = new CarManagerDbEntities())
//                //    {
//                //        var drivers =
//                //            (from driverToCompany in db.DriverToCompanies
//                //             where driverToCompany.CompanyId == companyGuid.Value
//                //             select driverToCompany.Driver).ToList();

////        var cars = new List<Car>();

////        foreach (Driver driver in drivers)
//                //        {
//                //            var tempCars =
//                //            (from carsToDriver in db.CarsToDrivers
//                //             where carsToDriver.DriverId == driver.Id
//                //             select carsToDriver.Car).ToList();

////            cars.AddRange(tempCars);
//                //        }

////        cars = cars.Distinct().ToList();

////        model.Cars = cars;
//                //        model.Drivers = drivers;
//                //    }
//                //}

//                catch (Exception ex)
//                {
//                    throw new Exception("Exception occured.", ex);
//                }
//            }
//            return View(model);
//        }
//    }
//}
