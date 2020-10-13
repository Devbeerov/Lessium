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


        /// <summary>
        /// TRY TO AVOID USING THIS METHOD IF POSSIBLE!
        /// Dictionaries are not intented to be used for searching.
        /// However, considering that Dictionary.Keys are small, it's could be used.
        /// Returns -1 if not found.
        /// </summary>
        public static int GetSectionID(this ObservableDictionary<string, Section> sections, Section section)
        {
            var title = section.GetTitle();
            var keys = sections.Keys;

            int index = 0;
            int max = keys.Count;

            // Using enumerator to auto-dispose it.
            using (var enumerator = keys.GetEnumerator())
            {
                enumerator.MoveNext(); //
                while (index < max)
                {
                    if (enumerator.Current.Equals(title))
                    {
                        return index;
                    }
                    enumerator.MoveNext();
                    index++;
                }

                return -1;
            }
        }
    }
}
