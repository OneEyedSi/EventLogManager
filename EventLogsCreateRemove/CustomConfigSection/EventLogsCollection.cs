using System;
using System.Configuration;

namespace EventLogsCreateRemove.CustomConfigSection
{
    public class EventLogsCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// The ElementName and CollectionType properties are required when creating a BasicMap 
        /// collection.  An AddRemoveClearMap collection does not need them.
        /// </summary>
        /// </remarks>
        protected override string ElementName
        {
            get { return "eventLog"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        public EventLogElement this[string name]
        {
            get
            {
                return base.BaseGet(name) as EventLogElement;
            }
        }

        public EventLogElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as EventLogElement;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new EventLogElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EventLogElement)element).Name;
        }
    }
}
