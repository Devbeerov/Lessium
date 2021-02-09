﻿using Lessium.Classes.IO;
using Lessium.ContentControls;
using Lessium.Properties;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace Lessium.ViewModels
{
    public class ProgressWindowViewModel : BindableBase
    {
        private Dictionary<ContentType, CountData> storedData;
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

        #endregion

        #region Methods

        #region Public

        public ProgressWindowViewModel(Dictionary<ContentType, CountData> countDataDictionary)
        {
            storedData = countDataDictionary;
            UpdateTabTitle(dataType);
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

        // Clears (sets to 0) properties related to tab.
        private void ClearProperties()
        {
            // Bars values

            SectionBarValue = 0;
            PageBarValue = 0;
            ContentBarValue = 0;

            // Count values

            SectionCount = 0;
            PageCount = 0;
            ContentCount = 0;
        }

        private void UpdateTabTitle(ContentType tabType)
        {
            string newTitle;

            switch (tabType)
            {
                case ContentType.Material:
                    newTitle = Resources.Materials;
                    break;
                case ContentType.Test:
                    newTitle = Resources.Tests;
                    break;
                default: throw new NotSupportedException($"{tabType.ToString()} is not supported.");
            }

            TabType = newTitle;
        }

        private void UpdateTab()
        {
            dataType++; // Since dataType is enum, we could increment it like integer to move to next dataType.

            // In case new dataType is already beyond possible ContentType, we just returns.
            if((int)dataType > Enum.GetValues(typeof(ContentType)).GetUpperBound(0)) return;

            ClearProperties();
            UpdateTabTitle(dataType);
        }

        private void UpdateSection()
        {
            PageBarValue = 0;

            UpdatePage();

            PageCount = storedData[dataType].PageCount[SectionBarValue];
            SectionBarValue++;
        }

        private void UpdatePage()
        {
            ContentBarValue = 0;

            ContentCount = storedData[dataType].ContentCount[PageBarValue];

            PageBarValue++;
        }

        #endregion

        #endregion
    }
}
