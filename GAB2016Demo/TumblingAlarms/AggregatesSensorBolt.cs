namespace EventHubsReaderTopology
{
    using Microsoft.SCP;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AggregateSensorBolt : ISCPBolt
    {
        Context ctx;

        Queue<SCPTuple> tuplesToAck = new Queue<SCPTuple>();

        Dictionary<string, List<double>> _data = new Dictionary<string, List<double>>();

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
            outputSchema.Add(Constants.DEFAULT_STREAM_ID, new List<Type>() { typeof(string), typeof(double) });

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

                foreach (var key in _data.Keys)
                {
                    var avg = _data[key].Average();

                    this.ctx.Emit(Constants.DEFAULT_STREAM_ID, tuplesToAck, new Values(key, avg));
                }

                Context.Logger.Info("acking the batch: " + tuplesToAck.Count);

                foreach (var t in tuplesToAck)
                {
                    this.ctx.Ack(t);
                }

                _data.Clear();
                tuplesToAck.Clear();

            }
            else
            {
                var sensorName = tuple.GetString(0);
                var value = tuple.GetDouble(1);

                if (!_data.ContainsKey(tuple.GetString(0)))
                {
                    _data.Add(sensorName, new List<double>() { value });
                }
                else
                {
                    _data[sensorName].Add(value);
                }

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