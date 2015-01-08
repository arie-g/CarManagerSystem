using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CarManagerWebApplication.Models;

namespace CarManagerWebApplication.Models
{
   

    public class DriverViewModel
    {
        public DriverViewModel()
        {
            this.CarsToDrivers = new HashSet<CarsToDriverViewModel>();
        }
        public Guid  Id { get; set; }
        public String Name { get; set; }
        public String FamilyName { get; set; }
        public String Licence { get; set; }
        public short? ExperienceYears { get; set; }
        public int UserId { get; set; }

        public virtual ICollection<CarsToDriverViewModel> CarsToDrivers { get; set; }
    }
}