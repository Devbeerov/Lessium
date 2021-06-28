using Lessium.Interfaces;
using Lessium.Models;
using System;

namespace Lessium.UndoableActions
{
    public class RemoveContentAction : IUndoableAction
    {
        public Action Callback { get; set; }
        public Action UndoCallback { get; set; }

        private readonly ContentPageModel page;
        private readonly IContentControl toRemove;

        private IContentControl storedControl;
        private int storedPosition;

        public RemoveContentAction(ContentPageModel page, IContentControl toRemove)
        {
            this.page = page;
            this.toRemove = toRemove;
        }

        public void ExecuteDo()
        {
            // Stores reference and position for undo

            storedControl = toRemove;
            storedPosition = page.Items.IndexOf(toRemove);

            // Removes

            page.Remove(toRemove, true);
            Callback?.Invoke();
        }

        public void Undo()
        {
            page.Insert(storedPosition, storedControl, true);
            UndoCallback?.Invoke();
        }
    }
}
