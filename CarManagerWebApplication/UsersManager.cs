using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dal;

namespace CarManagerWebApplication
{
    public static class UsersManager
    {
        private const int AdminRoleId = 1;
        private const int DriverRoleId = 2;

        private static string convertUrlToCompany(HttpContext url)
        {
            return url.Request.RawUrl.Split('/')[1];
        }

        public static bool IsAdmin(int UserId, HttpContext url)
        {
            Guid? companyGuid = RoleService.GetCompanyId(convertUrlToCompany(url));
            if (companyGuid.HasValue)
            {
                return GetRole(companyGuid.Value, UserId) == RoleStatus.Admin;
            }
            else
            {
                return false;
            }
        }

        public static List<string> GetCompaniesByDriver(Guid DriverId)
        {
            List<string> companiesList = new List<string>();
            using (var db = new CarManagerDbEntities())
            {
                var companiesOfDriver = from driver in db.DriverToCompanies
                    where driver.DriverId == DriverId && driver.Approved
                    select driver.CompanyId;
                foreach (Guid companyGuid in companiesOfDriver)
                {
                    companiesList.Add((db.Companies.Find(companyGuid)).Name);
                }
            }
            return companiesList;
        }

        public static List<Driver> DriversWaitingForApprove(Guid companyId)
        {
            using (var db = new CarManagerDbEntities())
            {
                List<Driver> driversToApprove = new List<Driver>();

                var driversWaitingToApprove = from driver in db.DriverToCompanies
                    where driver.CompanyId == companyId && driver.Approved == false
                    select driver.DriverId;
                foreach (Guid driverGuid in driversWaitingToApprove)
                {
                    driversToApprove.Add(db.Drivers.Find(driverGuid));
                }
                return driversToApprove;
            }
        }

        public static void ApproveDriver(Guid driverGuid, Guid companyId)
        {
            using (var db = new CarManagerDbEntities())
            {
                var newDriver = from driver in db.DriverToCompanies
                    where driver.DriverId == driverGuid && driver.CompanyId == companyId
                    select driver;
                if (newDriver.Any())
                {
                    var driver = newDriver.First();
                    driver.Approved = true;
                    db.SaveChanges();
                }
                int userId = (from driver in db.Drivers
                    where driver.Id == driverGuid
                    select driver.UserId).First();
                AddRoleToDriver(userId, companyId, DriverRoleId);
            }
        }

        public static void AddAdminToCompany(Guid DriverId, Guid CompanyId)
        {
            using (var db = new CarManagerDbEntities())
            {
                var userByDriver = from driver in db.Drivers
                    where driver.Id == DriverId
                    select driver.UserId;
                if (userByDriver.Any())
                {
                    AddAdminToCompany(userByDriver.First(), CompanyId);
                }
            }
        }

        public static void AddRoleToDriver(int UserId, Guid CompanyId, int RoleId)
        {
            using (var authDb = new AuthDBEntities())
            {
                authDb.webpages_UsersInRoles.Add(new webpages_UsersInRoles()
                {
                    CompanyId = CompanyId,
                    UserId = UserId,
                    RoleId = RoleId
                });
                authDb.SaveChanges();
            }
        }

        public static void AddAdminToCompany(int UserId, Guid CompanyId)
        {
            AddRoleToDriver(UserId, CompanyId, AdminRoleId);
        }

        public static bool IsUserAuthorized(int UserId, Guid companyId, int roleID)
        {
            using (var authDb = new AuthDBEntities())
            {
                return
                    authDb.webpages_UsersInRoles.Any(
                        userInRole =>
                            userInRole.UserId == UserId && userInRole.CompanyId == companyId &&
                            userInRole.RoleId == roleID);
            }
        }

        public static RoleStatus GetRole(Guid companyId, int UserId)
        {
            const int AdminRole = 1;
            const int DriverRole = 2;
            RoleStatus role = RoleStatus.Anonymous;
                using (var oAuthDb = new AuthDBEntities())
                {
                    if (
                        oAuthDb.webpages_UsersInRoles.Any(
                            roles =>
                                roles.UserId == UserId && roles.CompanyId == companyId &&
                                roles.RoleId == AdminRole))
                    {
                        role = RoleStatus.Admin;
                    }
                    else if (
                        oAuthDb.webpages_UsersInRoles.Any(
                            roles =>
                                roles.UserId == UserId && roles.CompanyId == companyId &&
                                roles.RoleId == DriverRole))
                    {
                        role = RoleStatus.Driver;
                    }
                }
            return role;
        }

        public enum RoleStatus
        {
            Driver,
            Admin,
            Anonymous
        }

        public static string GetAdminCompany(int currentUserId)
        {
            using (var oAuth = new AuthDBEntities())
            {
                const int AdminRole = 1;
                string companyName = string.Empty;
                if (oAuth.webpages_UsersInRoles.Any(role => role.UserId == currentUserId && role.RoleId == AdminRole))
                {
                    var companyGuid = (from role in oAuth.webpages_UsersInRoles
                        where role.UserId == currentUserId && role.RoleId == AdminRole
                        select role.CompanyId).First();

                    if (companyGuid != null)
                    {
                        using (var db = new CarManagerDbEntities())
                        {
                            companyName = db.Companies.Find(companyGuid).Name;
                        }
                    }
                }
                return companyName;
            }
        }

        public static bool UrlInCompany(HttpContext current)
        {
            Guid? companyGuid = RoleService.GetCompanyId(convertUrlToCompany(current));
            return companyGuid.HasValue;
        }

        public static bool isAnonymus(int userId)
        {
            using (var oAuth = new AuthDBEntities())
            {
                return oAuth.webpages_UsersInRoles.Any(user => user.UserId == userId && user.RoleId > 0);
            }
        }

    }
}