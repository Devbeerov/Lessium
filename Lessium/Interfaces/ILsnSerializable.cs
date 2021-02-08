using Lessium.Classes.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lessium.Interfaces
{
    /// <summary>
    /// Custom interface for Lsn format serialization. Methods returns Task, therefore they're support await.
    /// </summary>
    public interface ILsnSerializable
    {
        Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token);
        Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token);
    }
}
