using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarManagerWebApplication.Models
{
    public class CarSum
    {
        public string Model { get; set; }
        public long Number { get; set; }
        public int NumberRides { get; set; }
        public double Distance { get; set; }
    }
}