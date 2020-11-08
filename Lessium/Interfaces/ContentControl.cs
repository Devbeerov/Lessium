using System.Runtime.Serialization;

namespace Lessium.Interfaces
{
    /// <summary>
    /// Basic interface for ContentControl.
    /// </summary>
    public interface IContentControl : ISerializable
    {
        // Similar to IsReadOnly
        void SetEditable(bool editable);
    }
}
