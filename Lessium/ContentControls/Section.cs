using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using Lessium.ContentControls.Models;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;
using Lessium.Interfaces;
using System.Threading.Tasks;
using Lessium.Utility;
using System.Threading;
using System.IO;
using Lessium.Classes.IO;

namespace Lessium.ContentControls
{
    [Serializable]
    public class Section : StackPanel, ISerializable, ILsnSerializable
    {
        public const double PageWidth = 795;
        public const double PageHeight = 637;

        private bool editable = false;

        private readonly ObservableCollection<ContentPage> pages = new ObservableCollection<ContentPage>();

        // Serialization

        private List<ContentPage> storedPages;

        [Obsolete("Used only in XAML (constructor without parameters). Please use another constructor.", true)]
        public Section() : base()
        {
            ContentType = ContentType.Material;
            SetPages(pages);
            Initialize();
        }

        public Section(ContentType type, bool initialize = true) : base()
        {
            ContentType = type;
            SetPages(pages);
            if (initialize)
            {
                Initialize();
            }
        }

        protected Section(SerializationInfo info, StreamingContext context) : base()
        {
            var type = (ContentType) info.GetValue("ContentType", typeof(ContentType));
            var title = info.GetString("Title");
            storedPages = info.GetValue("Pages", typeof(List<ContentPage>)) as List<ContentPage>;

            ContentType = type;
            SetTitle(title);

            // Further initialization at OnSerialized method.
        }

        #region Public CLR Properties

        public ContentType ContentType { get; set; }

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

            Initialize();

            // Clears and sets to null, so GC will collect List instance.

            storedPages.Clear();
            storedPages = null;
        }

        #endregion

        #region Public

        public void Initialize()
        {
            // Visible

            Width = double.NaN;
            Height = double.NaN;

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;

            if (pages.Count == 0)
            {
                // Section should contain at least 1 page.

                var page = new ContentPage(ContentType);
                pages.Add(page);
            }

            SetSelectedPage(pages[0]);
        }

        public void Add(ContentPage page)
        {
            if(page.ContentType != this.ContentType) { throw new InvalidOperationException
                    ("You can only add pages with same ContentType to Section!"); }

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
                typeof(Section), new PropertyMetadata(0));

        #endregion

        #region ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Title", GetTitle());
            info.AddValue("Pages", GetPages().ToList());
            info.AddValue("ContentType", ContentType);
        }

        #endregion

        #region ILsnSerializable

        public async Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token)
        {
            #region Section

            await writer.WriteStartElementAsync("Section");
            await writer.WriteAttributeStringAsync("Title", GetTitle());

            #region Page(s)

            for (int i = 0; i < pages.Count; i++)
            {
                if(token.HasValue && token.Value.IsCancellationRequested) { break; }

                var page = pages[i];
                await page.WriteXmlAsync(writer, progress, token);
            }

            #endregion

            await writer.WriteEndElementAsync();

            #endregion

            // Reports progress.

            progress.Report(ProgressType.Section);
        }

        public async Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token)
        {
            var title = reader.GetAttribute("Title");

            if (title == null)
            {
                throw new ArgumentNullException(title);
            }

            SetTitle(title);

            while (await reader.ReadAsync())
            {
                token?.ThrowIfCancellationRequested();

                // Read until getting to Page element.
                if (await reader.ReadToFollowingAsync("Page"))
                {
                    var page = new ContentPage(this.ContentType);

                    await page.ReadXmlAsync(reader, progress, token);
                    Add(page);

                    // Reports progress.

                    progress.Report(ProgressType.Section);
                }
            }

            if (GetPages().Count == 0)
            {
                throw new InvalidDataException("Section must have at least 1 Page. Something is wrong with Section.");
            }
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

    public enum ContentType
    {
        Material, Test
    }
}
