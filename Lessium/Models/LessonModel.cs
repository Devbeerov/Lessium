using Lessium.ContentControls;
using Lessium.Utility;
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

        public override bool Equals(object obj)
        {
            var other = obj as LessonModel;

            if (other == null)
            {
                return false;
            }

            if (!EqualsHelper.AreEqual(MaterialSections, other.MaterialSections)) { return false; }
            if (!EqualsHelper.AreEqual(TestSections, other.TestSections)) { return false; }

            return true;
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException();
        }
    }
}
