using Lessium.ContentControls.Models;
using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Lessium.Utility;

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
            DataContextChanged += OnDataContextChanged;
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

                var controlRect = controlElement.TransformToAncestor(this).TransformBounds(
                    new Rect(controlLocation, controlElement.RenderSize));

                var rect = LayoutTransform.TransformBounds(new Rect(0, 0, ActualWidth, ActualHeight));

                if (controlRect.Bottom > rect.Height)
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

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty Items =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
            typeof(ContentPageControl), new PropertyMetadata(null));

        #endregion
    }
}
