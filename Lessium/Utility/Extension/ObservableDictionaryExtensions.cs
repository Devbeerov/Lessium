using Lessium.ContentControls;
using System.Collections.ObjectModel;

namespace Lessium.Utility.Extension
{
    public static class ObservableDictionaryExtensions
    {
        public static void AddSection(this ObservableDictionary<string, Section> sections, Section section)
        {
            sections.Add(section.GetTitle(), section);
        }
    }
}
