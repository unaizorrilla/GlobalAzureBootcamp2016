namespace AlarmsTopology
{
    using Microsoft.SCP;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;


    public class FlattBolt : ISCPBolt
    {
        Context ctx;

        public FlattBolt(Context ctx)
        {
            this.ctx = ctx;

            // set input schemas
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string) });

            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(String) });

            // Declare input and output schemas
            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, outputSchema));

            this.ctx.DeclareCustomizedDeserializer(new CustomizedInteropJSONDeserializer());
        }

        public void Execute(SCPTuple tuple)
        {
            if (!tuple.IsTick())
            {
                var sensor = tuple.GetSensor();

                if (sensor != null)
                {
                    var serializedContent = JsonConvert.SerializeObject(sensor);

                    this.ctx.Emit(Constants.DEFAULT_STREAM_ID, new List<object>() { serializedContent });
                }

                this.ctx.Ack(tuple);
            }
        }

        public static FlattBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new FlattBolt(ctx);
        }
    }
}