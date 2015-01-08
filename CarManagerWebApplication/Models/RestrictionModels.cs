using Dal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarManagerWebApplication.Models
{
    public class RestrictionModel
    {
        public List<Dal.Driver> Drivers { get; set; }
        public List<Dal.Car> Cars { get; set; }
        public List<Guid> SelectedDrivers { get; set; }
        public List<Guid> SelectedCars { get; set; }
    }

    public class AddTimeRestrictionModel : RestrictionModel
    {
        [DisplayFormat(DataFormatString = "{0:t}",
               ApplyFormatInEditMode = true)]
        public DateTime StartTime { get; set; }
         [DisplayFormat(DataFormatString = "{0:t}",
               ApplyFormatInEditMode = true)]
        public DateTime EndTime { get; set; }
    }

    public class AddDayRestrictionModel : RestrictionModel
    {
        public Nullable<bool> Sunday { get; set; }
        public Nullable<bool> Monday { get; set; }
        public Nullable<bool> Tuesday { get; set; }
        public Nullable<bool> Wednesday { get; set; }
        public Nullable<bool> Thursday { get; set; }
        public Nullable<bool> Friday { get; set; }
        public Nullable<bool> Saturday { get; set; }
    }

    public class AddRPMLimitModel : RestrictionModel
    {
        public short WarningLimit { get; set; }
        public short MaxtBrokeWarningLimit { get; set; }
        public short PunishmentLimit { get; set; }
        public short MaxtBrokePunishmentLimit { get; set; }
    }

    public class AddSpeedLimitModel : RestrictionModel
    {
        public short WarningLimit { get; set; }
        public short MaxtBrokeWarningLimit { get; set; }
        public short PunishmentLimit { get; set; }
        public short MaxtBrokePunishmentLimit { get; set; }
    }

    public class AddEngineTempLimitModel : RestrictionModel
    {
        public short WarningLimit { get; set; }
        public short MaxtBrokeWarningLimit { get; set; }
        public short PunishmentLimit { get; set; }
        public short MaxtBrokePunishmentLimit { get; set; }
    }

    public class WrapTimeRestriction
    {
        public Car Car { get; set; }
        public Driver Driver { get; set; }
        public Dal.Role_TimeRestriction TimeRestriction { get; set; }
    }


    public class WrapDayRestriction
    {
        public Car Car { get; set; }
        public Driver Driver { get; set; }
        public Dal.Role_DayRestriction DayRestriction { get; set; }
    }

    public class WrapRPMLimit
    {
        public Car Car { get; set; }
        public Driver Driver { get; set; }
        public Dal.Role_RPMLimit RPMLimit { get; set; }
    }

    public class WrapSpeedLimit
    {
        public Car Car { get; set; }
        public Driver Driver { get; set; }
        public Dal.Role_SpeedLimit SpeedLimit { get; set; }
    }

    public class WrapEngineTempLimit
    {
        public Car Car { get; set; }
        public Driver Driver { get; set; }
        public Dal.Role_EngineTempLimit EngineTempLimit { get; set; }
    }
}
