using Lessium.Models;
using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lessium.ContentControls
{
    // All ContentControls should have parent of type ContentPageControl.
    public class ContentPageControl : WrapPanel
    {
        private ContentPageModel contentPage;

        [Obsolete("You should not manually create ContentPageControl. This constructor used for creating control in XAML.", true)]
        public ContentPageControl() : base()
        {
            Initialize();
        }

        #region Public CLR Properties

        public ObservableCollection<IContentControl> Items
        {
            get { return (ObservableCollection<IContentControl>) GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            // Default properties

            MaxWidth = ContentPageModel.PageWidth;
            Width = MaxWidth;

            Orientation = Orientation.Vertical;

            // Events

            DataContextChanged += OnDataContextChanged;
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // If MaxHeight property will be modified before loading - it could make binding problems.
            // It's can be fixed easily, just adding binding after loading!

            var binding = new Binding(nameof(MaxHeight));
            binding.Source = this;
            binding.FallbackValue = ContentPageModel.PageHeight;

            SetBinding(HeightProperty, binding);
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
            contentPage.SetMaxWidth(newSize.Width);
            contentPage.SetMaxHeight(newSize.Height);
        }

        #endregion

        #endregion

        #region Events

        // DataContext = CurrentPage
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            contentPage = e.NewValue as ContentPageModel;

            if (contentPage == null) { return; } // Wrong DataContext (probably MainWindowViewModel)

            // We set PageControl of model here and keep it for later. 
            // Therefore, we could check IsContentFit even from older model.

            contentPage.SetPageControl(this);

            // Items

            Items = contentPage.Items;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (contentPage != null)
            {
                var newSize = new Size(e.NewSize.Width, e.NewSize.Height);

                UpdateModelMaxSize(newSize);
            }
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
            typeof(ContentPageControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion
     }
}
