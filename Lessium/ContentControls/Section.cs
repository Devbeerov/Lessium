using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Lessium.ContentControls.Models;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using Lessium.Interfaces;
using System.Threading.Tasks;
using Lessium.Utility;
using System.Threading;

namespace Lessium.ContentControls
{
    [Serializable]
    public class Section : StackPanel, ISerializable, ILsnSerializable
    {
        public const double PageWidth = 795;
        public const double PageHeight = 637;

        private bool editable = false;
        private SectionType sectionType;

        private readonly ObservableCollection<ContentPage> pages = new ObservableCollection<ContentPage>();

        // Serialization

        private List<ContentPage> storedPages;

        [Obsolete("Used only in XAML (constructor without parameters). Please use another constructor.", true)]
        public Section() : base()
        {
            Initialize(SectionType.MaterialSection);
        }

        public Section(SectionType type) : base()
        {
            Initialize(type);
        }

        protected Section(SerializationInfo info, StreamingContext context) : base()
        {
            var type = (SectionType) info.GetValue("SectionType", typeof(SectionType));
            var title = info.GetString("Title");
            storedPages = info.GetValue("Pages", typeof(List<ContentPage>)) as List<ContentPage>;

            this.sectionType = type;
            SetTitle(title);

            // Further initialization at OnSerialized method.
            
        }

        #region Public CLR Properties

        public SectionType SectionType { get { return sectionType; } }

        #endregion

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

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            pages.AddRange(storedPages);

            // Initializes with already added pages, so it won't create new page.

            Initialize(sectionType);

            // Clears and sets to null, so GC will collect List instance.

            storedPages.Clear();
            storedPages = null;
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

            if (pages.Count == 0)
            {
                // Section should contain at least 1 page.

                var page = new ContentPage();
                pages.Add(page);
            }

            // Sets Items and pages reference to internal variables.

            SetPages(pages);

            SetSelectedPageIndex(0);
            SetSelectedPage(pages[0]);
            
        }

        public void Add(ContentPage page)
        {
            pages.Add(page);

            // Raising event

            var affectedPages = new Collection<ContentPage>();
            affectedPages.Add(page);

            var args = new PagesChangedEventArgs(affectedPages, CollectionChangeAction.Add);
            PagesChanged?.Invoke(this, args);

            UpdatePageEditable(page);
        }

        public void Remove(ContentPage page)
        {
            pages.Remove(page);

            // Raising event

            var affectedPages = new Collection<ContentPage>();
            affectedPages.Add(page);

            var args = new PagesChangedEventArgs(affectedPages, CollectionChangeAction.Remove);
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

        #region ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Title", GetTitle());
            info.AddValue("Pages", GetPages().ToList());
            info.AddValue("SectionType", sectionType);
        }

        #endregion

        #region ILsnSerializable

        public async Task WriteXmlAsync(XmlWriter writer, CancellationToken? token, IProgress<int> progress = null)
        {
            #region Section

            await writer.WriteStartElementAsync("Section");
            await writer.WriteAttributeStringAsync("Title", GetTitle());

            #region Page(s)

            for (int i = 0; i < pages.Count; i++)
            {
                if(token.HasValue && token.Value.IsCancellationRequested) { break; }

                var page = pages[i];
                await page.WriteXmlAsync(writer, token, progress);
                    
                double calculatedProgress = i * 100d / pages.Count; // Careful with math here
                progress?.Report((int)calculatedProgress); // Casting to integer will "floor" calculatedProgress
            }


            #endregion

            await writer.WriteEndElementAsync();

            #endregion
        }

        public async Task ReadXmlAsync(XmlReader reader, CancellationToken? token, IProgress<int> progress = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class PagesChangedEventArgs
    {
        public Collection<ContentPage> Pages { get; private set; }
        public CollectionChangeAction Action { get; private set; }

        public PagesChangedEventArgs(Collection<ContentPage> pages, CollectionChangeAction action)
        {
            Pages = pages;
            Action = action;
        }
    }

    public enum SectionType
    {
        MaterialSection, TestSection
    }
}
