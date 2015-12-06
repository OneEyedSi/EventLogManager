using System;
using System.Configuration;

namespace EventLogManager.CustomConfigSection
{
    public class LogsAndSourcesElement : ConfigurationElement
    {
        [ConfigurationProperty("eventLogs", IsDefaultCollection = true, IsRequired = false)]
        public EventLogsCollection EventLogs { get { return this["eventLogs"] as EventLogsCollection; } }

        [ConfigurationProperty("eventSources", IsRequired = false, IsDefaultCollection = false)]
        public EventSourcesCollection EventSources { get { return this["eventSources"] as EventSourcesCollection; } }
    }
}
