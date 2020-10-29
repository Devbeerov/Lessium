namespace Lessium.Interfaces
{
    /// <summary>
    /// Basic interface for ContentControl.
    /// </summary>
    public interface IContentControl
    {
        // Similar to IsReadOnly
        void SetEditable(bool editable);
    }
}
