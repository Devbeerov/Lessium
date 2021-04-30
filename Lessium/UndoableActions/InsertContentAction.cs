using Lessium.ContentControls.Models;
using Lessium.Interfaces;

namespace Lessium.UndoableActions
{
    public class InsertContentAction : IUndoableAction
    {
        private readonly ContentPage page;
        private readonly IContentControl control;
        private readonly int position;

        public InsertContentAction(ContentPage page, IContentControl control, int position)
        {
            this.page = page;
            this.control = control;
            this.position = position;
        }

        public void ExecuteDo()
        {
            page.Insert(position, control, true);
        }

        public void Undo()
        {
            page.Remove(control, true);
        }
    }
}
