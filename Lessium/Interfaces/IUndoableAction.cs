namespace Lessium.Interfaces
{
    public interface IUndoableAction
    {
        void ExecuteDo();
        void Undo();
    }
}
