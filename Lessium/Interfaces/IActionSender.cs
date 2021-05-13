using Lessium.Models;

namespace Lessium.Interfaces
{
    /// <summary>
    /// Support sending UndoableActions to subscribers, therefore they will process it instead.
    /// </summary>
    public interface IActionSender
    {
        event SendActionEventHandler SendAction;
    }
}
