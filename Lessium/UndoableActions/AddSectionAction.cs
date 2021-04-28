using Lessium.ContentControls;
using Lessium.Interfaces;
using System.Collections.Generic;

namespace Lessium.UndoableActions
{
    public class AddSectionAction : IUndoableAction
    {
        private readonly ICollection<Section> sections;
        private readonly Section toAdd;

        public AddSectionAction(ICollection<Section> sections, Section toAdd)
        {
            this.sections = sections;
            this.toAdd = toAdd;
        }

        public void ExecuteDo()
        {
            sections.Add(toAdd);
        }

        public void Undo()
        {
            sections.Remove(toAdd);
        }
    }
}
