namespace CarManagerPhoneApp
{
    public class CarDataBll
    {
        private const int HighRpm = 4000;
        private const int HighTemp = 115;

        public static void CheckAndProcessData(CarData carData)
        {
            UpdateSpeedString(carData);
            UpdateRpmString(carData);
            UpdateTempString(carData);
            carData.needToUpdate = false;
        }

        private static void UpdateTempString(CarData carData)
        {
            carData.IsHighTemp = carData.EngineCoolantTemperature > HighTemp;
            carData.TemperatureString = string.Format("{0}", carData.EngineCoolantTemperature);
        }

        private static void UpdateRpmString(CarData carData)
        {
            carData.IsHighRpm = carData.EngineRpm > HighRpm;
            carData.RpmString = string.Format("{0}", carData.EngineRpm);
        }

        private static void UpdateSpeedString(CarData carData)
        {
            carData.SpeedString = carData.Speed.ToString();
        }
    }
}
