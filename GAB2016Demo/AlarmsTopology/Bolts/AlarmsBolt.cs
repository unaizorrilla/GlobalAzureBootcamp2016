
namespace AlarmsTopology
{
    using Microsoft.SCP;
    using System;
    using System.Collections.Generic;

    public class AlarmsBolt : ISCPBolt
    {
        Context ctx;


        public AlarmsBolt(Context ctx)
        {
            this.ctx = ctx;

            // set input schemas
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(String) });

            // Declare input and output schemas
            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, null));
        }

        public void Execute(SCPTuple tuple)
        {
            var aggregate = tuple.GetSensorAggregate();

            if (aggregate != null)
            {
                if (aggregate.Name.Equals("Sensor-A", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (aggregate.Average > .5)
                    {
                        //WRITE ALARMS!!
                        Context.Logger.Warn("THE SENSOR {0} AVERAGE {1} IS GRATHER THAN .5  NEW ALARM",aggregate.Name,aggregate.Average);
                    }
                    else
                    {
                        Context.Logger.Warn("NO ALARM ON AGGREGATE {0}",aggregate.Name);
                    }
                }
            }
        }

        public static AlarmsBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new AlarmsBolt(ctx);
        }
    }
}