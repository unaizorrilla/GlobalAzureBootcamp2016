
namespace AlarmsTopology
{
    using Microsoft.SCP;
    using Microsoft.SCP.Topology;
    using System;
    using System.Collections.Generic;
    using System.Configuration;



    [Active(true)]
    public class AlarmsOnAggregatesTopology : TopologyDescriptor
    {
        public ITopologyBuilder GetTopologyBuilder()
        {
            TopologyBuilder topologyBuilder = new TopologyBuilder(typeof(AlarmsOnAggregatesTopology).Name + DateTime.Now.ToString("yyyyMMddHHmmss"));

            var eventHubPartitions = int.Parse(ConfigurationManager.AppSettings["EventHubPartitions"]);

            topologyBuilder.SetEventHubSpout(
                "com.microsoft.eventhubs.spout.EventHubSpout",
                new EventHubSpoutConfig(
                    ConfigurationManager.AppSettings["EventHubSharedAccessKeyName"],
                    ConfigurationManager.AppSettings["EventHubSharedAccessKey"],
                    ConfigurationManager.AppSettings["EventHubNamespace"],
                    ConfigurationManager.AppSettings["EventHubEntityPath"],
                    eventHubPartitions),
                eventHubPartitions);

            List<string> javaSerializerInfo = new List<string>() { "microsoft.scp.storm.multilang.CustomizedInteropJSONSerializer" };


            //FLATT MESSAGES
            topologyBuilder.SetBolt(
                typeof(FlattBolt).Name,
                FlattBolt.Get,
                new Dictionary<string, List<string>>()
                {
                    {Constants.DEFAULT_STREAM_ID, new List<string>(){ "sensor" } }
                },
                eventHubPartitions,
                true
                ).DeclareCustomizedJavaSerializer(javaSerializerInfo)
                .shuffleGrouping("com.microsoft.eventhubs.spout.EventHubSpout");

            //AGGREGATE MESSAGES
            var boltConfig = new StormConfig();
            boltConfig.Set("topology.tick.tuple.freq.secs", "10");

            topologyBuilder.SetBolt(
                typeof(AggregateBolt).Name,
                AggregateBolt.Get,
                new Dictionary<string, List<string>>()
                {
                    {Constants.DEFAULT_STREAM_ID, new List<string>(){ "aggregated" } }
                },
                parallelismHint: 1,
                enableAck: false
                )
                .globalGrouping(typeof(FlattBolt).Name)
                .addConfigurations(boltConfig);

            //ALARMS MESSAGES
            topologyBuilder.SetBolt(
               typeof(AlarmsBolt).Name,
               AlarmsBolt.Get,
               new Dictionary<string, List<string>>() { },
               parallelismHint: 4,
               enableAck: false
               ).shuffleGrouping(typeof(AggregateBolt).Name);

            var topologyConfig = new StormConfig();
            topologyConfig.setMaxSpoutPending(8192);
            topologyConfig.setNumWorkers(eventHubPartitions);

            topologyBuilder.SetTopologyConfig(topologyConfig);
            return topologyBuilder;
        }
    }
}
