using Lessium.Models;
using System;

namespace Lessium.Interfaces
{
    /// <summary>
    /// Support sending UndoableActions to subscribers, therefore they will process it instead.
    /// </summary>
    public interface IActionSender
    {
        event EventHandler<SendActionEventArgs> SendAction;
    }
}
