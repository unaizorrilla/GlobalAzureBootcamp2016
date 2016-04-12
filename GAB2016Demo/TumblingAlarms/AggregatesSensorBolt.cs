using System;
using System.Collections.Generic;
using Microsoft.SCP;
using System.Diagnostics;
using System.Linq;
using AzureEventHubsReaderStormApplication;

namespace EventHubsReaderTopology
{
    public class AggregateSensorBolt : ISCPBolt
    {
        Context ctx;

        Queue<SCPTuple> tuplesToAck = new Queue<SCPTuple>();

        public AggregateSensorBolt(Context ctx)
        {
            this.ctx = ctx;

            // set input schemas
            Dictionary<string, List<Type>> inputSchema = new Dictionary<string, List<Type>>();
            inputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string) });

            //Add the Tick tuple Stream in input streams - A tick tuple has only one field of type long
            inputSchema.Add(Constants.SYSTEM_TICK_STREAM_ID, new List<Type>() { typeof(long) });

            // set output schemas
            Dictionary<string, List<Type>> outputSchema = new Dictionary<string, List<Type>>();
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string),typeof(double) });

            // Declare input and output schemas
            this.ctx.DeclareComponentSchema(new ComponentStreamSchema(inputSchema, outputSchema));

            this.ctx.DeclareCustomizedDeserializer(new CustomizedInteropJSONDeserializer());

        }

        /// <summary>
        /// The Execute() function will be called, when a new tuple is available.
        /// </summary>
        /// <param name="tuple"></param>
        public void Execute(SCPTuple tuple)
        {
            if (tuple.GetSourceStreamId().Equals(Constants.SYSTEM_TICK_STREAM_ID))
            {
                Context.Logger.Info("Aggregates tuple values..");

                var sensors = tuplesToAck
                    .Select(s => s.GetSensor())
                    .GroupBy(s => s.Name);

                foreach(var item in sensors)
                {
                    var avg = item.Select(s => s.Value)
                        .Average();

                    this.ctx.Emit(Constants.DEFAULT_STREAM_ID, tuplesToAck, new Values(item.Key,avg));
                }


                Context.Logger.Info("acking the batch: " + tuplesToAck.Count);

                foreach (var t in tuplesToAck)
                {
                    this.ctx.Ack(t);
                }

                tuplesToAck.Clear();

            }
            else
            {
                tuplesToAck.Enqueue(tuple);
            }
        }

        /// <summary>
        ///  Implements of delegate "newSCPPlugin", which is used to create a instance of this spout/bolt
        /// </summary>
        /// <param name="ctx">SCP Context instance</param>
        /// <param name="parms">Parameters to initialize this spout/bolt</param>
        /// <returns></returns>
        public static AggregateSensorBolt Get(Context ctx, Dictionary<string, Object> parms)
        {
            return new AggregateSensorBolt(ctx);
        }
    }
}