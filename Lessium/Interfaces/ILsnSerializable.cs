using Lessium.Classes.IO;
using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lessium.Interfaces
{
    /// <summary>
    /// Custom interface for Lsn format serialization. Requires implemented ISerializable.
    /// </summary>
    public interface ILsnSerializable : ISerializable
    {
        Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token);
        Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token);
    }
}
