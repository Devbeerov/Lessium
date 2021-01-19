using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Lessium.ContentControls.Models;
using System.ComponentModel;

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

        public static void SetPages(DependencyObject obj, ObservableCollection<ContentPage> pages)
        {
            obj.SetValue(Pages, pages);
        }

        public ObservableCollection<ContentPage> GetPages()
        {
            return GetPages(this);
        }

        public void SetPages(ObservableCollection<ContentPage> pages)
        {
            SetPages(this, pages);
        }

        #endregion

        #region SelectedPage

        public static ContentPage GetSelectedPage(DependencyObject obj)
        {
            return (ContentPage)obj.GetValue(SelectedPage);
        }

        public static void SetSelectedPage(DependencyObject obj, ContentPage page)
        {
            obj.SetValue(SelectedPage, page);
        }

        public ContentPage GetSelectedPage()
        {
            return GetSelectedPage(this);
        }

        public void SetSelectedPage(ContentPage page)
        {
            SetSelectedPage(this, page);
        }

        #endregion

        #region SelectedPageIndex

        public static int GetSelectedPageIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(SelectedPageIndex);
        }

        public static void SetSelectedPageIndex(DependencyObject obj, int index)
        {
            obj.SetValue(SelectedPageIndex, index);
        }

        public int GetSelectedPageIndex()
        {
            return GetSelectedPageIndex(this);
        }

        public void SetSelectedPageIndex(int index)
        {
            SetSelectedPageIndex(this, index);
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

            var page = new ContentPage();
            pages.Add(page);

            // Sets Items and pages reference to internal variables.

            SetPages(pages);

            SetSelectedPageIndex(0);
            SetSelectedPage(page);
            
        }

        public void Add(ContentPage page)
        {
            pages.Add(page);

            // Raising event

            var args = new PagesChangedEventArgs(page, CollectionChangeAction.Add);
            PagesChanged?.Invoke(this, args);

            UpdatePageEditable(page);
        }

        public void Remove(ContentPage page)
        {
            pages.Remove(page);

            // Raising event

            var args = new PagesChangedEventArgs(page, CollectionChangeAction.Remove);
            PagesChanged?.Invoke(this, args);
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
            SectionSerializationInfo info = new SectionSerializationInfo
            {
                title = GetTitle(),
                editable = editable,
                pages = pages.ToList(), // Linq does copying
                sectionType = sectionType
            };

            return info;
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler<PagesChangedEventArgs> PagesChanged;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty Title =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(Section), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty Pages =
            DependencyProperty.Register("Pages", typeof(ObservableCollection<ContentPage>),
                typeof(Section), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedPage =
            DependencyProperty.Register("SelectedPage", typeof(ContentPage),
                typeof(Section), new PropertyMetadata(null));

        // Won't do anything with Section here. Used in ViewModel to change SelectedPage.
        public static readonly DependencyProperty SelectedPageIndex =
            DependencyProperty.Register("SelectedPageIndex", typeof(int),
                typeof(Section), new PropertyMetadata(null));

        #endregion
    }

    public class PagesChangedEventArgs
    {
        public ContentPage Page { get; private set; }
        public CollectionChangeAction Action { get; private set; }

        public PagesChangedEventArgs(ContentPage page, CollectionChangeAction action)
        {
            Page = page;
            Action = action;
        }
    }

    [Serializable]
    public class SectionSerializationInfo
    {
        #pragma warning disable S1104 // Fields should not have public accessibility

        public string title;
        public bool editable;
        public List<ContentPage> pages; // COPY of ObservableCollection as List!

        public SectionType sectionType;

        #pragma warning restore S1104 // Fields should not have public accessibility
    }

    public enum SectionType
    {
        MaterialSection, TestSection
    }
}
