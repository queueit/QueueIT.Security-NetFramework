using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace QueueIT.Security.Configuration
{
    [ConfigurationCollection(typeof(QueueSection), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
    public class QueueCollection : ConfigurationElementCollection
    {
        internal const string ItemPropertyName = "queue";

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMapAlternate; }
        }

        protected override string ElementName
        {
            get { return ItemPropertyName; }
        }

        protected override bool IsElementName(string elementName)
        {
            return (elementName == ItemPropertyName);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((QueueSection)element).Name;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new QueueSection();
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        public void Add(QueueSection section)
        {
            this.BaseAdd(section);
        }

        public IEnumerable<QueueSection> GetGeneric()
        {
            return this.Cast<object>().Select(section => section as QueueSection);
        }
    }
}
