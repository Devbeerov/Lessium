using Lessium.ContentControls.Models;
using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lessium.ContentControls
{
    public class ContentPageControl : WrapPanel
    {
        private ContentPage model;

        [Obsolete("This constructor used for creating control in XAML.", true)]
        public ContentPageControl() : base()
        {
            Initialize();
        }

        #region Dependency Properties Methods

        #region Items

        public static ObservableCollection<IContentControl> GetItems(DependencyObject obj)
        {
            return (ObservableCollection<IContentControl>)obj.GetValue(Items);
        }

        protected static void SetItems(DependencyObject obj, ObservableCollection<IContentControl> items)
        {
            obj.SetValue(Items, items);
        }

        public ObservableCollection<IContentControl> GetItems()
        {
            return GetItems(this);
        }

        protected void SetItems(ObservableCollection<IContentControl> items)
        {
            SetItems(this, items);
        }

        #endregion

        #region OffsetX

        public static double GetOffsetX(DependencyObject obj)
        {
            return (double)obj.GetValue(OffsetXProperty);
        }

        public static void SetOffsetX(DependencyObject obj, double offset)
        {
            obj.SetValue(OffsetXProperty, offset);
        }

        public double GetOffsetX()
        {
            return GetOffsetX(this);
        }

        public void SetOffsetX(double offset)
        {
            SetOffsetX(this, offset);
        }

        #endregion

        #region OffsetY

        public static double GetOffsetY(DependencyObject obj)
        {
            return (double)obj.GetValue(OffsetYProperty);
        }

        public static void SetOffsetY(DependencyObject obj, double offset)
        {
            obj.SetValue(OffsetYProperty, offset);
        }

        public double GetOffsetY()
        {
            return GetOffsetY(this);
        }

        public void SetOffsetY(double offset)
        {
            SetOffsetY(this, offset);
        }

        #endregion

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            // Visual

            MaxWidth = ContentPage.PageWidth;
            MaxHeight = ContentPage.PageHeight;

            Width = ContentPage.PageWidth;
            Height = ContentPage.PageHeight;

            Orientation = Orientation.Vertical;

            // Events

            DataContextChanged += OnDataContextChanged;
            SizeChanged += OnSizeChanged;
        }

        #endregion

        #region Private

        private void UpdateModelMaxSize(Size newSize)
        {
            model.SetMaxWidth(newSize.Width);
            model.SetMaxHeight(newSize.Height);
        }

        /// <summary>
        /// Checks if item position is fits to ContentPageControl.
        /// If it's not valid, throws event for further handling.
        /// </summary>
        /// <param name="item">Element to check</param>
        private void ValidateContentPlacement(FrameworkElement item)
        {
            var controlLocation = item.TranslatePoint(new Point(), this);

            if (item.ActualHeight + controlLocation.Y > this.ActualHeight - GetOffsetY())
            {
                Console.WriteLine("Should go to next page");
            }
        }

        #endregion

        #endregion

        #region Events

        private void OnModelControlResized(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                var controlElement = sender as FrameworkElement;

                ValidateContentPlacement(controlElement);
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            model = e.NewValue as ContentPage;

            // Items

            SetItems(model.Items);

            // Events

            model.ContentResized += OnModelControlResized;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var offset = new Point(GetOffsetX(), GetOffsetY());
            var newSize = new Size(e.NewSize.Width, e.NewSize.Height);

            newSize.Width -= offset.X;
            newSize.Height -= offset.Y;

            UpdateModelMaxSize(newSize);
        }

        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var newItems = e.NewItems;

            if(newItems == null) { return; }

            var items = sender as ObservableCollection<IContentControl>;

            foreach(FrameworkElement item in newItems)
            {
                if (item.IsLoaded)
                {
                    ValidateContentPlacement(item);
                }

                else
                {
                    // Wait for new added item to initialize for further check.

                    item.Loaded += OnItemLoaded;
                }
            }

        }

        private void OnItemLoaded(object sender, EventArgs e)
        {
            var item = sender as FrameworkElement;

            // Validates placement

            ValidateContentPlacement(item);

            // Unsubscribes

            item.Loaded -= OnItemLoaded;
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty Items =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
            typeof(ContentPageControl), new PropertyMetadata(null, ItemsSourceChangedCallback));

        public static readonly DependencyProperty OffsetXProperty =
            DependencyProperty.Register("OffsetX", typeof(double),
            typeof(ContentPageControl), new PropertyMetadata(0d, OffsetXChangedCallback));

        public static readonly DependencyProperty OffsetYProperty =
            DependencyProperty.Register("OffsetY", typeof(double),
            typeof(ContentPageControl), new PropertyMetadata(0d, OffsetYChangedCallback));

        #region Callbacks

        private static void OffsetXChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var pageControl = dependencyObject as ContentPageControl;

            var newOffset = new Point((double)args.NewValue, pageControl.GetOffsetY());
            var newSize = new Size(pageControl.ActualWidth, pageControl.ActualHeight);

            newSize.Width -= newOffset.X;
            newSize.Height -= newOffset.Y;

            pageControl.UpdateModelMaxSize(newSize);
        }

        private static void OffsetYChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var pageControl = dependencyObject as ContentPageControl;

            var newOffset = new Point(pageControl.GetOffsetX(), (double)args.NewValue);
            var newSize = new Size(pageControl.ActualWidth, pageControl.ActualHeight);

            newSize.Width -= newOffset.X;
            newSize.Height -= newOffset.Y;

            pageControl.UpdateModelMaxSize(newSize);
        }

        private static void ItemsSourceChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var obj = dependencyObject as ContentPageControl;

            var oldCollection = args.OldValue as ObservableCollection<IContentControl>;
            var newCollectiion = args.NewValue as ObservableCollection<IContentControl>;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= obj.OnItemsChanged;
            }

            if (newCollectiion != null)
            {
                newCollectiion.CollectionChanged += obj.OnItemsChanged;
            }
        }

        #endregion

        #endregion
     }
}
