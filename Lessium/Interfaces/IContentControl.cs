using System;
using System.Runtime.Serialization;
using System.Windows;

namespace Lessium.Interfaces
{
    /// <summary>
    /// Basic interface for ContentControl.
    /// </summary>
    public interface IContentControl : ILsnSerializable
    {
        // Similar to IsReadOnly
        void SetEditable(bool editable);

        // MaxWidth, MaxHeight

        void SetMaxWidth(double width);
        void SetMaxHeight(double height);

        // Events

        event RoutedEventHandler RemoveControl;
        event SizeChangedEventHandler Resize;
    }
}
