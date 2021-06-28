using Lessium.Interfaces;
using System;
using System.Collections.Generic;

namespace Lessium.UndoableActions.Generic
{
    public class AddToCollectionAction<T> : IUndoableAction
    {
        public Action Callback { get; set; }
        public Action UndoCallback { get; set; }

        private readonly IList<T> collection;
        private readonly T toAdd;

        public AddToCollectionAction(IList<T> collection, T toAdd)
        {
            this.collection = collection;
            this.toAdd = toAdd;
        }

        public void ExecuteDo()
        {
            collection.Add(toAdd);
            Callback?.Invoke();
        }

        public void Undo()
        {
            collection.Remove(toAdd);
            UndoCallback?.Invoke();
        }
    }
}
