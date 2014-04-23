using System;
using System.Configuration;

namespace EventLogsCreateRemove.CustomConfigSection
{
    public class RemoveElement : ConfigurationElement
    {
        [ConfigurationProperty("eventLogs", IsDefaultCollection = true, IsRequired = false)]
        [ConfigurationCollection(typeof(EventLogsCollection))]
        public EventLogsCollection EventLogs { get { return this["eventLogs"] as EventLogsCollection; } }

        [ConfigurationProperty("eventSources", IsRequired = false, IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(EventSourcesCollection))]
        public EventSourcesCollection EventSources  { get { return this["eventSources"] as EventSourcesCollection; } }
    }
}
