using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dal;

namespace CarManagerWebApplication.Models
{
    public class WrapEndedRidesModel
    {
        public List<EndedRide> EndedRides { get; set; }
        public Driver Driver { get; set; }
    }
}