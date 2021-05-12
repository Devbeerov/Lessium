using Lessium.Interfaces;
using System.Collections.Generic;

namespace Lessium.UndoableActions.Generic
{

    public class RemoveFromCollectionAction<T> : IUndoableAction
    {
        private readonly IList<T> list;
        private readonly T toRemove;

        private T storedObject;
        private int storedPosition;

        public RemoveFromCollectionAction(IList<T> list, T toRemove)
        {
            this.list = list;
            this.toRemove = toRemove;
        }

        public void ExecuteDo()
        {
            // Stores reference and position for undo

            storedObject = toRemove;
            storedPosition = list.IndexOf(toRemove);

            // Removes

            list.Remove(toRemove);
        }

        public void Undo()
        {
            list.Insert(storedPosition, storedObject);
        }
    }
    
}
