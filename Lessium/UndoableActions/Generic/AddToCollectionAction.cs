using Lessium.Interfaces;
using System.Collections.Generic;

namespace Lessium.UndoableActions.Generic
{
    public class AddToCollectionAction<T> : IUndoableAction
    {
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
        }

        public void Undo()
        {
            collection.Remove(toAdd);
        }
    }
}
