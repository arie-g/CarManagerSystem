using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarManagerPhoneApp.Arduino
{
    public class RootObject
    {
        public List<RideArduino> rides { get; set; }
    }

    public class RideArduino
    {
        public string dId { get; set; }
        public string cId { get; set; }
        public string time { get; set; }
    }
}
