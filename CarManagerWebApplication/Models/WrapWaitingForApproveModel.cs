using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarManagerWebApplication.Models
{
    public class WrapWaitingForApproveModel
    {
        public List<Driver> Drivers { get; set; }
        public List<Guid> SelectedDrivers { get; set; }
    }
}