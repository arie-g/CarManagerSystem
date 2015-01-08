using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dal;

namespace CarManagerCommon
{
    [DataContract]
    public class DriveStatistics
    {
        [DataMember]
        public Guid CarId { get; set; }
        [DataMember]
        public Guid DriverId { get; set; }
        [DataMember]
        public DateTime StartDrive { get; set; }
        [DataMember]
        public DateTime FinishDrive { get; set; }
        [DataMember]
        public List<DrivePackage> Data { get; set; }
    }
}