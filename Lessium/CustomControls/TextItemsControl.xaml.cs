using Lessium.ContentControls.MaterialControls;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.CustomControls
{
    /// <summary>
    /// Manages Items ContentPresenters MaxHeight property to ensure that everything will have proper.
    /// Have built-in support for TextControl.
    /// However, each Item that want this behavior to work, should bind it's MaxHeight property to ContentPresenter.
    /// Example: {Binding RelativeSource={RelativeSource Mode=TemplatedParent}, Path=MaxHeight}
    /// </summary>
    public partial class TextItemsControl : UserControl
    {
        public TextItemsControl()
        {
            InitializeComponent();
        }

        #region Methods

        #region Public

        public void UpdateCheckboxes(List<DynamicCheckBoxType> checkBoxTypes)
        {
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

        #endregion

        #region Private

        private void UpdateTextChangedHandlers(object oldSource, object newSource)
        {
            var newEnumerable = newSource as IEnumerable;
            var oldEnumerable = oldSource as IEnumerable;

            if (newEnumerable != null) SubscribeItemsTextChanged(newEnumerable);
            if (oldEnumerable != null) SubscribeItemsTextChanged(oldEnumerable, false);
        }

        #endregion

        #endregion

        #region Events

        private void OnTextChanged(object sender, TextChangedEventArgs args)
        {

        }

        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            UpdateTextChangedHandlers(args.OldItems, args.NewItems);
        }

        private void SubscribeItemsTextChanged(IEnumerable enumerable, bool subscribe = true)
        {
            foreach (var item in enumerable)
            {
                var textControl = item as TextControl;

                // Uses break instead of continue, because if first item is not TextControl, other will not as well.

                if (textControl == null) break;

                if (subscribe) 
                    WeakEventManager<TextBox, TextChangedEventArgs>.AddHandler(textControl.textBox, nameof(TextBox.TextChanged), OnTextChanged);

                else
                    WeakEventManager<TextBox, TextChangedEventArgs>.RemoveHandler(textControl.textBox, nameof(TextBox.TextChanged), OnTextChanged);
            }
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
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(TextItemsControl), new PropertyMetadata(false,
                new PropertyChangedCallback(OnEditableChanged)));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(TextItemsControl), new PropertyMetadata(null,
                new PropertyChangedCallback(OnItemsSourceChanged)));

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(TextItemsControl), new PropertyMetadata(null));

        private static void OnEditableChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var pageItemControl = sender as TextItemsControl;
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

        private static void OnItemsSourceChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as TextItemsControl;

            var oldNotifyCollection = args.OldValue as INotifyCollectionChanged;
            var newNotifyCollection = args.NewValue as INotifyCollectionChanged;

            control.UpdateTextChangedHandlers(args.OldValue, args.NewValue);

            if (oldNotifyCollection != null)
            {
                CollectionChangedEventManager.RemoveHandler(oldNotifyCollection, control.OnItemsCollectionChanged);
            }

            if (newNotifyCollection != null)
            {
                CollectionChangedEventManager.AddHandler(newNotifyCollection, control.OnItemsCollectionChanged);
            }
        }

        #endregion
    }
}
