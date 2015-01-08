using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarManagerWebApplication.Models
{
    public class AddCarsToDriverModel
    {
        public List<SelectListItem> PossibleCars { get; set; }
        public List<SelectListItem> PossibleDrivers { get; set; }
        public bool offlineRideAllowed { get; set; }
    }    
}