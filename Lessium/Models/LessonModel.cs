using Lessium.ContentControls;
using System;
using System.Collections.ObjectModel;

namespace Lessium.Models
{
    public class LessonModel
    {
        public Collection<Section> MaterialSections { get; private set; } = new Collection<Section>();
        public Collection<Section> TestSections { get; private set; } = new Collection<Section>();

        public Collection<Section> GetSectionsOfType(ContentType type)
        {
            switch (type)
            {
                case ContentType.Material:
                    return MaterialSections;
                case ContentType.Test:
                    return TestSections;
                default: throw new NotSupportedException($"{type} not supported.");
            }
        }
    }
}
