namespace EventHubsReaderTopology
{
    using Microsoft.SCP;
    using System;
    using System.Collections.Generic;

    public class FlattBolt
        : ISCPBolt
    {
        Context _ctx;

        public FlattBolt(Context ctx)
        {
            _ctx = ctx;

            // set input schemas
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string) });

            // set output schemas
            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string), typeof(double) });

            // Declare input and output schemas
            this._ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, outputSchema));

        }
        public void Execute(SCPTuple tuple)
        {
            var sensor = tuple.GetSensor();

            _ctx.Emit(Constants.DEFAULT_STREAM_ID, new List<object>() { sensor.Name, sensor.Value });

            _ctx.Ack(tuple);
        }

        /// <summary>
        ///  Implements of delegate "newSCPPlugin", which is used to create a instance of this spout/bolt
        /// </summary>
        /// <param name="ctx">SCP Context instance</param>
        /// <param name="parms">Parameters to initialize this spout/bolt</param>
        /// <returns></returns>
        public static FlattBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new FlattBolt(ctx);
        }
    }
}
