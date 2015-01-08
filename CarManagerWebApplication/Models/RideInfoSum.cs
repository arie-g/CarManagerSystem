using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarManagerWebApplication.Models
{
    public class RideInfoSum
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public long CarLicence { get; set; }
        public string CarModel { get; set; }
        public string DriverName { get; set; }
        public double Speed { get; set; }
        public int RPM { get; set; }
        public int Temp { get; set; }
    }
}