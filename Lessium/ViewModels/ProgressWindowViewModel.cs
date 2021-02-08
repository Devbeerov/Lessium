using Lessium.Classes.IO;
using Lessium.Properties;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace Lessium.ViewModels
{
    public class ProgressWindowViewModel : BindableBase
    {
        private CountData storedData;

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

        #endregion

        #region Methods

        #region Public

        public ProgressWindowViewModel()
        {

        }

        /// <summary>
        /// Stores CountData for proper update properties later.
        /// </summary>
        public void SetCountData(CountData data)
        {
            storedData = data;   
        }

        public void UpdateProgress(ProgressType type)
        {
            switch (type)
            {
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

        private void UpdateSection()
        {
            SectionBarValue++;
            PageBarValue = 0;

            UpdatePage();

            PageCount = storedData.PageCount[SectionBarValue - 1]; // by index
        }

        private void UpdatePage()
        {
            PageBarValue++;
            ContentBarValue = 1;

            ContentCount = storedData.ContentCount[PageBarValue - 1]; // by index
        }

        #endregion

        #endregion
    }
}
