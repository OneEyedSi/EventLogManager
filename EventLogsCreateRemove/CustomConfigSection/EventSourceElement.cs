﻿using System;
using System.Configuration;

namespace EventLogsCreateRemove.CustomConfigSection
{
    public class EventSourceElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name { get { return (string)this["name"]; } }
    }
}
