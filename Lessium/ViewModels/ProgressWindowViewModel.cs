using Lessium.Classes.IO;
using Lessium.ContentControls;
using Lessium.Properties;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace Lessium.ViewModels
{
    public class ProgressWindowViewModel : BindableBase
    {
        private readonly Dictionary<ContentType, CountData> storedData;
        private ContentType dataType = ContentType.Material;
        private bool firstTabUpdate = true;

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
        public int SectionIndex
        {
            get { return sectionIndex; }
            set
            {
                SetProperty(ref sectionIndex, value);
            }
        }

        private int pageIndex;
        public int PageIndex
        {
            get { return pageIndex; }
            set
            {
                SetProperty(ref pageIndex, value);
            }
        }

        private int contentIndex;
        public int ContentIndex
        {
            get { return contentIndex; }
            set
            {
                SetProperty(ref contentIndex, value);
            }
        }

        #endregion

        #region Constructors

        public ProgressWindowViewModel(Dictionary<ContentType, CountData> countDataDictionary)
        {
            storedData = countDataDictionary;
        }

        #endregion

        #region Methods

        #region Public

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
                    ContentIndex++;
                    break;
                default: throw new NotSupportedException($"{type} not supported (case insensitive).");
            }
        }

        #endregion

        #region Private

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
            // If it's first tab update, we don't increment dataType.
            if (firstTabUpdate) firstTabUpdate = false;
            else dataType++;

            // In case new dataType is already beyond possible ContentType, we just returns.

            if (dataType.IsBeyondMaxValue()) return;

            ClearIndexers();
            UpdateAllCounts();
            UpdateTabTitle(dataType);
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
            if (SectionCount == 0)
            {
                PageCount = 0;
                ContentCount = 0;
                return;
            }

            PageCount = storedData[dataType].GetPagesCount(SectionIndex);
            UpdateContentCount();
        }

        #endregion

        #endregion
    }
}
