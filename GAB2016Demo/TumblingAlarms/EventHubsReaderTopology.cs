namespace EventHubsReaderTopology
{
    using Microsoft.SCP;
    using Microsoft.SCP.Topology;
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    [Active(true)]
    public class EventCountHybridTopology : TopologyDescriptor
    {
        public ITopologyBuilder GetTopologyBuilder()
        {
            TopologyBuilder topologyBuilder = new TopologyBuilder(typeof(EventCountHybridTopology).Name + DateTime.Now.ToString("yyyyMMddHHmmss"));

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

            // Set a customized JSON Serializer to serialize a Java object (emitted by Java Spout) into JSON string
            // Here, full name of the Java JSON Serializer class is required
            List<string> javaSerializerInfo = new List<string>() { "microsoft.scp.storm.multilang.CustomizedInteropJSONSerializer" };

            var boltConfig = new StormConfig();
            boltConfig.Set("topology.tick.tuple.freq.secs", "60");


            topologyBuilder.SetBolt(
                typeof(FlattBolt).Name,
                FlattBolt.Get,
                new Dictionary<string, List<string>>()
                {
                    {Constants.DEFAULT_STREAM_ID, new List<string>(){ "sensor","value" } }
                },
                parallelismHint: eventHubPartitions,
                enableAck: true
                ).
                DeclareCustomizedJavaSerializer(javaSerializerInfo).
                shuffleGrouping("com.microsoft.eventhubs.spout.EventHubSpout").
                addConfigurations(boltConfig);

            topologyBuilder.SetBolt(
                typeof(AggregateSensorBolt).Name,
                AggregateSensorBolt.Get,
                new Dictionary<string, List<string>>()
                {
                    {Constants.DEFAULT_STREAM_ID, new List<string>(){ "sensor","average" } }
                },
                10,
                true
                ).
                DeclareCustomizedJavaSerializer(javaSerializerInfo).
                fieldsGrouping(typeof(FlattBolt).Name, new List<int> { 0 }).
                addConfigurations(boltConfig);

            topologyBuilder.SetBolt(
                typeof(AlarmsSensorBolt).Name,
                AlarmsSensorBolt.Get,
                new Dictionary<string, List<string>>()
                {
                    {Constants.DEFAULT_STREAM_ID, new List<string>(){ "sensor" } }
                },
                1,
                true).
                globalGrouping(typeof(AggregateSensorBolt).Name).
                addConfigurations(boltConfig);

            var topologyConfig = new StormConfig();
            topologyConfig.setMaxSpoutPending(8192);
            topologyConfig.setNumWorkers(eventHubPartitions);

            topologyBuilder.SetTopologyConfig(topologyConfig);
            return topologyBuilder;
        }
    }
}
