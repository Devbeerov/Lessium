using Lessium.ContentControls.MaterialControls;
using Lessium.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
        private bool raiseTextChanged = true;

        public TextItemsControl()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            WeakEventManager<ItemContainerGenerator, EventArgs>.AddHandler(
                innerItemsControl.ItemContainerGenerator, nameof(ItemContainerGenerator.StatusChanged), OnGeneratorStatusChanged);
        }

        #region Methods

        #region Public

        public void UpdateCheckboxes<T>(IList<T> answers, List<DynamicCheckBoxType> checkBoxTypes)
        {
            foreach (var item in answers)
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

        private TextControl FindTextContainer(ContentPresenter presenter)
        {
            var dataTemplate = presenter.ContentTemplate;
            var textContainer = dataTemplate.FindName("TextContainer", presenter);

            if (textContainer == null) throw new KeyNotFoundException("\"TextContainer\" name has not been found in template.");

            return textContainer as TextControl;
        }

        private double FindTotalHeightOfLastItem()
        {
            var generator = innerItemsControl.ItemContainerGenerator;
            var lastIndex = innerItemsControl.Items.Count - 1;

            var lastTextControlPresenter = generator.ContainerFromIndex(lastIndex) as ContentPresenter;
            var lastTextControl = FindTextContainer(lastTextControlPresenter);
            var lastItemPoint = lastTextControl.TranslatePoint(default, this);

            return lastItemPoint.Y + lastTextControl.ActualHeight;
        }

        private void UpdateTextWithoutRaise(TextBox textBox, string newText)
        {
            raiseTextChanged = false;

            textBox.Text = newText;

            raiseTextChanged = true;
        }

        private void ValidateItemsPlacement(TextBox textBox)
        {
            innerItemsControl.UpdateLayout();

            var totalHeight = FindTotalHeightOfLastItem();

            if (totalHeight <= MaxHeight) return;

            var difference = totalHeight - MaxHeight;
            var lineHeight = textBox.CalculateLineHeight();

            var exceedingLineCount = (int)Math.Ceiling(difference / lineHeight);
            var newLastLineIndex = textBox.LineCount - exceedingLineCount - 1;

            int firstPositionInLastValidLine = textBox.GetCharacterIndexFromLineIndex(newLastLineIndex);
            int lengthOfLastValidLine = textBox.GetLineLength(newLastLineIndex);
            int lastValidPosition = firstPositionInLastValidLine + lengthOfLastValidLine;

            var newText = textBox.Text.Remove(lastValidPosition);

            int prevCaret = textBox.CaretIndex;

            UpdateTextWithoutRaise(textBox, newText);

            // Restores caret

            if (prevCaret > newText.Length)
            {
                prevCaret = newText.Length;
            }

            textBox.CaretIndex = prevCaret;
        }

        #endregion

        #endregion

        #region Events

        private void OnTextChanged(object sender, TextChangedEventArgs args)
        {
            if (!raiseTextChanged) return;

            ValidateItemsPlacement(sender as TextBox);
        }

        private void OnGeneratorStatusChanged(object sender, EventArgs args)
        {
            if (innerItemsControl.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;

            SubscribeItemsTextChanged(innerItemsControl.Items);
        }

        private void SubscribeItemsTextChanged(IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                var presenter = innerItemsControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;

                // Template might be not applied yet, thats why this method should be executed before FindName.

                presenter.ApplyTemplate();

                var textControl = FindTextContainer(presenter);
                var textBox = textControl.textBox;

                // Another advantage of WeakEventManager.
                // Does not require try-catch block to check if handler already attached, and if so - remove it.
                WeakEventManager<TextBox, TextChangedEventArgs>.RemoveHandler(textBox, nameof(TextBox.TextChanged), OnTextChanged);
                WeakEventManager<TextBox, TextChangedEventArgs>.AddHandler(textBox, nameof(TextBox.TextChanged), OnTextChanged);
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
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(TextItemsControl), new PropertyMetadata(null));

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
                    var textContainer = pageItemControl.FindTextContainer(contentPresenter);

                    // Support for TextContainer
                    if (textContainer != null) textContainer.IsEditable = pageItemControl.IsEditable;
                }
            }
        }

        #endregion
    }
}
