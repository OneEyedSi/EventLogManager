using System;
using System.Configuration;

namespace EventLogManager.CustomConfigSection
{
    public class ListElement : ConfigurationElement
    {
        [ConfigurationProperty("eventLogs", IsDefaultCollection = true, IsRequired = false)]
        public EventLogsCollection EventLogs 
        { 
            get 
            {
                EventLogsCollection eventLogsToList = new EventLogsCollection();
                EventLogsCollection configEventLogsToList = 
                    this["eventLogs"] as EventLogsCollection;
                if (configEventLogsToList != null && configEventLogsToList.Count > 0)
                {
                    eventLogsToList = configEventLogsToList;
                }
                return eventLogsToList;
            } 
        }
    }
}
