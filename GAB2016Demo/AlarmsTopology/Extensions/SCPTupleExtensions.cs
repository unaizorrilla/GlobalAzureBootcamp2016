namespace AlarmsTopology
{
    using Microsoft.SCP;
    using Newtonsoft.Json;
    using System;

    public static class SCPTupleExtensions
    {
        public static Sensor GetSensor(this SCPTuple tuple)
        {
            try
            {
                Sensor sensor = null;

                if (tuple.Size() == 1)
                {
                    var content = tuple.GetString(0)
                    .Replace("\"", "'")
                    .Replace("\\", "");

                    sensor = JsonConvert.DeserializeObject<Sensor>(@content);
                }

                return sensor;
            }
            catch (Exception ex)
            {
                Context.Logger.Error("JSON Serialization error:{0}", ex.ToString());

                return null;
            }

        }
        public static SensorAggregate GetSensorAggregate(this SCPTuple tuple)
        {
            try
            {
                SensorAggregate sensor = null;

                if (tuple.Size() == 1)
                {
                    var content = tuple.GetString(0)
                    .Replace("\"", "'")
                    .Replace("\\", "");

                    sensor = JsonConvert.DeserializeObject<SensorAggregate>(@content);
                }

                return sensor;
            }
            catch (Exception ex)
            {
                Context.Logger.Error("JSON Serialization error:{0}", ex.ToString());

                return null;
            }

        }

        public static bool IsTick(this SCPTuple tuple)
        {
            return tuple.GetSourceStreamId()
                .Equals(Constants.SYSTEM_TICK_STREAM_ID, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class Sensor
    {
        public string Name { get; set; }

        public double Value { get; set; }
    }

    public class SensorAggregate
    {
        public string Name { get; set; }

        public double Average { get; set; }
    }
}
