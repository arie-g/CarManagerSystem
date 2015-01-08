using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web.UI;
using CarManagerCommon;
using Dal;
using System;
using System.Linq;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CarManagerWebApplication
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CarManagerApi" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select CarManagerApi.svc or CarManagerApi.svc.cs at the Solution Explorer and start debugging.
    public class CarManagerApi : ICarManagerApi
    {
        public async Task<Guid?> SendRideStatistics(DriveStatistics stas)
        {
            return await RoleService.SendFullRideData(stas);
        }

        public Guid? CarIdByCarLicence(string carLicence)
        {
            return RoleService.GetCarId(carLicence);
        }

        public int? UserIdByProviderId(string providerUserId)
        {
            const string facebooKProvider = "Facebook";
            int? id = RoleService.GetUserId(providerUserId) ??  RoleService.AddNewUser(facebooKProvider,providerUserId);
            return id;
        }

        public Guid? DriverIdByUserId(int userId)
        {
            Guid? id = RoleService.GetDriverId(userId);
            return id;
        }

        public Guid? DriverIdByProviderId(string providerUserId)
        {
            Guid? id = RoleService.GetDriverId(providerUserId);
            return id;
        }

        public void UpdateOfflineRide(DriveStatistics statistics, bool emergencyRide)
        {
            if (emergencyRide)
            {
                
            }
            else
            {
                
            }
        }

        public async Task<bool> ApproveDrivers(List<Guid> driversIdList, Guid CompanyId)
        {
            return await RoleService.ApproveDrivers(driversIdList, CompanyId);
        }

        public List<Guid> DriversToApprove(Guid companyId)
        {
            return RoleService.GetDriversToApprove(companyId);
        }

        public async Task<Guid?> AddDriver(Driver newDriver, List<Guid> companiesId)
        {
            return await RoleService.AddNewDriver(newDriver, companiesId);
        }

        public eRecognizeStatus RecognizeUserStatus(string providerUserId)
        {
            using (var authDb = new AuthDBEntities())
            {
                bool recognizedUser = authDb.webpages_OAuthMembership.Any(u => u.ProviderUserId == providerUserId);
                if (!recognizedUser) return eRecognizeStatus.NotRecognized;
                webpages_OAuthMembership user = authDb.webpages_OAuthMembership.First(u => u.ProviderUserId == providerUserId);
                eRecognizeStatus res;
                using (CarManagerDbEntities db = new CarManagerDbEntities())
                {
                    res = db.Drivers.Any(u => u.UserId == user.UserId) ? eRecognizeStatus.Driver : eRecognizeStatus.User;
                }
                return res;
            }
        }

        public void SendDrivePackage(DrivePackage package)
        {
            RoleService.SendDrivePackage(package);
        }

        public int? CheckBreakingRoles(Guid driverId, string carPlateNumber)
        {
            Guid? carId = RoleService.GetCarId(carPlateNumber);
            Guid? carToDriverId = null;
            if (carId.HasValue)
            {
                carToDriverId = RoleService.GetCarToDriverId(carId.Value, driverId);
            }
            if (carId == null || driverId == null || carToDriverId == null)
            {
                return null;
            }
            if (RoleService.CanDrive(carToDriverId.Value))
            {
                return RoleService.AuthorizationCode(carToDriverId.Value);
            }
            else
            {
                return null;
            }
        }

        public async Task<Guid?> StartNewRide(Guid driverId, DateTime startTime, string carLicenceId)
        {
            if (!RoleService.CheckRideExist(driverId, startTime))
            {
                return await RoleService.AddRide(driverId, startTime, carLicenceId,false);
            }
            else
            {
                return null;
            }
        }

        public async Task<Guid?> EmergencyDriveByFacebookId(string providerUserId, string carPlateNumber)
        {
            return await RoleService.EmergencyDriveByFacebookId(providerUserId, carPlateNumber);
        }

        public async Task<Guid?> EmergencyDrive(Guid driverId, string carPlateNumber)
        {
            return await RoleService.EmergencyDrive(driverId, carPlateNumber);
        }

        public List<KeyValuePair<Guid, String>> GetCompanies()
        {
            return RoleService.GetCompanies();
        }

        public KeyValuePair<List<Guid>, List<Guid>>? GetOfflineAndEmergencyAllowedDrivers(string carNumber)
        {
            return RoleService.GetOfflineAndEmergencyAllowedDrivers(carNumber);
        }

        public void RestrictDrivingTime(Guid carToDriverGuid, DateTime startTime, DateTime endTime)
        {
            RoleService.AddDrivingTimeRestrict(carToDriverGuid, startTime, endTime);
        }

        public bool TimeToUpdateAuthorizedList(string carPlateNumber)
        {
            return RoleService.TimeToUpdateAuthorizedList(carPlateNumber);
        }
    }
}
