using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace EventLogsCreateRemove.CustomConfigSection
{
    public class EventLogsSection : ConfigurationSection
    {
        /// <summary>
        /// retrieve the EventLogsSection from the app.config
        /// </summary>
        /// <returns></returns>
        public static EventLogsSection Settings
        {
            get
            {
                return ConfigurationManager.GetSection("eventLogs") as EventLogsSection;
            }
        }

        [ConfigurationProperty("machineName", IsRequired = false)]
        public string MachineName { get { return (string)this["machineName"]; } }

        [ConfigurationProperty("add", IsRequired = false)]
        public AddElement Add { get { return this["add"] as AddElement; } }

        [ConfigurationProperty("remove", IsRequired = false)]
        public RemoveElement Remove { get { return this["remove"] as RemoveElement; } }
    }
}
