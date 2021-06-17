using Lessium.ContentControls.MaterialControls;
using Lessium.Converters;
using Lessium.Services;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lessium.CustomControls
{
    /// <summary>
    /// Manages Items ContentPresenters MaxHeight property to ensure that everything will have proper.
    /// Have built-in support for TextControl.
    /// However, each Item that want this behavior to work, should bind it's MaxHeight property to ContentPresenter.
    /// Example: {Binding RelativeSource={RelativeSource Mode=TemplatedParent}, Path=MaxHeight}
    /// </summary>
    public partial class PageItemsControl : UserControl
    {
        private static UIElementsDistanceConverter distanceConverter;

        public PageItemsControl()
        {
            WeakEventManager<PageItemsControl, RoutedEventArgs>.AddHandler(this, nameof(Loaded), OnLoaded);
            InitializeComponent();
        }

        #region Methods

        private UIElementsDistanceConverter FindDistanceConverter()
        {
            var converter = (UIElementsDistanceConverter)Application.Current.FindResource(nameof(UIElementsDistanceConverter));

            if (converter == null) throw new KeyNotFoundException("Failed to find converter.");

            return converter;
        }

        private double CalculateItemMaxHeight(FrameworkElement item, FrameworkElement nextItem)
        {
            if (distanceConverter == null) distanceConverter = FindDistanceConverter();

            double maxItemHeight = MaxHeight;

            var inputToItemsControl = new object[] { this, item };
            var distanceToItemsControl = (double)distanceConverter.Convert(inputToItemsControl, null, Coordinate.Y.ToString(), CultureInfo.InvariantCulture);

            maxItemHeight -= distanceToItemsControl;

            if (nextItem != null)
            {
                var inputToNextItem = new object[] { item, nextItem };
                var distanceToNextItem = (double)distanceConverter.Convert(inputToNextItem, null, Coordinate.Y.ToString(), CultureInfo.InvariantCulture);

                maxItemHeight -= distanceToNextItem;
            }

            return maxItemHeight;
        }

        private void UpdateItemsMaxHeight()
        {
            // Backwards
            for (int i = innerItemsControl.Items.Count - 1; i >= 0; i--)
            {
                var itemPresenter = (ContentPresenter)innerItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                ContentPresenter nextItemPresenter = null;

                if (i + 1 != innerItemsControl.Items.Count)
                    nextItemPresenter = (ContentPresenter)innerItemsControl.ItemContainerGenerator.ContainerFromIndex(i + 1);

                var maxHeight = CalculateItemMaxHeight(itemPresenter, nextItemPresenter);

                itemPresenter.MaxHeight = maxHeight;
            }
        }

        #endregion

        #region Events

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            WeakEventManager<PageItemsControl, RoutedEventArgs>.RemoveHandler(this, nameof(Loaded), OnLoaded);

            UpdateItemsMaxHeight();
        }

        private void innerItemsControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemsMaxHeight();
        }

        #endregion

        #region Dependency Properties

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(PageItemsControl), new PropertyMetadata(false,
                new PropertyChangedCallback(OnEditableChanged)));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PageItemsControl), new PropertyMetadata(null));

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(PageItemsControl), new PropertyMetadata(null));

        private static void OnEditableChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var pageItemControl = sender as PageItemsControl;
            var innerItemsControl = pageItemControl.innerItemsControl;

            for (int i = 0; i < innerItemsControl.Items.Count; i++)
            {
                // Should be used ContainerFromIndex instead of ContainerFromItem,
                // Because it will return same ContentPresenter for each element due to overriden AnswerMode.Equals (perhaps GetHashCode involved too)
                var contentPresenter = innerItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (contentPresenter != null) // If container is already loaded, otherwise we wait for TextContainer_Loaded method
                {
                    var dataTemplate = contentPresenter.ContentTemplate;
                    var textContainer = dataTemplate.FindName("TextContainer", contentPresenter) as TextControl;

                    // Support for TextContainer
                    if (textContainer != null) textContainer.IsEditable = pageItemControl.IsEditable;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// It's just very simple implementation of methods, that are should not be directly presented in PageItemsControl.
    /// Because it's responsibility just to manage Items properly to fit the Page.
    /// Also possible to make PageItemsControl abstract, but it's not needed for now.
    /// </summary>
    public static class PageItemControlExtensions
    {
        /// <summary>
        /// Updates all DynamicCheckBox that are Items of PageItemsControl.
        /// </summary>
        /// <param name="checkBoxTypes"></param>
        public static void UpdateCheckboxes(this PageItemsControl control, List<DynamicCheckBoxType> checkBoxTypes)
        {
            var innerItemsControl = control.innerItemsControl;

            foreach (var item in innerItemsControl.Items)
            {
                var containerPresenter = innerItemsControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                var contentTemplate = containerPresenter.ContentTemplate;
                var checkBox = contentTemplate.FindName("checkBox", containerPresenter) as DynamicCheckBox;

                if (checkBox == null) continue;

                foreach (var checkBoxType in checkBoxTypes)
                {
                    checkBox.UpdateIsChecked(true, checkBoxType);
                }
            }
        }
    }
}
