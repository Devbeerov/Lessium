using System;
using System.Windows.Controls;

namespace Lessium.Interfaces
{
    public interface IRemoveButtonRequestor
    {
        event RemoveButtonRequestEventHandler RequestRemoveButton;
    }

    public class RemoveButtonRequestEventArgs : EventArgs
    {
        public ContentPresenter RemoveButtonPresenter { get; private set; }

        public RemoveButtonRequestEventArgs(ContentPresenter removeButtonPresenter)
        {
            RemoveButtonPresenter = removeButtonPresenter;
        }
    }

    public delegate void RemoveButtonRequestEventHandler(RemoveButtonRequestEventArgs args);
}