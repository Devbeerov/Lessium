using System;

namespace Lessium.Interfaces
{
    public interface IUndoableAction
    {
        void ExecuteDo();
        void Undo();

        Action Callback { get; set; }
        Action UndoCallback { get; set; }
    }
}
