using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarManagerWebApplication.Models
{
    public class WrapCarModel
    {
        public WrapCarModel()
        {
            DoesCarExist = true;
        }
        
        public bool DoesCarExist { get; set; }
        public Car Car { get; set; }
        public string Message { get; set; }
        public Guid CompanyId { get; set; }
    }
}
