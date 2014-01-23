using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace QueueIT.Security.Configuration
{
    [ConfigurationCollection(typeof(RepositorySettingSection), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
    public class RepositorySettingsCollection : ConfigurationElementCollection
    {
        private const string ItemPropertyName = "setting";

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
            return ((RepositorySettingSection)element).Name;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RepositorySettingSection();
        }
    }
}