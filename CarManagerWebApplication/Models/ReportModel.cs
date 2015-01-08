using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarManagerWebApplication.Models
{
    public class ReportModel
    {
        public string DriverName { get; set; }
        public string CarModel { get; set; }
        public string CarLicence { get; set; }
        public double Distance { get; set; }
        public double Cost { get; set; }
    }
}