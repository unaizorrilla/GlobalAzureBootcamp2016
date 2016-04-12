using Microsoft.SCP;
using System;
using System.Collections.Generic;

namespace EventHubsReaderTopology
{
    /// <summary>
    /// Partially count number of messages from EventHubs
    /// </summary>
    public class ThresholdBolt : ISCPBolt
    {
        Context ctx;

        public ThresholdBolt(Context ctx)
        {
            this.ctx = ctx;

            // set input schemas
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string) });

            //Add the Tick tuple Stream in input streams - A tick tuple has only one field of type long
            inputSchema.Add(Constants.SYSTEM_TICK_STREAM_ID, new List<Type>() { typeof(long) });

            // set output schemas
            //Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            //outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(long) });

            // Declare input and output schemas
            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, null));

            this.ctx.DeclareCustomizedDeserializer(new CustomizedInteropJSONDeserializer());
        }

        /// <summary>
        /// The Execute() function will be called, when a new tuple is available.
        /// </summary>
        /// <param name="tuple"></param>
        public void Execute(SCPTuple tuple)
        {
            ctx.Ack(tuple);

            //Log tuple content 
            Context.Logger.Warn(tuple.GetString(0));
        }

        /// <summary>
        ///  Implements of delegate "newSCPPlugin", which is used to create a instance of this spout/bolt
        /// </summary>
        /// <param name="ctx">SCP Context instance</param>
        /// <param name="parms">Parameters to initialize this spout/bolt</param>
        /// <returns></returns>
        public static ThresholdBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new ThresholdBolt(ctx);
        }
    }
}