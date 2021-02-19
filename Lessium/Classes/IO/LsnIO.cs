using Lessium.ContentControls;
using System;
using System.Collections.ObjectModel;

namespace Lessium.Classes.IO
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
                default: throw new NotSupportedException($"{type.ToString()} not supported.");
            }
        }
    }

    public enum IOResult
    {
        Null, Error, Sucessful, Cancelled, Timeout
    }

    public enum IOType
    {
        Unknown, Read, Write
    }

    public enum ProgressType
    {
        Tab, Section, Page, Content
    }
}
