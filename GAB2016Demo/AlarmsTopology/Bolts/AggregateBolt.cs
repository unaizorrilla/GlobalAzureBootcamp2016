
namespace AlarmsTopology
{
    using Microsoft.SCP;
    using System;
    using System.Linq;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AggregateBolt : ISCPBolt
    {
        Context ctx;

        ConcurrentDictionary<string, List<double>> _aggregateValues = new ConcurrentDictionary<string, List<double>>();

        public AggregateBolt(Context ctx)
        {
            this.ctx = ctx;

            // set input schemas
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(String) });
            inputSchema.Add(Constants.SYSTEM_TICK_STREAM_ID, new List<Type>() { typeof(long) });

            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(String) });

            // Declare input and output schemas
            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, outputSchema));
        }

        public void Execute(SCPTuple tuple)
        {
            if (tuple.IsTick())
            {
                Context.Logger.Warn("ON TICK, Ticks Number {0}", _aggregateValues.Keys.Count);

                foreach (var item in _aggregateValues.Keys)
                {
                    List<double> values;
                    if (_aggregateValues.TryGetValue(item, out values))
                    {
                        var average = values.Average();

                        var aggregate = new SensorAggregate()
                        {
                            Name = item,
                            Average = average
                        };

                        Context.Logger.Info("Emiting aggreagate");

                        var serialized = JsonConvert.SerializeObject(aggregate);

                        ctx.Emit(Constants.DEFAULT_STREAM_ID, new List<object>() { serialized });
                    }
                }
            }
            else
            {
                var sensor = tuple.GetSensor();

                if (sensor != null)
                {
                    _aggregateValues.AddOrUpdate(sensor.Name, new List<double>() { sensor.Value }, (key, currentValues) =>
                    {
                        currentValues.Add(sensor.Value);

                        return currentValues;
                    });
                }
            }
        }

        public static AggregateBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new AggregateBolt(ctx);
        }
    }
}