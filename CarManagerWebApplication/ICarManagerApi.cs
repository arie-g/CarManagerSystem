
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using CarManagerCommon;
using System;
using Dal;

namespace CarManagerWebApplication
{
    [ServiceContract]
    public interface ICarManagerApi
    {
        [OperationContract]
        bool TimeToUpdateAuthorizedList(string carPlateNumber);

        [OperationContract]
        void RestrictDrivingTime(Guid carToDriverGuid, DateTime startTime, DateTime endTime);

        [OperationContract]
        int? CheckBreakingRoles(Guid driverId, string carPlateNumber);

        [OperationContract]
        KeyValuePair<List<Guid>, List<Guid>>? GetOfflineAndEmergencyAllowedDrivers(string carNumber);

        [OperationContract]
        Guid? CarIdByCarLicence(string carLicence);

        [OperationContract]
        Task<Guid?> StartNewRide(Guid driverId, DateTime startTime, string carLicenceId);

        [OperationContract]
        eRecognizeStatus RecognizeUserStatus(string providerUserId);

        [OperationContract]
        void UpdateOfflineRide(DriveStatistics statistics, bool emergencyRide);

        [OperationContract]
        Guid? DriverIdByUserId(int userId);

        [OperationContract]
        List<Guid> DriversToApprove(Guid companyId);

        [OperationContract]
        Guid? DriverIdByProviderId(string providerUserId);

        [OperationContract]
        Task<bool> ApproveDrivers(List<Guid> driversIdList, Guid CompanyId);

        [OperationContract]
        Task<Guid?> AddDriver(Driver newDriver, List<Guid> companiesId);

        [OperationContract]
        int? UserIdByProviderId(string providerId);

        [OperationContract]
        Task<Guid?> EmergencyDriveByFacebookId(string providerUserId, string carPlateNumber);

        [OperationContract]
        Task<Guid?> EmergencyDrive(Guid driverId, string carPlateNumber);

        [OperationContract]
        Task<Guid?> SendRideStatistics(DriveStatistics stas);

        [OperationContract]
        void SendDrivePackage(DrivePackage package);

        [OperationContract]
        List<KeyValuePair<Guid, String>> GetCompanies();
    }
}