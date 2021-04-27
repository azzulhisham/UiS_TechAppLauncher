using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TechAppLauncher.Models.xml;

namespace TechAppLauncher.Helpers
{
    public class Serializer
    {
        public T Deserialize<T>(string input) where T : class
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (StringReader sr = new StringReader(input))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        public feed? DeserializeFeed(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(feed));
            using (StringReader reader = new StringReader(xml))
            {
                return (feed)serializer.Deserialize(reader);
            }

			return null;
        }
    }
}
