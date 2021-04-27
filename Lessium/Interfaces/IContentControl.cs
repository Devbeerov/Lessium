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
        // MaxWidth, MaxHeight

        void SetMaxWidth(double width);
        void SetMaxHeight(double height);

        // Events

        event RoutedEventHandler RemoveControl;
        event SizeChangedEventHandler Resize;

        // Preferable to implement with Dependency Properties

        bool IsReadOnly { get; set; }
    }
}
