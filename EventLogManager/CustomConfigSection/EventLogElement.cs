using System;
using System.Configuration;

namespace EventLogManager.CustomConfigSection
{
    public class EventLogElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name { get { return (string)this["name"]; } }

        // Declare the eventSources collection property.
        // Note: the "IsDefaultCollection = false" instructs 
        // .NET Framework to build a nested section of 
        // the kind <urls> ...</urls>.
        [ConfigurationProperty("eventSources", IsRequired = false, IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(EventSourcesCollection))]
        public EventSourcesCollection EventSources
        {
            get { return (EventSourcesCollection)base["eventSources"]; }
        }
    }
}
