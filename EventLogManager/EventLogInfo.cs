using System.Collections.Generic;

namespace EventLogManager
{
    public class EventLogInfo
    {
        public string Name { get; set; }

        public IList<string> EventSourceNames { get; set; }

        public EventLogInfo(string name, IList<string> eventSourceNames)
        {
            this.Name = name;
            this.EventSourceNames = eventSourceNames;
        }
    }
}