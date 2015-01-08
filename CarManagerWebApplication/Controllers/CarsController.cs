using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dal;
using CarManagerWebApplication.Models;
using WebMatrix.WebData;

namespace CarManagerWebApplication.Controllers
{
    public class CarsController : Controller
    {
        private const string companyIdKey ="CompanyId";
        // GET: Cars
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

            List<Car> cars;

            try
            {
                using (CarManagerDbEntities db = new CarManagerDbEntities())
                {
                    if (roleOfAction == UsersManager.RoleStatus.Admin)
                    {
                        cars = RoleService.GetCarListByCompany(companyName);
                    }
                    else
                    {
                        cars = RoleService.GetCarListByCompanyAndDriver(companyName, WebSecurity.CurrentUserId);
                    }
                }
            }
            catch (Exception )
            {
                return RedirectToAction("Index", "Home");
            }

            cars.Sort((car1, car2) => car1.Number.CompareTo(car2.Number));
            return View(cars);
        }

        [HttpGet]
        public ActionResult Edit(string companyName, string id)
        {
            Guid? companyId = RoleService.GetCompanyId(companyName);
            if (companyId.HasValue)
            {
                UsersManager.RoleStatus roleOfAction = UsersManager.GetRole(companyId.Value, WebSecurity.CurrentUserId);
                if (roleOfAction == UsersManager.RoleStatus.Admin)
                {
                    WrapCarModel model = new WrapCarModel();
                    // if id is null then adding a new driver, else editing existing one
                    if (id == null)
                    {
                        model.Car = new Car();
                        model.DoesCarExist = false;
                        if (TempData.ContainsKey(companyIdKey))
                        {
                            TempData.Remove(companyIdKey);
                        }
                        TempData.Add(companyIdKey, companyId.Value);
                        return View(model);
                    }
                    else
                    {
                        try
                        {
                            Guid guid = new Guid(id);
                            using (CarManagerDbEntities db = new CarManagerDbEntities())
                            {
                                model.Car = db.Cars.Find(guid);
                            }

                            if (model.Car == null)
                            {
                                model.DoesCarExist = false;
                                TempData.Add(companyIdKey, companyId.Value);
                                model.Message = string.Format("Car with id {0} was not found in DB", id);

                            }
                            else if (model.Car.CompanyId != companyId.Value)
                            {
                                return RedirectToAction("Index", "Home");
                            }

                            return View(model);
                        }
                        catch
                            (Exception )
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
     
    }

        [HttpPost]
        public ActionResult Save([Bind(Include = "Id,Number,Model")] Car car)
        {
            try
            {
                using (CarManagerDbEntities db = new CarManagerDbEntities())
                {
                    Car tempCar;

                    if (car.Id != new Guid())
                    {
                        tempCar = db.Cars.Find(car.Id);
                    }
                    else if (TempData.ContainsKey(companyIdKey))
                    {
                        //Guid guid = Guid.NewGuid();
                        tempCar = new Car() {Id = Guid.NewGuid()};
                        car.Id = tempCar.Id;
                        tempCar.CompanyId = (Guid)TempData[companyIdKey];
                        car.CompanyId = tempCar.CompanyId;
                        db.Cars.Add(tempCar);
                        TempData.Remove(companyIdKey);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    if (tempCar != null)
                    {
                        tempCar.Number= car.Number;
                        tempCar.Model = car.Model;

                        db.SaveChanges();
                    }
                }

                return Json("Car saved successfully");
            }
            catch (Exception )
            {
                Response.StatusCode = 406; // Or any other proper status code.
                Response.Write("Exception occured.");
                return null;
            }
        }
    }
}