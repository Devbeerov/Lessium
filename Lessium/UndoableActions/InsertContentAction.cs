using Lessium.Models;
using Lessium.Interfaces;
using System;

namespace Lessium.UndoableActions
{
    public class InsertContentAction : IUndoableAction
    {
        public Action Callback { get; set; }
        public Action UndoCallback { get; set; }

        private readonly ContentPageModel page;
        private readonly IContentControl control;
        private readonly int position;

        public InsertContentAction(ContentPageModel page, IContentControl control, int position)
        {
            this.page = page;
            this.control = control;
            this.position = position;
        }

        public void ExecuteDo()
        {
            page.Insert(position, control, true);
            Callback?.Invoke();
        }

        public void Undo()
        {
            page.Remove(control, true);
            UndoCallback?.Invoke();
        }
    }
}
