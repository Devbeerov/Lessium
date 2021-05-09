using System.Collections.Generic;

namespace Lessium.Classes.IO
{
    /// <summary>
    /// Contains all required data for ProgressWindowViewModel. 
    /// NOTE: Should be class instead of struct, because it contains dictionaries.
    /// </summary>
    public class CountData
    {
        public int GetSectionsCount()
        {
            return data.Keys.Count;
        }

        public int GetPagesCount(int sectionIndex)
        {
            return data[sectionIndex].Keys.Count;
        }

        public int GetContentsCount(int sectionIndex, int pageIndex)
        {
            return data[sectionIndex][pageIndex];
        }

        public void AddSection(int sectionIndex)
        {
            data.Add(sectionIndex, new Dictionary<int, int>());
        }

        public void AddPage(int sectionIndex, int pageIndex, int contentAmount)
        {
            data[sectionIndex].Add(pageIndex, contentAmount);
        }

        public override bool Equals(object obj)
        {
            var otherData = obj as CountData;

            if (otherData == null)
            {
                return false;
            }

            var sectionsCount = this.GetSectionsCount();
            var otherSectionsCount = otherData.GetSectionsCount();

            if (sectionsCount != otherSectionsCount) return false;

            for (int i = 0; i < sectionsCount; i++)
            {
                var pageCount = this.GetPagesCount(i);
                var otherPageCount = otherData.GetPagesCount(i);

                if (pageCount != otherPageCount) return false;

                for (int j = 0; j < pageCount; j++)
                {
                    var contentCount = this.GetContentsCount(i, j);
                    var otherContentCount = otherData.GetContentsCount(i, j);

                    if (contentCount != otherContentCount) { return false; }
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var sectionsCount = GetSectionsCount();
            var hash = sectionsCount;

            for (int i = 0; i < sectionsCount; i++)
            {
                var pageCount = this.GetPagesCount(i);
                hash = hash * 17 + pageCount;

                for (int j = 0; j < pageCount; j++)
                {
                    var contentCount = this.GetContentsCount(i, j);
                    hash = hash * 17 + contentCount;
                }
            }

            return hash;
        }


        /// <summary>
        /// (int) index of Section
        /// (Dictionary) Section's content by Page.
        /// 1st - PageIndex
        /// 2nd - Amount of ContentControls which specified Page contains.
        /// </summary>
        private readonly Dictionary<int, Dictionary<int, int>> data = new Dictionary<int, Dictionary<int, int>>();
    }
}
