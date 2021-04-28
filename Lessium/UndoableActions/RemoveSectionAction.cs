using Lessium.ContentControls;
using Lessium.Interfaces;
using System.Collections.Generic;

namespace Lessium.UndoableActions
{
    public class RemoveSectionAction : IUndoableAction
    {
        private readonly ICollection<Section> sections;
        private readonly Section toRemove;

        private Section storedSection;

        public RemoveSectionAction(ICollection<Section> sections, Section toRemove)
        {
            this.sections = sections;
            this.toRemove = toRemove;
        }

        public void ExecuteDo()
        {
            // Stores reference

            storedSection = toRemove;

            // Removes from sections

            sections.Remove(toRemove);
        }

        public void Undo()
        {
            // Add stored section to sections.

            sections.Add(storedSection);
        }
    }
}
