using Lessium.ContentControls.Models;
using Lessium.Interfaces;
using System.Collections.Generic;

namespace Lessium.UndoableActions
{
    public class RemovePageAction : IUndoableAction
    {
        private readonly ICollection<ContentPage> pages;
        private readonly ContentPage toRemove;

        private ContentPage storedPage;

        public RemovePageAction(ICollection<ContentPage> pages, ContentPage toRemove)
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
