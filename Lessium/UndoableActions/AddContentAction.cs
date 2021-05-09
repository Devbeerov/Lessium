using Lessium.Models;
using Lessium.Interfaces;

namespace Lessium.UndoableActions
{
    public class AddContentAction : IUndoableAction
    {
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
        }

        public void Undo()
        {
            page.Remove(control, true);
        }
    }
}
