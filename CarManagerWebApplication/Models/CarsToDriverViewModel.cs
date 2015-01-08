using System;

namespace CarManagerWebApplication.Models
{
    public class CarsToDriverViewModel
    {
        public Guid Id { get; set; }
        public Guid CarId { get; set; }
        public Guid DriverId { get; set; }
        public string KilometersDriven { get; set; }    
    }
}