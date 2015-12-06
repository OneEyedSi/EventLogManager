using System;
using System.Configuration;

namespace EventLogManager.CustomConfigSection
{
    public class EventSourcesCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// The ElementName and CollectionType properties are required when creating a BasicMap 
        /// collection.  An AddRemoveClearMap collection does not need them.
        /// </summary>
        /// </remarks>
        protected override string ElementName
        {
            get { return "eventSource"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }
        
        public EventSourceElement this[string name]
        {
            get
            {
                return base.BaseGet(name) as EventSourceElement;
            }
        }

        public EventSourceElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as EventSourceElement;
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
            return new EventSourceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EventSourceElement)element).Name;
        }
    }
}
