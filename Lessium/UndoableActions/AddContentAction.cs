using Lessium.Interfaces;
using Lessium.Models;
using System;

namespace Lessium.UndoableActions
{
    public class AddContentAction : IUndoableAction
    {
        public Action Callback { get; set; }
        public Action UndoCallback { get; set; }

        private readonly ContentPageModel page;
        private readonly IContentControl control;

        public AddContentAction(ContentPageModel page, IContentControl control)
        {
            this.page = page;
            this.control = control;
        }

        public void ExecuteDo()
        {
            page.Add(control, true);
            Callback?.Invoke();
        }

        public void Undo()
        {
            page.Remove(control, true);
            UndoCallback?.Invoke();
        }
    }
}
