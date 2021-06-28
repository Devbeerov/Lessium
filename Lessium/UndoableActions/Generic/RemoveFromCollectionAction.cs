using Lessium.Interfaces;
using System;
using System.Collections.Generic;

namespace Lessium.UndoableActions.Generic
{

    public class RemoveFromCollectionAction<T> : IUndoableAction
    {
        public Action Callback { get; set; }
        public Action UndoCallback { get; set; }

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

            Callback?.Invoke();
        }

        public void Undo()
        {
            list.Insert(storedPosition, storedObject);
            UndoCallback?.Invoke();
        }
    }

}
