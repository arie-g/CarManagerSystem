using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarManagerPhoneApp.Arduino
{
    public abstract class OperationToSend
    {
        public const string StartMessage = ">>>>>>";
        public const string EndMessage = "<<<<<<";
        public const string EmergencyDriverInitString = "@@@@@@";
        public const string PermanentDriverInitString = "!!!!!!!";
        public const string GetRidesInitString = "------";
        public const string GetCarIdInitString = "______";
        public const string IsEmerAllowedInitString = "))))))";
        public const string IsPerAllowedInitString = "((((((";
        public const string EmerCountInitString = "******";
        public const string PerCountInitString = "``````";
        public const string RideRequestInitString = "^^^^^^";
        public const string AddRideInitString = "%%%%%%";
        public const string DelEmerListInitString = "~~~~~~";
        public const string DelPerListInitString = "######";

        public abstract string FunctionName { get; }
        public abstract string InitString { get; }
    }

    public class PermanentDriver : OperationToSend
    {
        public override string FunctionName
        {
            get { return "AddPerDr"; }
        }

        public override string InitString
        {
            get { return PermanentDriverInitString; }
        }

        public string driverId;
    }

    public class EmergencyDriver : OperationToSend
    {
        public override string FunctionName
        {
            get { return "AddEmerDr"; }
        }

        public override string InitString
        {
            get { return EmergencyDriverInitString; }
        }


        public string driverId;
        public string count;
    }

    public class DeletePermanentList : OperationToSend
    {
        public override string FunctionName
        {
            get { return "DelPerList"; }
        }

        public override string InitString
        {
            get { return DelPerListInitString; }
        }
    }

    public class DeleteEmergencyList : OperationToSend
    {
        public override string FunctionName
        {
            get { return "DelEmerList"; }
        }

        public override string InitString
        {
            get { return DelEmerListInitString; }
        }
    }

    public class NewRide : OperationToSend
    {
        public override string FunctionName
        {
            get { return "AddRide"; }
        }

        public override string InitString
        {
            get { return AddRideInitString; }
        }

        public string dId;
        public string cId;
        public string time;
        //  public 
    }

    public class RideRequest : OperationToSend
    {
        public override string FunctionName
        {
            get { return "RideRequest"; }
        }

        public override string InitString
        {
            get { return RideRequestInitString; }
        }

        public string driverId;
    }

    public class GetPermaNum : OperationToSend
    {
        public override string FunctionName
        {
            get { return "PerCount"; }
        }

        public override string InitString
        {
            get { return PerCountInitString; }
        }
    }

    public class GetEmerNum : OperationToSend
    {
        public override string FunctionName
        {
            get { return "EmerCount"; }
        }

        public override string InitString
        {
            get { return EmerCountInitString; }
        }
    }

    public class IsPermaAllowedToDrive : OperationToSend
    {
        public override string FunctionName
        {
            get { return "IsPerAllowed"; }
        }

        public override string InitString
        {
            get { return IsPerAllowedInitString; }
        }

        public string driverId;
    }

    public class IsEmerAllowedToDrive : OperationToSend
    {
        public override string FunctionName
        {
            get { return "IsEmerAllowed"; }
        }

        public override string InitString
        {
            get { return IsEmerAllowedInitString; }
        }

        public string driverId;
    }

    public class GetCarId : OperationToSend
    {
        public override string FunctionName
        {
            get { return "GetCarId"; }
        }

        public override string InitString
        {
            get { return GetCarIdInitString; }
        }
    }

    public class GetRidesList : OperationToSend
    {
        public override string FunctionName
        {
            get { return "GetRides"; }
        }

        public override string InitString
        {
            get { return GetRidesInitString; }
        }
    }
}
