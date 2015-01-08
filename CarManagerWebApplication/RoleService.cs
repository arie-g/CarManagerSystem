using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using CarManagerCommon;
using CarManagerWebApplication.Models;
using Dal;
using System;
using System.Linq;

namespace CarManagerWebApplication
{
    public static class RoleService
    {
        public static bool CheckPackageBreakRoles(DrivePackage package, Guid rideId)
        {
            using (var db = new CarManagerDbEntities())
            {
                bool rideFound = db.Rides.Any(u => u.Id == rideId);
                if (!rideFound)
                {
                    return false;
                }
                else
                {
                    bool goodDay = false;
                    bool goodTime = false;
                    Ride curRide = db.Rides.First(u => u.Id == rideId);
                    Guid? carToDriverGuid = GetCarToDriverId(curRide.CarID, curRide.DriverID);
                    if (carToDriverGuid.HasValue)
                    {
                        goodDay = CheckDayRestriction(carToDriverGuid.Value);
                        if (goodDay)
                        {
                            goodTime = CheckTimeRestriction(carToDriverGuid.Value);
                        }
                        return goodDay && goodTime;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public static int? AddNewUser(string mobileProvider, string providerId)
        {
            using (var authDb = new AuthDBEntities())
            {
                int userId = authDb.Users.Max(u => u.Id);
                userId++;
                authDb.Users.Add(new User() {Id = userId, UserName = string.Empty});
                authDb.webpages_OAuthMembership.Add(new webpages_OAuthMembership()
                {
                    UserId = userId,
                    Provider = mobileProvider,
                    ProviderUserId = providerId
                });
                try
                {
                    authDb.SaveChanges();
                    return userId;
                }
                catch (DbEntityValidationException)
                {
                    return null;
                }
            }
        }

        public static UserSum GetAdminSumByUserId(int UserId, string companyName)
        {
            UserSum sum = new UserSum();
            Guid? adminId = GetDriverId(UserId);
            if (adminId.HasValue)
            {
                using (var db = new CarManagerDbEntities())
                {
                    Guid? companyGuid = GetCompanyId(companyName);
                    Driver driver = db.Drivers.Find(adminId.Value);
                    sum.Notifications = new List<string>();
                    sum.FullName = string.Format("{0} {1}", driver.Name, driver.FamilyName);
                    sum.LastRides = GetLastRidesOfCompany(companyGuid);
                    sum.CarsSum = GetCarsSumOfCompany(companyGuid);
                }
            }
            return sum;
        }

        private static List<CarSum> GetCarsSumOfCompany(Guid? companyGuid)
        {
            using (var db = new CarManagerDbEntities())
            {
                List<CarSum> sum = new List<CarSum>();
                if (companyGuid.HasValue)
                {
                    var carsList = from car in db.Cars
                        where car.CompanyId == companyGuid.Value
                        select car;

                    foreach (var car in carsList)
                    {
                        sum.Add(new CarSum()
                        {
                            Model = car.Model,
                            Number = car.Number,
                            NumberRides = NumberRidesByCar(car.Id),
                            Distance = DistanceByCar(car.Id)
                        });
                    }
                }
                return sum;
            }
        }

        private static double DistanceByCar(Guid carId)
        {
            using (var db = new CarManagerDbEntities())
            {
                double distance = 0;

                var rides = from ride in db.Rides
                            where ride.CarID == carId 
                            select ride;
                foreach (Ride ride in rides)
                {
                    if (ride.EndDrive.HasValue)
                    {
                        var driving = ride.EndDrive.Value.Subtract(ride.StartDrive);
                        distance += avgSpeed(ride.Id) * (driving.TotalHours);
                    }
                }
                return distance;
            }
        }

        private static int NumberRidesByCar(Guid carId)
        {
            using (var db = new CarManagerDbEntities())
            {
                return (from rides in db.Rides
                        where rides.CarID == carId 
                        select rides.Id).Count();
            }
        }

        private static List<EndedRide> GetLastRidesOfCompany(Guid? companyGuid)
        {
            using (var db = new CarManagerDbEntities())
            {
                List<EndedRide> rideList = new List<EndedRide>();
                if (companyGuid.HasValue)
                {
                   var carsInCompanyId = from cars in db.Cars
                                         where cars.CompanyId  == companyGuid.Value
                                         select cars.Id;

                    var lastRides = (from rides in db.Rides
                        where carsInCompanyId.Contains(rides.CarID) && rides.EndDrive.HasValue
                        select rides).OrderByDescending(ride => ride.StartDrive).ToList();
                    
                    var ridesId = (from ride in lastRides
                                  select ride.Id).Take(10);

                    rideList = (from endedRides in db.EndedRides
                                where ridesId.Contains(endedRides.Id)
                                select endedRides).ToList();
                }
                return rideList;
            }
        }

        public static UserSum GetDriverSumByUserId(int UserId)
        {
            UserSum sum = new UserSum();
            Guid? driverId = GetDriverId(UserId);
            if (driverId.HasValue)
            {
                using (var db = new CarManagerDbEntities())
                {
                    Driver driver = db.Drivers.Find(driverId.Value);
                    sum.Notifications = new List<string>();
                    sum.FullName = string.Format("{0} {1}", driver.Name, driver.FamilyName);
                    sum.LastRides = GetLastRidesOfDriver(driverId.Value);
                    sum.CarsSum = GetCarsSumOfDriver(driverId.Value);
                }
            }
            return sum;
        }

        private static List<CarSum> GetCarsSumOfDriver(Guid driverGuid)
        {
            using (var db = new CarManagerDbEntities())
            {
                List<CarSum> sum = new List<CarSum>();
                var carsIdList = from driverToCar in db.CarsToDrivers
                    where driverToCar.DriverId == driverGuid
                    select driverToCar.CarId;

                foreach (var carGuid in carsIdList)
                {
                    Car car = db.Cars.Find(carGuid);
                    sum.Add(new CarSum()
                    {
                        Model = car.Model,
                        Number = car.Number,
                        NumberRides = NumberRidesByDriverAndCar(car.Id, driverGuid),
                        Distance = DistanceByCarAndDriver(car.Id,driverGuid)
                    });
                }
                return sum;
            }
        }

        private static double DistanceByCarAndDriver(Guid carGuid, Guid driverGuid)
        {
            using (var db = new CarManagerDbEntities())
            {

                var rides = from ride in db.Rides
                    where ride.CarID == carGuid && ride.DriverID == driverGuid
                    select ride;
                
                return distanceByRides(rides);
            }
        }

        private static double distanceByRides(IQueryable<Ride> rides)
        {
            double distance = 0;

            foreach (Ride ride in rides)
            {
                if (ride.EndDrive.HasValue)
                {
                    var driving = ride.EndDrive.Value.Subtract(ride.StartDrive);
                    distance += avgSpeed(ride.Id) * (driving.TotalHours);
                }
            }

            return double.Parse(string.Format("{0:0.##}",distance));
        }

        private static int NumberRidesByDriverAndCar(Guid CarGuid, Guid driverGuid)
        {
            using (var db = new CarManagerDbEntities())
            {
             return (from rides in db.Rides
                    where rides.CarID == CarGuid && rides.DriverID == driverGuid
                         select rides.Id).Count();
            }
        }

        private static List<EndedRide> GetLastRidesOfDriver(Guid driverGuid)
        {
            using (var db = new CarManagerDbEntities())
            {
                List<EndedRide> rideList = new List<EndedRide>();

                var ridesList = (from rides in db.Rides
                    where rides.DriverID == driverGuid
                    orderby rides.EndDrive
                    select rides.Id).Take(10);

                rideList = (from endedRides in db.EndedRides
                            where ridesList.Contains(endedRides.Id)
                    select endedRides).ToList();
                return rideList;
            }
        }

        private static bool CheckTimeRestriction(Guid carToDriverId)
        {
            using (var db = new CarManagerDbEntities())
            {
                bool canDrive = false;
                if (!db.CarsToDrivers.Any(u => u.Id == carToDriverId))
                {
                    return true;
                }
                CarsToDriver carToDriver = db.CarsToDrivers.First(u => u.Id == carToDriverId);

                var result = from u in db.Role_TimeRestriction
                    where u.CarToDriverId == carToDriver.Id
                    select u;

                bool haveLimitation = result.Count() > 0;
                if (haveLimitation)
                {
                    canDrive = false;
                    var timeNow = DateTime.Now.TimeOfDay;
                    foreach (var rest in result)
                    {
                        if ((rest.StartTime.TimeOfDay <= timeNow) && (rest.EndTime.TimeOfDay >= timeNow))
                        {
                            canDrive = true;
                            break;
                        }
                    }
                }
                else
                {
                    canDrive = true;
                }
                return canDrive;
            }
        }

        private static bool CheckDayRestriction(Guid carToDriverId)
        {
            bool retVal = false;
            using (var db = new CarManagerDbEntities())
            {
                if (!db.CarsToDrivers.Any(u => u.Id == carToDriverId))
                {
                    return false;
                }
                CarsToDriver carToDriver = db.CarsToDrivers.First(u => u.Id == carToDriverId);

                bool restrictionFound = db.Role_DayRestriction.Any(u => u.CarToDriverId == carToDriver.Id);
                if (restrictionFound)
                {
                    var dayRestriction = db.Role_DayRestriction.First(u => u.CarToDriverId == carToDriver.Id);
                    switch (DateTime.Now.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            retVal = dayRestriction.Sunday == true;
                            break;

                        case DayOfWeek.Monday:
                            retVal = dayRestriction.Monday == true;
                            break;

                        case DayOfWeek.Tuesday:
                            retVal = dayRestriction.Tuesday == true;
                            break;

                        case DayOfWeek.Wednesday:
                            retVal = dayRestriction.Wednesday == true;
                            break;

                        case DayOfWeek.Thursday:
                            retVal = dayRestriction.Thursday == true;
                            break;

                        case DayOfWeek.Friday:
                            retVal = dayRestriction.Friday == true;
                            break;

                        case DayOfWeek.Saturday:
                            retVal = dayRestriction.Saturday == true;
                            break;
                        default:
                            retVal = false;
                            break;
                    }
                }
                else
                {
                    retVal = true;
                }
            }
            return retVal;
        }

        public static int AuthorizationCode(Guid carToDriverId)
        {
            return 1;
        }

        public static bool CanDrive(Guid carToDriverId)
        {
            return CheckPunishedDriver(carToDriverId) && CheckTimeRestriction(carToDriverId) &&
                   CheckDayRestriction(carToDriverId);
        }

        private static bool CheckPunishedDriver(Guid carToDriverId)
        {
            using (var db = new CarManagerDbEntities())
            {
                if (db.CarsToDrivers.Any(ctd => ctd.Id == carToDriverId))
                {
                    Guid? driverId = (from ctd in db.CarsToDrivers
                        where ctd.Id == carToDriverId
                        select ctd.DriverId).First();
                    if (driverId.HasValue)
                    {
                        DateTime now = DateTime.Now;
                        if (db.Role_Punished.Any(punish => punish.DriverId == driverId.Value))
                        {
                            var res = (from punish in db.Role_Punished
                                where punish.DriverId == driverId.Value
                                select punish.expirationDate).First();
                            return res != default(DateTime) && res > now;
                        }
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }
        }

        public static int? GetUserId(string providerId)
        {
            using (var oAuthDb = new AuthDBEntities())
            {
                //bool userFound = oAuthDb.webpages_OAuthMembership.Any(u => u.ProviderUserId == providerUserId);
                if (oAuthDb.webpages_OAuthMembership.Any(oAuth => oAuth.ProviderUserId == providerId))
                {
                    webpages_OAuthMembership oAuthUser =
                        oAuthDb.webpages_OAuthMembership.First(u => u.ProviderUserId == providerId);
                    return oAuthUser.UserId;
                }
                else
                {
                    return null;
                }
            }
        }

        public static Guid? GetDriverId(int userId)
        {
            using (var db = new CarManagerDbEntities())
            {
                bool driverFound = db.Drivers.Any(u => u.UserId == userId);

                if (driverFound)
                {
                    var driver = db.Drivers.First(u => u.UserId == userId);
                    return driver.Id;
                }
                else
                {
                    return null;
                }

            }
        }

        public static Guid? GetDriverId(string providerUserId)
        {
            int? userId = GetUserId(providerUserId);
            return userId == null ? null : GetDriverId(userId.Value);
        }

        public static Guid? GetCarId(string carPlateNumber)
        {
            using (var db = new CarManagerDbEntities())
            {
                long carPlateLong = long.Parse(carPlateNumber);
                bool carFound = db.Cars.Any(u => u.Number == carPlateLong);
                if (carFound)
                {
                    Car car = db.Cars.First(u => u.Number == carPlateLong);
                    return car.Id;
                }
                else
                {
                    return null;
                }
            }
        }

        public static async Task<bool> ApproveDrivers(List<Guid> driversIdList, Guid CompanyId)
        {
            using (var db = new CarManagerDbEntities())
            {
                foreach (Guid driverToApproveId in driversIdList)
                {
                    var driver = (from u in db.DriverToCompanies
                        where u.CompanyId == CompanyId && u.DriverId == driverToApproveId
                        select u).First();
                    if (driver != null)
                    {
                        driver.Approved = true;
                    }
                }
                try
                {
                    await db.SaveChangesAsync();
                    return true;
                }
                catch (DbEntityValidationException)
                {
                    return false;
                }
            }
        }

        public static List<Guid> GetDriversToApprove(Guid companyId)
        {
            using (var db = new CarManagerDbEntities())
            {
                List<Guid> driversWaiting = new List<Guid>();
                foreach (DriverToCompany driver in db.DriverToCompanies)
                {
                    if (!driver.Approved && driver.CompanyId == companyId)
                    {
                        driversWaiting.Add(driver.DriverId);
                    }
                }
                return driversWaiting;
            }
        }

        public static List<Company> GetCompaniesByUserId(int userId)
        {
            using (var db = new CarManagerDbEntities())
            {
                List<Company> companiesAproovedList = new List<Company>();

                if (db.Drivers.Any(driver => driver.UserId == userId))
                {
                    Guid driverGuid = (from driver in db.Drivers
                        where driver.UserId == userId
                        select driver.Id).First();
                    var companiesAproovedForDriver = from companies in db.DriverToCompanies
                        where companies.DriverId == driverGuid && companies.Approved
                        select companies;

                    foreach (DriverToCompany driverToCompany in companiesAproovedForDriver)
                    {
                        Company compToAdd = db.Companies.Find(driverToCompany.CompanyId);
                        companiesAproovedList.Add(compToAdd);
                    }
                }
                return companiesAproovedList;
            }
        }

        public static async Task<Guid?> AddNewDriver(Driver newDriver, List<Guid> companiesId)
        {
            using (var db = new CarManagerDbEntities())
            {
                Guid id = Guid.NewGuid();
                bool driverFound = db.Drivers.Any(u => u.UserId == newDriver.UserId);
                if (driverFound)
                {
                    return null;
                }
                else
                {
                    newDriver.Id = id;
                    db.Drivers.Add(newDriver);
                    foreach (Guid company in companiesId)
                    {
                        if (db.Companies.Any(comp => comp.Id == company))
                        {
                            db.DriverToCompanies.Add(new DriverToCompany()
                            {
                                CompanyId = company,
                                DriverId = newDriver.Id,
                                Approved = false
                            });
                        }


                    }
                    try
                    {
                        await db.SaveChangesAsync();

                    }
                    catch (DbEntityValidationException)
                    {
                        return null;
                    }

                }
                return id;
            }
        }

        public static bool CheckRideExist(Guid driverId, DateTime startTime)
        {
            using (var db = new CarManagerDbEntities())
            {
                return db.Rides.Any(u => u.DriverID == driverId && u.StartDrive == startTime);
            }
        }

        public static async Task<Guid?> AddRide(Guid driverId, DateTime startTime, string carLicenceId,
            bool emergancyRide)
        {
            using (var db = new CarManagerDbEntities())
            {
                Guid? carId = GetCarId(carLicenceId);
                if (!carId.HasValue)
                {
                    return null;
                }

                Guid? carToDriverId = GetCarToDriverId(carId.Value, driverId);
                if (!carToDriverId.HasValue)
                {
                    return null;
                }
                if (emergancyRide || CanDrive(carToDriverId.Value))
                {
                    Guid rideId = Guid.NewGuid();
                    Ride newRide = new Ride()
                    {
                        DriverID = driverId,
                        CarID = carId.Value,
                        StartDrive = startTime,
                        EndDrive = startTime,
                        Id = rideId,
                        Emergency = emergancyRide
                    };

                    db.Rides.Add(newRide);
                    try
                    {
                        await db.SaveChangesAsync();
                        return rideId;

                    }
                    catch (DbEntityValidationException)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public static async Task<Guid?> SendFullRideData(DriveStatistics stas)
        {
            using (var db = new CarManagerDbEntities())
            {
                Ride ride;
                try
                {
                    ride = (from u in db.Rides
                        where u.StartDrive == stas.StartDrive && u.DriverID == stas.DriverId
                        select u).First();
                }
                catch (Exception)
                {
                    Guid rideId = Guid.NewGuid();
                    ride = new Ride()
                    {
                        CarID = stas.CarId,
                        DriverID = stas.DriverId,
                        StartDrive = stas.StartDrive,
                        EndDrive = stas.FinishDrive,
                        Id = rideId
                    };
                    db.Rides.Add(ride);
                }

                ride.EndDrive = stas.FinishDrive;
                foreach (DrivePackage newPackage in from package in stas.Data
                    let drivePackageFound = db.DrivePackages.Any(u => u.RideId == ride.Id && u.Time == package.Time)
                    where !drivePackageFound
                    select
                        new Dal.DrivePackage()
                        {
                            Time = package.Time,
                            RideId = ride.Id,
                            EngineTemp = package.EngineTemp,
                            RPM = package.RPM,
                            Speed = package.Speed
                        })
                {
                    checkPackageRestrictions(newPackage);
                    db.DrivePackages.Add(newPackage);
                }

                try
                {
                    await db.SaveChangesAsync();
                    return ride.Id;

                }
                catch (DbEntityValidationException)
                {
                    return null;
                }
            }
        }

        public static void checkPackageRestrictions(DrivePackage package)
        {
            using (var db = new CarManagerDbEntities())
            {
                if (db.Rides.Any(ride => ride.Id == package.RideId))
                {
                    Ride ride = db.Rides.Find(package.RideId);
                    Guid? carToDriverGuid = GetCarToDriverId(ride.CarID, ride.DriverID);
                    if (carToDriverGuid.HasValue)
                    {
                        Role_RPMLimit rpmLimit = GetRPMLimitRestriction(carToDriverGuid.Value);
                        if (rpmLimit != null)
                        {
                            if (package.RPM > rpmLimit.WarningLimit)
                            {
                                rpmLimit.CountBrokeWarningLimit ++;
                                if (rpmLimit.MaxtBrokeWarningLimit > rpmLimit.CountBrokeWarningLimit)
                                {
                                    Punish(carToDriverGuid.Value, punishReason.RPMWarning);
                                }
                            }
                            if (package.RPM > rpmLimit.PunishmentLimit)
                            {
                                rpmLimit.CountBrokePunishmentLimit++;
                                if (rpmLimit.MaxtBrokePunishmentLimit > rpmLimit.CountBrokePunishmentLimit)
                                {
                                    Punish(carToDriverGuid.Value, punishReason.RPMPunish);
                                }
                            }
                            Role_EngineTempLimit engineTempLimit = GetEngineTempLimitRestriction(carToDriverGuid.Value);
                            if (engineTempLimit != null)
                            {
                                if (package.EngineTemp > engineTempLimit.WarningLimit)
                                {
                                    engineTempLimit.CountBrokeWarningLimit++;
                                    if (engineTempLimit.CountBrokeWarningLimit > engineTempLimit.MaxtBrokeWarningLimit)
                                    {
                                        Punish(carToDriverGuid.Value, punishReason.EngineTempWarning);
                                    }
                                }
                                if (package.EngineTemp > engineTempLimit.PunishmentLimit)
                                {
                                    engineTempLimit.CountBrokePunishmentLimit++;
                                    if (engineTempLimit.CountBrokePunishmentLimit >
                                        engineTempLimit.MaxtBrokePunishmentLimit)
                                    {
                                        Punish(carToDriverGuid.Value, punishReason.EngineTempPunish);
                                    }
                                }
                            }
                            Role_SpeedLimit speedLimit = GetSpeedLimitRestriction(carToDriverGuid.Value);
                            if (speedLimit != null)
                            {
                                if (package.Speed > speedLimit.WarningLimit)
                                {
                                    speedLimit.CountBrokeWarningLimit++;
                                    if (speedLimit.CountBrokeWarningLimit > speedLimit.MaxtBrokeWarningLimit)
                                    {
                                        Punish(carToDriverGuid.Value, punishReason.SpeedWarning);
                                    }
                                }
                                if (package.Speed > speedLimit.PunishmentLimit)
                                {
                                    speedLimit.CountBrokePunishmentLimit++;
                                    if (speedLimit.CountBrokePunishmentLimit > speedLimit.MaxtBrokePunishmentLimit)
                                    {
                                        Punish(carToDriverGuid.Value, punishReason.SpeedPunish);
                                    }
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
        }

        private static Role_SpeedLimit GetSpeedLimitRestriction(Guid carToDriverGuid)
        {
            using (var db = new CarManagerDbEntities())
            {
                if (db.Role_SpeedLimit.Any(rest => rest.CarToDriverId == carToDriverGuid))
                {
                    return (from limit in db.Role_SpeedLimit
                        where limit.CarToDriverId == carToDriverGuid
                        select limit).First();
                }
                else
                {
                    return null;
                }
            }
        }

        private static Role_EngineTempLimit GetEngineTempLimitRestriction(Guid carToDriverGuid)
        {
            using (var db = new CarManagerDbEntities())
            {
                if (db.Role_EngineTempLimit.Any(rest => rest.CarToDriverId == carToDriverGuid))
                {
                    return (from limit in db.Role_EngineTempLimit
                        where limit.CarToDriverId == carToDriverGuid
                        select limit).First();
                }
                else
                {
                    return null;
                }
            }
        }

        private enum punishReason
        {
            RPMWarning,
            RPMPunish,
            SpeedWarning,
            SpeedPunish,
            EngineTempWarning,
            EngineTempPunish
        }

        private static void Punish(Guid carToDriverGuid, punishReason reason)
        {
            using (var db = new CarManagerDbEntities())
            {
                const double RPMWarning = 1;
                const double RPMPunish = 3;
                const double SpeedWarning = 3;
                const double SpeedPunish = 7;
                const double EngineTempWarning = 1;
                const double EngineTempPunish = 3;

                Guid DriverId = (from ctd in db.CarsToDrivers
                    where ctd.Id == carToDriverGuid
                    select ctd.DriverId).First();


                if (!db.Role_Punished.Any(punish => punish.DriverId == DriverId))
                {
                    db.Role_Punished.Add(new Role_Punished()
                    {
                        DriverId = DriverId,
                        expirationDate = DateTime.Now
                    });
                }
                Role_Punished punished = (from punish in db.Role_Punished
                    where punish.DriverId == DriverId
                    select punish).First();
                switch (reason)
                {
                    case punishReason.RPMWarning:
                        punished.expirationDate = punished.expirationDate.AddDays(RPMWarning);
                        break;

                    case punishReason.RPMPunish:
                        punished.expirationDate = punished.expirationDate.AddDays(RPMPunish);
                        break;

                    case punishReason.SpeedWarning:
                        punished.expirationDate = punished.expirationDate.AddDays(SpeedWarning);
                        break;

                    case punishReason.SpeedPunish:
                        punished.expirationDate = punished.expirationDate.AddDays(SpeedPunish);
                        break;

                    case punishReason.EngineTempWarning:
                        punished.expirationDate = punished.expirationDate.AddDays(EngineTempWarning);
                        break;

                    case punishReason.EngineTempPunish:
                        punished.expirationDate = punished.expirationDate.AddDays(EngineTempPunish);
                        break;
                }
                db.SaveChanges();
            }
        }

        private static Role_RPMLimit GetRPMLimitRestriction(Guid carToDriverGuid)
        {
            using (var db = new CarManagerDbEntities())
            {
                if (db.Role_RPMLimit.Any(rest => rest.CarToDriverId == carToDriverGuid))
                {
                    return (from limit in db.Role_RPMLimit
                        where limit.CarToDriverId == carToDriverGuid
                        select limit).First();
                }
                else
                {
                    return null;
                }
            }
        }

        public static Guid? GetCarToDriverId(Guid carId, Guid driverId)
        {
            using (var db = new CarManagerDbEntities())
            {
                var result = from u in db.CarsToDrivers
                    where u.CarId == carId && u.DriverId == driverId
                    select u;
                if (result.Count() > 0)
                {
                    return result.First().Id;
                }
                else
                {
                    return null;
                }
            }
        }

        public static async Task<Guid?> EmergencyDriveByFacebookId(string providerUserId, string carPlateNumber)
        {
            Guid? driverId = GetDriverId(providerUserId);
            if (!driverId.HasValue)
            {
                return null;
            }
            return await EmergencyDrive(driverId, carPlateNumber);
        }

        public static async Task<Guid?> EmergencyDrive(Guid? driverId, string carPlateNumber)
        {
            Guid? carId = GetCarId(carPlateNumber);
            if (!carId.HasValue)
            {
                return null;
            }

            return await RoleService.AddRide(driverId.Value, DateTime.Now, carPlateNumber, true);
        }

        public static List<KeyValuePair<Guid, string>> GetCompanies()
        {
            var Companies = new List<KeyValuePair<Guid, string>>();
            using (var db = new CarManagerDbEntities())
            {
                foreach (Company comp in db.Companies)
                {
                    Companies.Add(new KeyValuePair<Guid, string>(comp.Id, comp.Name));
                }
                return Companies;
            }
        }

        private static List<Guid> GetEmergencyAllowedDrivers(Guid carId)
        {
            List<Guid> retList = new List<Guid>();

            using (var db = new CarManagerDbEntities())
            {
                DateTime Now = DateTime.Now;
                var notAllowedDrivers = from u in db.Role_Punished
                    where u.expirationDate > Now
                    select u.DriverId;

                var driversId = from u in db.CarsToDrivers
                    where u.CarId == carId && !notAllowedDrivers.Contains(u.Id)
                    select u.DriverId;
                foreach (Guid driverId in driversId)
                {
                    retList.Add(driverId);
                }
            }
            return retList;
        }

        public static KeyValuePair<List<Guid>, List<Guid>>? GetOfflineAndEmergencyAllowedDrivers(string carNumber)
        {
            KeyValuePair<List<Guid>, List<Guid>>? res = null;
            Guid? carId = GetCarId(carNumber);
            if (carId.HasValue)
            {
                if (timeToUpdate(carId.Value))
                {
                    res = new KeyValuePair<List<Guid>, List<Guid>>(
                        GetEmergencyAllowedDrivers(carId.Value),
                        offlineAllowedDrivers(carId.Value));
                }
            }
            return res;
        }

        private static List<Guid> offlineAllowedDrivers(Guid carId)
        {
            List<Guid> retList = new List<Guid>();
            using (var db = new CarManagerDbEntities())
            {
                var driversId = from u in db.CarsToDrivers
                    where u.CarId == carId && u.offlineRideAllowed == true
                    select u.DriverId;
                foreach (Guid driverId in driversId)
                {
                    retList.Add(driverId);
                }
            }
            return retList;
        }

        private static bool timeToUpdate(Guid carId)
        {
            using (var db = new CarManagerDbEntities())
            {
                var carToUpdate = from car in db.Cars
                    where car.Id == carId
                    select car;
                bool timeToUpdate = carToUpdate.Count() > 0;

                if (timeToUpdate)
                {
                    var car = carToUpdate.First(u => u.Id == carId);
                    car.LastPermissionUpdate = DateTime.Now;
                }
                return timeToUpdate;
            }
        }

        public static List<Car> GetCarListByCompany(string companyName)
        {
            using (var db = new CarManagerDbEntities())
            {
                List<Car> carList = new List<Car>();

                Guid? companyId = GetCompanyId(companyName);
                if (companyId.HasValue)
                {
                    carList = db.Cars.Where(car => car.CompanyId == companyId.Value).ToList<Car>();
                }
                return carList;
            }
        }

        public static Guid? GetCompanyId(string companyName)
        {
            using (var db = new CarManagerDbEntities())
            {
                if (string.IsNullOrEmpty(companyName))
                {
                    return null;
                }
                string lowCompanyName = companyName.ToLower();
              
                if (db.Companies.Any(comp => comp.Name.ToLower() == lowCompanyName))
                {
                    return db.Companies.First((comp => comp.Name.ToLower() == lowCompanyName)).Id;
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool DriverInCompany(Guid driverGuid, Guid companyGuid)
        {
            using (var db = new CarManagerDbEntities())
            {
                return db.DriverToCompanies.Any(dtc => dtc.CompanyId == companyGuid && dtc.DriverId == driverGuid);
            }
        }


        public static void AddRPMRestriction(Guid carToDriverGuid, short WarningLimit, short warningMaxTimes,
            short punishLimit, short punishMaxTimes)
        {
            using (var db = new CarManagerDbEntities())
            {
                Role_RPMLimit rpmRestriction;
                if (db.Role_RPMLimit.Any(restriction => restriction.CarToDriverId == carToDriverGuid))
                {
                    rpmRestriction = db.Role_RPMLimit.First(restriction => restriction.CarToDriverId == carToDriverGuid);
                    rpmRestriction.WarningLimit = WarningLimit;
                    if (rpmRestriction.CountBrokeWarningLimit > warningMaxTimes)
                    {
                        // warning crossed
                    }
                    else
                    {
                        rpmRestriction.CountBrokeWarningLimit = 0;
                    }
                    rpmRestriction.MaxtBrokeWarningLimit = warningMaxTimes;
                    rpmRestriction.PunishmentLimit = punishLimit;
                    if (rpmRestriction.CountBrokePunishmentLimit > punishMaxTimes)
                    {
                        // punish driver
                    }
                    else
                    {
                        rpmRestriction.CountBrokeWarningLimit = 0;
                    }
                    rpmRestriction.MaxtBrokePunishmentLimit = punishMaxTimes;
                }
                else
                {
                    rpmRestriction = new Role_RPMLimit()
                    {
                        CarToDriverId = carToDriverGuid,
                        CountBrokePunishmentLimit = 0,
                        CountBrokeWarningLimit = 0,
                        MaxtBrokePunishmentLimit = punishMaxTimes,
                        MaxtBrokeWarningLimit = warningMaxTimes,
                        PunishmentLimit = punishLimit,
                        WarningLimit = WarningLimit
                    };
                    db.Role_RPMLimit.Add(rpmRestriction);
                }
                db.SaveChanges();
            }
        }

        public static void AddEngineTempRestriction<T>(Guid carToDriverGuid, short WarningLimit, short warningMaxTimes,
            short punishLimit, short punishMaxTimes)
        {
            using (var db = new CarManagerDbEntities())
            {
                Role_EngineTempLimit engineTempRestriction;

                if (db.Role_EngineTempLimit.Any(restriction => restriction.CarToDriverId == carToDriverGuid))
                {
                    engineTempRestriction =
                        db.Role_EngineTempLimit.First(restriction => restriction.CarToDriverId == carToDriverGuid);
                    engineTempRestriction.WarningLimit = WarningLimit;
                    if (engineTempRestriction.CountBrokeWarningLimit > warningMaxTimes)
                    {
                        // warning crossed
                    }
                    else
                    {
                        engineTempRestriction.CountBrokeWarningLimit = 0;
                    }
                    engineTempRestriction.MaxtBrokeWarningLimit = warningMaxTimes;
                    engineTempRestriction.PunishmentLimit = punishLimit;
                    if (engineTempRestriction.CountBrokePunishmentLimit > punishMaxTimes)
                    {
                        // punish driver
                    }
                    else
                    {
                        engineTempRestriction.CountBrokeWarningLimit = 0;
                    }
                    engineTempRestriction.MaxtBrokePunishmentLimit = punishMaxTimes;
                }
                else
                {
                    engineTempRestriction = new Role_EngineTempLimit()
                    {
                        CarToDriverId = carToDriverGuid,
                        CountBrokePunishmentLimit = 0,
                        CountBrokeWarningLimit = 0,
                        MaxtBrokePunishmentLimit = punishMaxTimes,
                        MaxtBrokeWarningLimit = warningMaxTimes,
                        PunishmentLimit = punishLimit,
                        WarningLimit = WarningLimit
                    };
                    db.Role_EngineTempLimit.Add(engineTempRestriction);
                }
                db.SaveChanges();
            }
        }

        public static void AddSpeedRestriction(Guid carToDriverGuid, short WarningLimit, short warningMaxTimes,
            short punishLimit, short punishMaxTimes)
        {
            using (var db = new CarManagerDbEntities())
            {
                Role_SpeedLimit speedRestriction;
                if (db.Role_SpeedLimit.Any(restriction => restriction.CarToDriverId == carToDriverGuid))
                {
                    speedRestriction =
                        db.Role_SpeedLimit.First(restriction => restriction.CarToDriverId == carToDriverGuid);
                    speedRestriction.WarningLimit = WarningLimit;
                    if (speedRestriction.CountBrokeWarningLimit > warningMaxTimes)
                    {
                        // warning crossed
                    }
                    else
                    {
                        speedRestriction.CountBrokeWarningLimit = 0;
                    }
                    speedRestriction.MaxtBrokeWarningLimit = warningMaxTimes;
                    speedRestriction.PunishmentLimit = punishLimit;
                    if (speedRestriction.CountBrokePunishmentLimit > punishMaxTimes)
                    {
                        // punish driver
                    }
                    else
                    {
                        speedRestriction.CountBrokeWarningLimit = 0;
                    }
                    speedRestriction.MaxtBrokePunishmentLimit = punishMaxTimes;
                }
                else
                {
                    speedRestriction = new Role_SpeedLimit()
                    {
                        CarToDriverId = carToDriverGuid,
                        CountBrokePunishmentLimit = 0,
                        CountBrokeWarningLimit = 0,
                        MaxtBrokePunishmentLimit = punishMaxTimes,
                        MaxtBrokeWarningLimit = warningMaxTimes,
                        PunishmentLimit = punishLimit,
                        WarningLimit = WarningLimit
                    };
                    db.Role_SpeedLimit.Add(speedRestriction);
                }
                db.SaveChanges();
            }
        }

        public static void AddDrivingDayRestrict(Guid carToDriverGuid, bool sunday, bool monday, bool tuesday,
            bool wednesday, bool thursday, bool friday, bool saturday)
        {
            using (var db = new CarManagerDbEntities())
            {
                Role_DayRestriction dayRestriction;

                if (db.Role_DayRestriction.Any(restriction => restriction.CarToDriverId == carToDriverGuid))
                {
                    dayRestriction =
                        db.Role_DayRestriction.First(restriction => restriction.CarToDriverId == carToDriverGuid);
                    dayRestriction.Sunday = sunday;
                    dayRestriction.Monday = monday;
                    dayRestriction.Tuesday = tuesday;
                    dayRestriction.Wednesday = wednesday;
                    dayRestriction.Thursday = thursday;
                    dayRestriction.Friday = friday;
                    dayRestriction.Saturday = saturday;
                }
                else
                {
                    dayRestriction = new Role_DayRestriction()
                    {
                        CarToDriverId = carToDriverGuid,
                        Sunday = sunday,
                        Monday = monday,
                        Tuesday = tuesday,
                        Wednesday = wednesday,
                        Thursday = thursday,
                        Friday = friday,
                        Saturday = saturday
                    };
                    db.Role_DayRestriction.Add(dayRestriction);
                }
                db.SaveChanges();
            }

        }

        public static void AddDrivingTimeRestrict(Guid carToDriverGuid, DateTime startTime, DateTime endTime)
        {
            using (var db = new CarManagerDbEntities())
            {
                Role_TimeRestriction timeRestriction;

                if (db.Role_TimeRestriction.Any(restriction => restriction.CarToDriverId == carToDriverGuid))
                {
                    timeRestriction =
                        db.Role_TimeRestriction.First(restriction => restriction.CarToDriverId == carToDriverGuid);
                    timeRestriction.StartTime = startTime;
                    timeRestriction.EndTime = endTime;

                }
                else
                {
                    timeRestriction = new Role_TimeRestriction()
                    {
                        CarToDriverId = carToDriverGuid,
                        StartTime = startTime,
                        EndTime = endTime
                    };
                    db.Role_TimeRestriction.Add(timeRestriction);
                }
                db.SaveChanges();
            }
        }

        public static int? GetRoleID(string roleName)
        {
            using (var authDb = new AuthDBEntities())
            {
                var roleId = from role in authDb.webpages_Roles
                    where role.RoleName == roleName
                    select role.RoleId;
                if (roleId.Count() > 0)
                {
                    return roleId.First();
                }
                else
                {
                    return null;
                }
            }
        }

        public static bool TimeToUpdateAuthorizedList(string carPlateNumber)
        {
            using (var db = new CarManagerDbEntities())
            {
                Guid? carGuid = GetCarId(carPlateNumber);
                if (carGuid.HasValue)
                {
                    Car car = db.Cars.Find(carGuid.Value);
                    return timeToUpdate(car);
                }
                else
                {
                    return false;
                }
            }
        }

        private static bool timeToUpdate(Car car)
        {
            if (car.LastPermissionUpdate.HasValue)
            {
                return (car.LastPermissionUpdate.Value.AddDays(7).Ticks - DateTime.Now.Ticks) < 0;
            }
            else
            {
                return true;
            }
        }

        public static RideInfoSum RideInfoSummarize(Guid rideId)
        {
            using (var db = new CarManagerDbEntities())
            {
                if (db.Rides.Any(ride => ride.Id == rideId))
                {

                    Ride ride = db.Rides.Find(rideId);
                    Driver driver = db.Drivers.Find(ride.DriverID);
                    Car car = db.Cars.Find(ride.CarID);
                    string driverName = string.Format("{0} {1}", driver.Name, driver.FamilyName);

                    return new RideInfoSum()
                    {
                        CarLicence = car.Number,
                        CarModel = car.Model,
                        DriverName = driverName,
                        StartTime = ride.StartDrive,
                        EndTime = ride.EndDrive,
                        RPM = highRPM(rideId),
                        Speed = avgSpeed(rideId),
                        Temp = highTemp(rideId)
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        private static double avgSpeed(Guid rideId)
        {
            using (var db = new CarManagerDbEntities())
            {
                double avg = 0;
                if (db.DrivePackages.Any(pack => pack.RideId == rideId && pack.Speed.HasValue))
                {
                    var avgSpeed = (from ride in db.DrivePackages
                        where ride.RideId == rideId && ride.Speed.HasValue
                        select ride.Speed.Value).Average();
                        
                    avg += avgSpeed;
                }
                return double.Parse(string.Format("{0:0.##}", avg));
            }
        }

        private static int highTemp(Guid rideId)
        {
            using (var db = new CarManagerDbEntities())
            {
                var TempDetails = (from details in db.DrivePackages
                    where details.RideId == rideId && details.EngineTemp.HasValue
                    select details.EngineTemp).OrderByDescending(i => i).Take(2);
                return TempDetails.Min().Value;
            }
        }

        private static int highRPM(Guid rideId)
        {
            using (var db = new CarManagerDbEntities())
            {
                var RPMDetails = (from details in db.DrivePackages
                    where details.RideId == rideId && details.RPM.HasValue
                    select details.RPM).OrderByDescending(i => i).Take(5);
                return RPMDetails.Min().Value;
            }
        }

        public static void SendDrivePackage(DrivePackage package)
        {
            using (var db = new CarManagerDbEntities())
            {
                if (db.Rides.Any(ride => ride.Id == package.RideId))
                {
                    Ride ride = db.Rides.Find(package.RideId);

                    checkPackageRestrictions(package);
                    db.DrivePackages.Add(package);
                    db.SaveChanges();
                }
            }
        }

        public static void NotifyUser(int userId, string topic, string message)
        {
        }

        public static List<Car> GetCarListByCompanyAndDriver(string companyName, int UserId)
        {
            using (var db = new CarManagerDbEntities())
            {
                List<Car> carsList = new List<Car>();
                Guid? driverGuid = GetDriverId(UserId);
                Guid? companyGuid = GetCompanyId(companyName);
                if (companyGuid.HasValue && driverGuid.HasValue)
                {
                    var cars = GetCarListByCompany(companyName);
                    foreach (var car in cars)
                    {
                        if (db.CarsToDrivers.Any(ctd => ctd.CarId == car.Id && ctd.DriverId == driverGuid.Value))
                        {
                            carsList.Add(car);
                        }
                    }
                }
                return carsList;
            }

        }

        public static List<ReportModel> CreateReport(List<Driver> drivers, List<Car> cars, DateTime startDate,
            DateTime endDate, bool allRides, Double? Price)
        {
            List<ReportModel> reports = new List<ReportModel>();
            List<CarsToDriver> carsToDriversList = GetCarsToDriversList(cars,drivers);
            foreach (CarsToDriver carsToDriver in carsToDriversList)
            {
                if (allRides)
                {
                    reports.Add(CreateReportAllRides(carsToDriver, Price));
                }
                else
                {
                    reports.Add(CreateReportByDates(carsToDriver,startDate, endDate, Price));
                }
            }

            return reports;
        }

        private static ReportModel CreateReportByDates(CarsToDriver carsToDriver, DateTime startDate, DateTime endDate, double? Price)
        {
            using (var db = new CarManagerDbEntities())
            {
                Car car = db.Cars.Find(carsToDriver.CarId);
                Driver driver = db.Drivers.Find(carsToDriver.DriverId);
                var rides = from ride in db.Rides
                    where
                        ride.CarID == car.Id && ride.DriverID == driver.Id && ride.StartDrive > startDate &&
                        (ride.EndDrive.HasValue && ride.EndDrive.Value < endDate || ride.StartDrive < endDate)
                    select ride;
                double dist = distanceByRides(rides);
                double priceForKm = Price.HasValue ? Price.Value : 0;
                double cost = dist*priceForKm;
                return new ReportModel()
                {
                    DriverName = string.Format("{0} {1}", driver.Name, driver.FamilyName),
                    CarLicence = car.Number.ToString(),
                    CarModel = car.Model,
                    Distance = dist,
                    Cost = cost
                };
            }
        }

        private static ReportModel CreateReportAllRides(CarsToDriver carsToDriver, double? Price)
        {
            using (var db = new CarManagerDbEntities())
            {
                Car car = db.Cars.Find(carsToDriver.CarId);
                Driver driver = db.Drivers.Find(carsToDriver.DriverId);
                double dist = DistanceByCarAndDriver(carsToDriver.CarId, carsToDriver.DriverId);
                double priceForKm = Price.HasValue ? Price.Value : 0;
                double cost = dist*priceForKm;
                return new ReportModel()
                {
                    DriverName = string.Format("{0} {1}", driver.Name, driver.FamilyName),
                    CarLicence = car.Number.ToString(),
                    CarModel = car.Model,
                    Distance = dist,
                    Cost = cost
                };
            }
        }

        private static List<CarsToDriver> GetCarsToDriversList(List<Car> cars, List<Driver> drivers)
        {
            using (var db = new CarManagerDbEntities())
            {
                var carIds = from car in cars
                    select car.Id;
                var driverIds = from driver in drivers
                    select driver.Id;

                return (from ctd in db.CarsToDrivers
                    where carIds.Contains(ctd.CarId) && driverIds.Contains(ctd.DriverId)
                    select ctd).ToList();
            }
        }
    }
}