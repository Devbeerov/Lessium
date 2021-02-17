using Lessium.Classes.IO;
using Lessium.ContentControls;
using Lessium.Properties;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lessium.ViewModels
{
    public class ProgressWindowViewModel : BindableBase
    {
        private readonly Dictionary<ContentType, CountData> storedData;
        private ContentType dataType = ContentType.Material;

        #region CLR Properties

        private string tabType;
        public string TabType
        {
            get { return tabType; }
            set { SetProperty(ref tabType, value); }
        }

        public string SectionsHeader { get; } = Resources.Sections;
        public string PagesHeader { get; } = Resources.Pages;
        public string ContentsHeader { get; } = Resources.Contents;

        private int sectionBarValue;
        public int SectionBarValue
        {
            get { return sectionBarValue; }
            set { SetProperty(ref sectionBarValue, value); }
        }

        private int pageBarValue;
        public int PageBarValue
        {
            get { return pageBarValue; }
            set { SetProperty(ref pageBarValue, value); }
        }

        private int contentBarValue;
        public int ContentBarValue
        {
            get { return contentBarValue; }
            set { SetProperty(ref contentBarValue, value); }
        }

        private int sectionCount;
        public int SectionCount
        {
            get { return sectionCount; }
            set { SetProperty(ref sectionCount, value); }
        }

        private int pageCount;
        public int PageCount
        {
            get { return pageCount; }
            set { SetProperty(ref pageCount, value); }
        }

        private int contentCount;
        public int ContentCount
        {
            get { return contentCount; }
            set { SetProperty(ref contentCount, value); }
        }

        private int sectionIndex;
        private int SectionIndex
        {
            get { return sectionIndex; }
            set
            {
                if (SetProperty(ref sectionIndex, value))
                {
                    PageBarValue = sectionIndex;
                }
            }
        }

        private int pageIndex;
        private int PageIndex
        {
            get { return pageIndex; }
            set
            {
                if (SetProperty(ref pageIndex, value))
                {
                    PageBarValue = pageIndex;
                }
            }
        }

        private int contentIndex;
        private int ContentIndex
        {
            get { return contentIndex; }
            set
            {
                if (SetProperty(ref contentIndex, value))
                {
                    PageBarValue = contentIndex;
                }
            }
        }

        #endregion

        #region Methods

        #region Public

        public ProgressWindowViewModel(Dictionary<ContentType, CountData> countDataDictionary)
        {
            storedData = countDataDictionary;
            UpdateTabTitle(dataType);
            UpdateAllCounts();
        }

        public void UpdateContentType(ContentType type)
        {
            dataType = type;
        }

        public void UpdateProgress(ProgressType type)
        {
            switch (type)
            {
                case ProgressType.Tab:
                    UpdateTab();
                    break;
                case ProgressType.Section:
                    UpdateSection();
                    break;
                case ProgressType.Page:
                    UpdatePage();
                    break;
                case ProgressType.Content:
                    ContentBarValue++;
                    break;
                default: throw new NotSupportedException($"{type.ToString()} not supported (case insensitive).");
            }
        }

        #endregion

        #region Private

        private void ClearBars()
        {
            SectionBarValue = 0;
            PageBarValue = 0;
            ContentBarValue = 0;
        }

        private void ClearIndexers()
        {
            SectionIndex = 0;
            PageIndex = 0;
            ContentIndex = 0;
        }

        private void UpdateTabTitle(ContentType tabType)
        {
            TabType = tabType.ToTabString();
        }

        private void UpdateTab()
        {
            // In case new dataType is already beyond possible ContentType, we just returns.

            if (dataType.IsBeyondMaxValue()) return;

            ClearIndexers();
            UpdateAllCounts();
            UpdateTabTitle(dataType);

            // If next dataType won't be beyond MaxValue

            if (!(dataType + 1).IsBeyondMaxValue())
            {
                dataType++; // Since dataType is enum, we could increment it like integer to move to next dataType.
            }
        }

        private void UpdateSection()
        {
            PageIndex = 0;
            UpdatePageAndContentCount();

            if ((SectionIndex + 1) < SectionCount)
            {
                SectionIndex++;
            }
        }

        private void UpdatePage()
        {
            ContentIndex = 0;
            UpdateContentCount(); // Updates ContentCount of new Page.

            if ((PageIndex + 1) < PageCount)
            {
                PageIndex++;
            }
        }

        private void UpdateAllCounts()
        {
            SectionCount = storedData[dataType].GetSectionsCount();
            UpdatePageAndContentCount();
        }

        private void UpdateContentCount()
        {
            // Uses BarValues as indexers.
            ContentCount = storedData[dataType].GetContentsCount(SectionIndex, PageIndex);

            // We update ContentIndex only if next Content's index will be less than ContentCount

            if ((ContentIndex + 1) < ContentCount)
            {
                ContentIndex++;
            }
        }

        private void UpdatePageAndContentCount()
        {
            PageCount = storedData[dataType].GetPagesCount(SectionIndex);
            UpdateContentCount();
        }

        #endregion

        #endregion
    }
}
