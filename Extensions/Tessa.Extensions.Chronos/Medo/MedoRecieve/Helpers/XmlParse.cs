using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Tessa.Extensions.Chronos.Medo.MedoRecieve.Helpers
{
    public static class XmlParse
    {
        public static string GetAttrValue(XElement element, string name)
        {
            XNamespace elementNamespace = element.Name.Namespace;

            var attribute = element.Attribute(elementNamespace + name);

            if (attribute == null)
            {
                return string.Empty;
            }

            var attributeValue = attribute.Value;

            if (string.IsNullOrWhiteSpace(attributeValue))
            {
                return string.Empty;
            }

            return attributeValue;
        }

        public static string GetDescendantValue(XElement element, string name)
        {
            XNamespace elementNamespace = element.Name.NamespaceName;

            var descendant = element.Descendants(elementNamespace + name).FirstOrDefault();

            if (descendant == null)
            {
                return string.Empty;
            }

            var descendantValue = descendant.Value;

            if (string.IsNullOrWhiteSpace(descendantValue))
            {
                return string.Empty;
            }

            return descendantValue;
        }

        public static string GetDescendantAttributeValue(XElement element, string descendantName, string attributeName)
        {
            try
            {
                XNamespace elementNamespace = element.Name.NamespaceName;

                var descendant = element.Descendants(elementNamespace + descendantName).FirstOrDefault();

                if (descendant == null)
                {
                    return string.Empty;
                }

                var descendantaAttributeValue = GetAttrValue(descendant, attributeName);

                if (string.IsNullOrEmpty(descendantaAttributeValue))
                {
                    return descendantaAttributeValue;
                }

                return descendantaAttributeValue;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
    }
}
