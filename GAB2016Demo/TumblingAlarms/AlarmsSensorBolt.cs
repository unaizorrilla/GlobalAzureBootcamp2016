namespace EventHubsReaderTopology
{
    using Microsoft.SCP;
    using System;
    using System.Collections.Generic;

    public class AlarmsSensorBolt : ISCPBolt
    {
        Context ctx;

        public AlarmsSensorBolt(Context ctx)
        {
            this.ctx = ctx;

            // set input schemas
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string), typeof(double) });

            // set output schemas
            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string) });

            // Declare input and output schemas
            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, outputSchema));

        }

        /// <summary>
        /// The Execute() function will be called, when a new tuple is available.
        /// </summary>
        /// <param name="tuple"></param>
        public void Execute(SCPTuple tuple)
        {
            var sensor = tuple.GetString(0);
            var value = tuple.GetDouble(1);

            //really add logic here!!
            if (sensor.Equals("Sensor-A", StringComparison.InvariantCultureIgnoreCase)
                &&
                value > 10)
            {
                Context.Logger.Warn("The sensor {0} throw alarms by {1} value", sensor, value);

                ctx.Emit(Constants.DEFAULT_STREAM_ID, new List<object>() { sensor });
            }

            this.ctx.Ack(tuple);
        }

        /// <summary>
        ///  Implements of delegate "newSCPPlugin", which is used to create a instance of this spout/bolt
        /// </summary>
        /// <param name="ctx">SCP Context instance</param>
        /// <param name="parms">Parameters to initialize this spout/bolt</param>
        /// <returns></returns>
        public static AlarmsSensorBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new AlarmsSensorBolt(ctx);
        }
    }
}