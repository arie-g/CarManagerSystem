using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarManagerWebApplication.Models
{
    public class WrapDriverModel
    {
        public WrapDriverModel()
        {
            DoesDriverExist = true;
        }

        public Driver Driver { get; set; }
        public string Message { get; set; }
        public bool DoesDriverExist { get; set; }
    }
}