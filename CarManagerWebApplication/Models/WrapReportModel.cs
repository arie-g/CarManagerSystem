using Dal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarManagerWebApplication.Models
{
    public class WrapReportModel
    {
        public List<Dal.Driver> Drivers { get; set; }
        public List<Dal.Car> Cars { get; set; }
        public List<Guid> SelectedDrivers { get; set; }
        public List<Guid> SelectedCars { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndtTime { get; set; }
    }
}