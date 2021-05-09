using Lessium.Models;
using Lessium.Interfaces;

namespace Lessium.UndoableActions
{
    public class RemoveContentAction : IUndoableAction
    {
        private readonly ContentPageModel page;
        private readonly IContentControl toRemove;

        private IContentControl storedControl;

        public RemoveContentAction(ContentPageModel page, IContentControl toRemove)
        {
            this.page = page;
            this.toRemove = toRemove;
        }

        public void ExecuteDo()
        {
            // Stores reference

            storedControl = toRemove;

            // Removes

            page.Remove(toRemove, true);
        }

        public void Undo()
        {
            page.Add(storedControl, true);
        }
    }
}
