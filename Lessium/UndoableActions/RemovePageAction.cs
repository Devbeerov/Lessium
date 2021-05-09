using Lessium.Models;
using Lessium.Interfaces;
using System.Collections.Generic;

namespace Lessium.UndoableActions
{
    public class RemovePageAction : IUndoableAction
    {
        private readonly ICollection<ContentPageModel> pages;
        private readonly ContentPageModel toRemove;

        private ContentPageModel storedPage;

        public RemovePageAction(ICollection<ContentPageModel> pages, ContentPageModel toRemove)
        {
            this.pages = pages;
            this.toRemove = toRemove;
        }

        public void ExecuteDo()
        {
            // Stores reference

            storedPage = toRemove;

            // Removes from pages

            pages.Remove(toRemove);
        }

        public void Undo()
        {
            // Add stored page to pages.

            pages.Add(storedPage);
        }
    }
}
