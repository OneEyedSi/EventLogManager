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
        public string MachineName
        {
            get
            {
                // "." represents the local machine.  
                //  As seen in decompiled code in 
                //  System.Diagnostics.EventLog.FindSourceRegistration().
                string machineName = ".";
                string configMachineName = (string)this["machineName"];
                if (configMachineName != null && configMachineName.Trim().Length > 0)
                {
                    machineName = configMachineName;
                }
                return machineName;
            }
        }

        [ConfigurationProperty("add", IsRequired = false)]
        public LogsAndSourcesElement Add { get { return this["add"] as LogsAndSourcesElement; } }

        [ConfigurationProperty("remove", IsRequired = false)]
        public LogsAndSourcesElement Remove { get { return this["remove"] as LogsAndSourcesElement; } }

        [ConfigurationProperty("list", IsRequired = false)]
        public ListElement List { get { return this["list"] as ListElement; } }
    }
}
