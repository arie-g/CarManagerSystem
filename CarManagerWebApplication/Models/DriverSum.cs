using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dal;

namespace CarManagerWebApplication.Models
{
    public class UserSum
    {
        public List<String> Notifications { get; set; }
        public string FullName { get; set; }
        public List<EndedRide> LastRides { get; set; }
        public List<CarSum> CarsSum { get; set; }
    }
}