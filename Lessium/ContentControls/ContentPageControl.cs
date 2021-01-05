using Lessium.ContentControls.Models;
using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

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
        /// <returns>True if event throwed, otherwise - false.</returns>
        private bool ValidateContentPlacement(IContentControl item)
        {
            var element = item as FrameworkElement;
            var controlLocation = element.TranslatePoint(new Point(), this);

            if (element.ActualHeight + controlLocation.Y > this.ActualHeight - GetOffsetY())
            {
                var args = new ExceedingContentEventArgs()
                {
                    ExceedingItem = item,
                    Page = model
                };

                AddedExceedingContent.Invoke(this, args);

                return true;
            }

            return false;
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler AddedExceedingContent;

        public class ExceedingContentEventArgs : EventArgs
        {
            public ContentPage Page { get; set; }
            public IContentControl ExceedingItem { get; set; }
        }

        private void OnModelControlResized(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                var control = e.Source as IContentControl;

                var collection = model.Items;
                int controlPos = collection.IndexOf(control);

                if (ValidateContentPlacement(control))
                {
                    // 0 1 2 3 4 5 6 7
                    //           | <-
                    // 0 1 2 3 4 5 6
                    // item with index 6 become item with index 5
                    // we checked previous 5, so we check new 5 too

                    // If content gone to the next page, we also check all content from upper bound to previous content position.

                    int pos = collection.Count - 1;

                    for (; pos >= controlPos; pos--)
                    {
                        var item = collection[pos];
                        ValidateContentPlacement(item);
                    }
                }
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
            if (sender == this) { return; } // important

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

            foreach(FrameworkElement item in newItems)
            {
                if (item.IsLoaded)
                {
                    ValidateContentPlacement(item as IContentControl);
                }
                // else it will raise OnModelControlResized method once it will be loaded, which will validate placement.
            }

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
