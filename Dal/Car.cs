//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Dal
{
    using System;
    using System.Collections.Generic;
    
    public partial class Car
    {
        public Car()
        {
            this.CarsToDrivers = new HashSet<CarsToDriver>();
            this.Rides = new HashSet<Ride>();
        }
    
        public System.Guid Id { get; set; }
        public string Model { get; set; }
        public long Number { get; set; }
        public Nullable<System.DateTime> LastPermissionUpdate { get; set; }
        public System.Guid CompanyId { get; set; }
    
        public virtual ICollection<CarsToDriver> CarsToDrivers { get; set; }
        public virtual Company Company { get; set; }
        public virtual ICollection<Ride> Rides { get; set; }
    }
}
