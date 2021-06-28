namespace Lessium.Interfaces
{
    /// <summary>
    /// Basic interface for ContentControl.
    /// </summary>
    public interface IContentControl : ILsnSerializable
    {
        // Preferable to implement through Dependency Property
        bool IsEditable { get; set; }
    }
}
