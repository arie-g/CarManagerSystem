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
    
    public partial class RideInfo
    {
        public string Name { get; set; }
        public long Number { get; set; }
        public System.DateTime StartDrive { get; set; }
        public Nullable<System.DateTime> EndDrive { get; set; }
        public System.Guid Id { get; set; }
    }
}