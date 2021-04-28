using Lessium.ContentControls.Models;
using Lessium.Interfaces;

namespace Lessium.UndoableActions
{
    public class AddContentAction : IUndoableAction
    {
        private readonly ContentPage page;
        private readonly IContentControl control;

        public AddContentAction(ContentPage page, IContentControl control)
        {
            this.page = page;
            this.control = control;
        }

        public void ExecuteDo()
        {
            page.Add(control);
        }

        public void Undo()
        {
            page.Remove(control);
        }
    }
}
