using CarManagerWebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace CarManagerWebApplication.Controllers
{
    public class WaitingForApproveController : Controller
    {
        public ActionResult Index(string companyName)
        {
            WrapWaitingForApproveModel model = new WrapWaitingForApproveModel();

            if (string.IsNullOrEmpty(companyName))
            {
                if (WebSecurity.CurrentUserId > 0)
                {
                    companyName = UsersManager.GetAdminCompany(WebSecurity.CurrentUserId);
                    if (string.IsNullOrEmpty(companyName))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");

                }

            }
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
                model.Drivers = UsersManager.DriversWaitingForApprove(companyGuid.Value);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult ApproveDriver(string companyName, WrapWaitingForApproveModel model)
        {
            int addedCount = 0;
            Guid? companyGuid = RoleService.GetCompanyId(companyName);
            if (!companyGuid.HasValue ||
                UsersManager.GetRole(companyGuid.Value, WebSecurity.CurrentUserId) != UsersManager.RoleStatus.Admin)
            {
                Response.StatusCode = 300; // Or any other proper status code.
                Response.Write("Not Authorized.");
                return null;
            }
            if (model.SelectedDrivers == null || model.SelectedDrivers.Count == 0)
            {
                Response.StatusCode = 406; // Or any other proper status code.
                Response.Write("No Drivers were selected");
                return null;
            }
            else
            {
                try
                {
                    foreach (Guid selectedDriver in model.SelectedDrivers)
                    {
                        UsersManager.ApproveDriver(selectedDriver, companyGuid.Value);
                        addedCount++;
                    }
                }
                catch (Exception )
                {
                    Response.StatusCode = 406; // Or any other proper status code.
                    Response.Write("Exception occured.");
                    return null;
                }
            }

            return Json(string.Format("Approved {0} users as drivers.", addedCount));
        }

        [HttpPost]
        public ActionResult ApproveAdmin(string companyName, WrapWaitingForApproveModel model)
        {
            int addedCount = 0;
            Guid? companyGuid = RoleService.GetCompanyId(companyName);

            if (!companyGuid.HasValue ||
                UsersManager.GetRole(companyGuid.Value, WebSecurity.CurrentUserId) != UsersManager.RoleStatus.Admin)
            {
                Response.StatusCode = 300; // Or any other proper status code.
                Response.Write("Not Authorized.");
                return null;
            }

            if (model.SelectedDrivers == null || model.SelectedDrivers.Count == 0)
            {
                Response.StatusCode = 406; // Or any other proper status code.
                Response.Write("No Drivers were selected");
                return null;
            }
            else
            {
                try
                {
                    foreach (Guid selectedDriver in model.SelectedDrivers)
                    {
                        UsersManager.AddAdminToCompany(selectedDriver, companyGuid.Value);
                        addedCount++;
                    }
                }
                catch (Exception)
                {
                    Response.StatusCode = 406; // Or any other proper status code.
                    Response.Write("Exception occured.");
                    return null;
                }
            }

            return Json(string.Format("Approved {0} users as admins.", addedCount));
        }
    }


}