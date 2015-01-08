using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarManagerWebApplication.Models
{
    public class CarsModelView
    {
        public System.Guid Id { get; set; }
        public string Model { get; set; }
        public long Number { get; set; }
    }
}