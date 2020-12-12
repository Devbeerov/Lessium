using Lessium.ContentControls.Models;
using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
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

        #endregion

        #endregion

        #region Events

        private void OnModelControlResized(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                var controlElement = sender as FrameworkElement;
                var controlLocation = controlElement.TranslatePoint(new Point(), this);

                //var controlRect = controlElement.TransformToAncestor(this).TransformBounds(
                //    new Rect(controlLocation, controlElement.RenderSize));

                //var rect = LayoutTransform.TransformBounds(new Rect(0, 0, ActualWidth, ActualHeight));

                //var actualheight = controlElement.ActualHeight;

                if (controlElement.ActualHeight + controlLocation.Y > this.ActualHeight)
                {
                    Console.WriteLine("Should go to next page");
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
            var offset = new Point(GetOffsetX(), GetOffsetY());
            var newSize = new Size(e.NewSize.Width, e.NewSize.Height);

            newSize.Width -= offset.X;
            newSize.Height -= offset.Y;

            UpdateModelMaxSize(newSize);
        }
        
        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty Items =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
            typeof(ContentPageControl), new PropertyMetadata(null));

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

        #endregion

        #endregion
    }
}
