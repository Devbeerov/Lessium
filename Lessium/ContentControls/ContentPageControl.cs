using Lessium.ContentControls.Models;
using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lessium.ContentControls
{
    // All ContentControls should have parent of type ContentPageCotrol.
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
            return (ObservableCollection<IContentControl>)obj.GetValue(ItemsProperty);
        }

        protected static void SetItems(DependencyObject obj, ObservableCollection<IContentControl> items)
        {
            obj.SetValue(ItemsProperty, items);
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
            // Default properties

            MaxWidth = ContentPage.PageWidth;
            Width = MaxWidth;

            Orientation = Orientation.Vertical;

            // Binding

            var binding = new Binding(nameof(MaxHeight));
            binding.Source = this;
            binding.FallbackValue = ContentPage.PageHeight;

            SetBinding(HeightProperty, binding);

            // Events

            DataContextChanged += OnDataContextChanged;
            SizeChanged += OnSizeChanged;
        }

        public bool IsElementFits(FrameworkElement element)
        {
            var pos = element.TranslatePoint(default(Point), this);
            var fits = pos.Y + element.ActualHeight <= ActualHeight;
            return fits;
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

        // DataContext = CurrentPage
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            model = e.NewValue as ContentPage;

            if(model == null) { return; } // Wrong DataContext (MainWindowViewModel)

            // We set PageControl of model here and keep it for later. 
            // Therefore, we could check IsContentFit even from older model.

            model.SetPageControl(this);

            // Items

            SetItems(model.Items);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (model != null)
            {
                var newSize = new Size(e.NewSize.Width, e.NewSize.Height);

                UpdateModelMaxSize(newSize);
            }
        }

        #endregion

        #region Dependency Properties

        public ObservableCollection<IContentControl> Items
        {
            get { return GetItems(); }
            set { SetItems(value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
            typeof(ContentPageControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion
     }
}
