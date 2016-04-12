
namespace AzureEventHubsReaderStormApplication
{
    using Microsoft.SCP;
    using Newtonsoft.Json;


    public static class SCPTupleExtensions
    {
        public static Sensor GetSensor(this SCPTuple tuple)
        {
            return JsonConvert.DeserializeObject<Sensor>(
                tuple.GetString(0));
        }
    }

    public class Sensor
    {
        public string Name { get; set; }

        public double Value { get; set; }
    }
}
