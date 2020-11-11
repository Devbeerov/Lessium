using Lessium.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace Lessium.ContentControls
{
    public class Section : StackPanel
    {
        private bool editable = false;
        private SectionType sectionType;

        public const double PageWidth = 795;
        public const double PageHeight = 637;

        [Obsolete("Used only in XAML (constructor without parameters). Please use another constructor.", true)]
        public Section() : base()
        {
            Initialize(SectionType.MaterialSection);
        }

        public Section(SectionType type) : base()
        {
            Initialize(type);
        }

        // Custom Serialization
        public Section(SectionSerializationInfo info) : base()
        {
            Initialize(info.sectionType);

            SetTitle(info.title);
            SetEditable(info.editable);
            pages.AddRange(info.pages);
        }

        private readonly ObservableCollection<ContentPage> pages = new ObservableCollection<ContentPage>();

        #region Dependency Properties Methods

        #region Title

        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(Title);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(Title, value);
        }

        public string GetTitle()
        {
            return GetTitle(this);
        }

        public void SetTitle(string title)
        {
            SetTitle(this, title);
        }

        #endregion

        #region Pages

        public static ObservableCollection<ContentPage> GetPages(DependencyObject obj)
        {
            return (ObservableCollection<ContentPage>)obj.GetValue(Pages);
        }

        protected static void SetPages(DependencyObject obj, ObservableCollection<ContentPage> pages)
        {
            obj.SetValue(Pages, pages);
        }

        public ObservableCollection<ContentPage> GetPages()
        {
            return GetPages(this);
        }

        protected void SetPages(ObservableCollection<ContentPage> pages)
        {
            SetPages(this, pages);
        }

        #endregion

        #endregion

        #region Methods

        #region Private

        private void UpdatePageEditable(ContentPage page)
        {
            if (page != null)
            {
                page.SetEditable(editable);
            }
        }

        private void UpdatePagesEditable()
        {
            foreach (var page in pages)
            {
                UpdatePageEditable(page);
            }
        }

        #endregion

        #region Public

        public void Initialize(SectionType sectionType)
        {
            // Internal

            this.sectionType = sectionType;

            // Visible

            Width = double.NaN;
            Height = double.NaN;

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;

            // Section should contain at least 1 page.

            pages.Add(new ContentPage());

            // Sets Items and pages reference to internal variables.

            SetPages(pages);
        }

        public void Add(ContentPage page)
        {
            pages.Add(page);
            UpdatePageEditable(page);
        }

        public void Remove(ContentPage page)
        {
            pages.Remove(page);
        }

        public bool GetEditable()
        {
            return editable;
        }

        public void SetEditable(bool editable)
        {
            this.editable = editable;

            UpdatePagesEditable();
        }

        public SectionSerializationInfo GetSerializationInfo()
        {
            SectionSerializationInfo info = new SectionSerializationInfo();

            info.title = GetTitle();
            info.editable = editable;
            info.pages = pages.ToList(); // Linq does copying
            info.sectionType = sectionType;

            return info;
        }

        #endregion

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty Title =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(Section), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty Pages =
            DependencyProperty.Register("Pages", typeof(ObservableCollection<ContentPage>),
                typeof(ContentPage), new PropertyMetadata(null));

        #endregion

    }

    [Serializable]
    public class SectionSerializationInfo
    {
        public string title;
        public bool editable;
        public List<ContentPage> pages; // COPY of List of ObservableCollection!
        public SectionType sectionType;
    }

    public enum SectionType
    {
        MaterialSection, TestSection
    }
}
