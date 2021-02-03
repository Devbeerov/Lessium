using System;
using System.Threading.Tasks;
using System.Xml;

namespace Lessium.Utility
{
    public static class XmlWriterExtensions
    {
        public static async Task WriteStartElementAsync(this XmlWriter writer, string localName)
        {
            await writer.WriteStartElementAsync(null, localName, null);
        }

        public static async Task WriteAttributeStringAsync(this XmlWriter writer, string localName, string value)
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

        public static async Task<int> CountChildsAsync(this XmlReader reader)
        {
            int count = 0;
            int childDepth = reader.Depth + 1; // Will be lower (higher value) than parent depth
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Depth == childDepth)
                    ++count;
            }
            return count;
        }
    }
}
