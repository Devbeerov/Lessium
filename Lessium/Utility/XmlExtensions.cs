using System;
using System.Threading.Tasks;
using System.Xml;

namespace Lessium.Utility
{
    public static class XmlWriterExtensions
    {
        public async static Task WriteStartElementAsync(this XmlWriter writer, string localName)
        {
            await writer.WriteStartElementAsync(null, localName, null);
        }

        public async static Task WriteAttributeStringAsync(this XmlWriter writer, string localName, string value)
        {
            await writer.WriteAttributeStringAsync(null, localName, null, value);
        }

        public static async Task<bool> ReadToFollowingAsync(this XmlReader reader, string localName)
        {
            if (localName == null || localName.Length == 0)
            {
                throw new ArgumentException("localName is empty or null");
            }

            // atomize local name and namespace
            localName = reader.NameTable.Add(localName);

            // find element with that name
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && localName == reader.LocalName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
