using System;
using System.Configuration;

namespace EventLogsCreateRemove.CustomConfigSection
{
    public class AddElement : ConfigurationElement
    {
        [ConfigurationProperty("eventLogs", IsDefaultCollection = true, IsRequired = false)]
        public EventLogsCollection EventLogs { get { return this["eventLogs"] as EventLogsCollection; } }

        [ConfigurationProperty("eventSources", IsRequired = false, IsDefaultCollection = false)]
        public EventSourcesCollection EventSources  { get { return this["eventSources"] as EventSourcesCollection; } }
    }
}
