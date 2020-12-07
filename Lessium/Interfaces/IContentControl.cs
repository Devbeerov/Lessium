﻿using System;
using System.Runtime.Serialization;
using System.Windows;

namespace Lessium.Interfaces
{
    /// <summary>
    /// Basic interface for ContentControl.
    /// </summary>
    public interface IContentControl : ISerializable
    {
        // Similar to IsReadOnly
        void SetEditable(bool editable);
        void SetMaxWidth(double width);
        void SetMaxHeight(double height);
        event RoutedEventHandler RemoveControl;
    }
}