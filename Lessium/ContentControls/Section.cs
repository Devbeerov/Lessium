﻿using Lessium.Classes.IO;
using Lessium.Interfaces;
using Lessium.Models;
using Lessium.Properties;
using Lessium.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Lessium.ContentControls
{
    [Serializable]
    public class Section : VirtualizingStackPanel, ILsnSerializable
    {
        public const double PageWidth = 795;
        public const double PageHeight = 637;

        private bool setupDone = false;
        private IDispatcher dispatcher;

        private readonly ObservableCollection<ContentPageModel> pages = new ObservableCollection<ContentPageModel>();

        // Serialization

        private List<ContentPageModel> storedPages;

        [Obsolete("Used only in XAML (constructor without parameters). Please use another constructor.", true)]
        public Section() : base()
        {
            // Dispatcher

            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;

            // Initialization

            SetupProperties(ContentType.Material);
            Initialize();
        }

        public Section(ContentType type, bool initialize = true, IDispatcher dispatcher = null) : base()
        {
            // Dispatcher

            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;

            // Initialization

            SetupProperties(type);

            if (initialize)
            {
                Initialize();
            }
        }

        protected Section(SerializationInfo info, StreamingContext context) : base()
        {
            var type = (ContentType)info.GetValue(nameof(ContentType), typeof(ContentType));
            var title = info.GetString(nameof(Title));
            storedPages = info.GetValue(nameof(Pages), typeof(List<ContentPageModel>)) as List<ContentPageModel>;

            // Dispatcher

            dispatcher = DispatcherUtility.Dispatcher;

            // Setup

            SetupProperties(type);
            Title = title;

            // Further initialization at OnSerialized method.
        }

        #region Public CLR Properties

        public ContentType ContentType { get; set; }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public ObservableCollection<ContentPageModel> Pages
        {
            get { return (ObservableCollection<ContentPageModel>)GetValue(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public ContentPageModel SelectedPage
        {
            get { return (ContentPageModel)GetValue(SelectedPageProperty); }
            set { SetValue(SelectedPageProperty, value); }
        }

        public int SelectedPageIndex
        {
            get { return (int)GetValue(SelectedPageIndexProperty); }
            set { SetValue(SelectedPageIndexProperty, value); }
        }

        #endregion

        #region Methods

        #region Private

        private void SetupProperties(ContentType type)
        {
            if (setupDone) throw new InvalidOperationException("SetupProperties method should be called only once!");

            SetIsVirtualizing(this, true);
            SetVirtualizationMode(this, VirtualizationMode.Recycling);

            ContentType = type;
            Pages = pages;

            setupDone = true;
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

                var page = new ContentPageModel(ContentType);
                pages.Add(page);
            }

            SelectedPage = pages[0];
        }

        public void Add(ContentPageModel page)
        {
            if (page.ContentType != this.ContentType)
            {
                throw new InvalidOperationException
("You can only add pages with same ContentType to Section!");
            }

            pages.Add(page);

            // Raising event

            var affectedPages = new Collection<ContentPageModel>();
            affectedPages.Add(page);

            var args = new PagesChangedEventArgs(affectedPages, CollectionChangeAction.Add);
            PagesChanged?.Invoke(this, args);
        }

        public void Remove(ContentPageModel page)
        {
            pages.Remove(page);

            // Raising event

            var affectedPages = new Collection<ContentPageModel>();
            affectedPages.Add(page);

            var args = new PagesChangedEventArgs(affectedPages, CollectionChangeAction.Remove);
            PagesChanged?.Invoke(this, args);
        }



        #endregion

        #endregion

        #region Events

        public event EventHandler<PagesChangedEventArgs> PagesChanged;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(Section), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty PagesProperty =
            DependencyProperty.Register("Pages", typeof(ObservableCollection<ContentPageModel>),
                typeof(Section), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedPageProperty =
            DependencyProperty.Register("SelectedPage", typeof(ContentPageModel),
                typeof(Section), new PropertyMetadata(null));

        // Won't do anything with Section here. Used in ViewModel to change SelectedPage.
        public static readonly DependencyProperty SelectedPageIndexProperty =
            DependencyProperty.Register("SelectedPageIndex", typeof(int),
                typeof(Section), new PropertyMetadata(0));

        #endregion

        #region ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dispatcher.Invoke(() =>
            {
                info.AddValue(nameof(Title), Title);
                info.AddValue(nameof(Pages), Pages.ToList());
            });

            info.AddValue(nameof(ContentType), ContentType);
        }

        #endregion

        #region ILsnSerializable

        public async Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Reports to process new Section.

            progress.Report(ProgressType.Section);

            #region Section

            await writer.WriteStartElementAsync("Section");

            await dispatcher.InvokeAsync(async () =>
            {
                await writer.WriteAttributeStringAsync("Title", Title);
            });

            #region Page(s)

            for (int i = 0; i < pages.Count; i++)
            {
                if (token.HasValue && token.Value.IsCancellationRequested) { break; }

                var page = pages[i];
                await page.WriteXmlAsync(writer, progress, token);
            }

            #endregion

            await writer.WriteEndElementAsync();

            #endregion


        }

        public async Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token)
        {
            var title = reader.GetAttribute("Title");

            if (title == null)
            {
                throw new ArgumentNullException(title);
            }

            dispatcher.Invoke(() =>
            {
                Title = title;
            });

            // Reports to process new Section.

            progress.Report(ProgressType.Section);

            // After getting all attributes, we can ReadSubtree to read Pages within this Section.

            reader = reader.ReadSubtree();

            // Reads while "Page" elements could be found.
            while (await reader.ReadToFollowingAsync("Page"))
            {
                if (token.HasValue && token.Value.IsCancellationRequested) break;
                if (reader.NodeType != XmlNodeType.Element) continue;

                var page = new ContentPageModel(this.ContentType);

                await page.ReadXmlAsync(reader, progress, token);
                Add(page);

            }

            dispatcher.Invoke(() =>
            {
                if (Pages.Count == 0)
                {
                    throw new InvalidDataException("Section must have at least 1 Page.");
                }
            });
        }

        #endregion
    }

    public class PagesChangedEventArgs
    {
        public Collection<ContentPageModel> Pages { get; private set; }
        public CollectionChangeAction Action { get; private set; }

        public PagesChangedEventArgs(Collection<ContentPageModel> pages, CollectionChangeAction action)
        {
            Pages = pages;
            Action = action;
        }
    }

    public enum ContentType
    {
        Material, Test
    }

    public static class ContentTypeExtensions
    {
        private static readonly ResourceManager manager = new ResourceManager(typeof(Resources));
        private static readonly int maxContentTypeValue = Enum.GetValues(typeof(ContentType)).GetUpperBound(0);

        /// <param name="invariantCulture">
        /// Should be independant culture used or not.
        /// Otherwise uses CurrentCulture.
        /// </param>
        public static string ToTabString(this ContentType type, bool invariantCulture = false)
        {
            CultureInfo culture;
            if (!invariantCulture)
            {
                culture = CultureInfo.CurrentCulture;
            }
            else
            {
                culture = CultureInfo.InvariantCulture;
            }

            switch (type)
            {
                case ContentType.Material:
                    return manager.GetString("Materials", culture);
                case ContentType.Test:
                    return manager.GetString("Tests", culture);
                default: throw new NotSupportedException($"{type.ToString()} is not supported.");
            }
        }

        public static bool IsBeyondMaxValue(this ContentType type)
        {
            return (int)type > maxContentTypeValue;
        }
    }
}
