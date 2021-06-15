using Lessium.Models;
using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Lessium.Services;
using Lessium.Utility;

namespace Lessium.ContentControls
{
    // All ContentControls should have parent of type ContentPageControl.
    public partial class ContentPageControl : UserControl, IActionSender
    {
        private ContentPageModel contentPage;

        [Obsolete("You should not manually create ContentPageControl. This constructor used for creating control in XAML.", true)]
        public ContentPageControl() : base()
        {
            Initialize();
        }

        #region Methods

        #region Public

        public void Initialize()
        {
            // Subscribes to events

            DataContextChanged += OnDataContextChanged;
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;

            InitializeComponent();

            // Will redirect SendAction events from ContentPageModel to MainWindowViewModel

            ActionSenderRegistratorService.RegisterSender(this);
        }

        public bool IsElementFits(FrameworkElement element)
        {
            // Ensures that content childs properly updated.

            element.UpdateLayout();

            // Ensures that all items are properly placed

            this.UpdateLayout();

            // Checks

            var pos = element.TranslatePoint(default(Point), this);
            var fits = pos.Y + element.ActualHeight <= ActualHeight;

            return fits;
        }

        public Button RequestRemoveButtonCopy()
        {
            var button = FindResource("removeButtonTemplate") as Button;
            var newButton = CloneRemoveButton(button);

            contentPage.BindRemoveButtonToPage(newButton);

            return newButton;
        }

        public bool IsModelContainsControl(IContentControl control)
        {
            return contentPage.Items.Contains(control);
        }

        #endregion

        #region Private

        private void CloneBindings(Button newButton, Button template)
        {
            var isEnabledBindingExpression = template.GetBindingExpression(IsEnabledProperty);
            newButton.SetBinding(IsEnabledProperty, isEnabledBindingExpression.ParentBinding);

            var visibilityBindingExpression = template.GetBindingExpression(VisibilityProperty);
            newButton.SetBinding(VisibilityProperty, visibilityBindingExpression.ParentBinding);
        }

        private void CloneContent(Button newButton, Button template)
        {
            var templateImage = template.Content as Image;

            newButton.Content = new Image()
            {
                Source = templateImage.Source,
            };
        }

        private void CloneVisual(Button newButton, Button template)
        {
            newButton.MinWidth = template.MinWidth;
            newButton.MinHeight = template.MinHeight;

            newButton.MaxWidth = template.MaxWidth;
            newButton.MaxHeight = template.MaxHeight;

            newButton.Width = template.Width;
            newButton.Height = template.Height;

            newButton.HorizontalAlignment = template.HorizontalAlignment;
            newButton.VerticalAlignment = template.VerticalAlignment;
        }

        private Button CloneRemoveButton(Button template)
        {
            // EventHandlers will be attached in ContentPageModel.

            var newButton = new Button();

            CloneVisual(newButton, template);
            CloneContent(newButton, template);
            CloneBindings(newButton, template);

            return newButton;
        }

        private void UpdateModelMaxWidth(double newMaxWidth)
        {
            contentPage.MaxWidth = newMaxWidth;
        }

        private void UpdateModelMaxHeight(double newMaxHeight)
        {
            contentPage.MaxHeight = newMaxHeight;
        }

        private void UpdateModelMaxWidth()
        {
            UpdateModelMaxWidth(MaxWidth);
        }

        private void UpdateModelMaxHeight()
        {
            UpdateModelMaxHeight(MaxHeight);
        }

        private void UpdateModelMaxSize(Size newSize)
        {
            UpdateModelMaxWidth(newSize.Width);
            UpdateModelMaxHeight(newSize.Height);
        }

        private void UpdateModelMaxSize()
        {
            UpdateModelMaxWidth();
            UpdateModelMaxHeight();
        }

        private void ValidateSendActionRegistration(ContentPageModel oldPage, ContentPageModel newPage)
        {
            if (oldPage != null) oldPage.SendAction -= this.SendAction;
            if (newPage != null) newPage.SendAction += this.SendAction;
        }

        private void UpdateModelEditable()
        {
            if (contentPage == null) return;

            contentPage.IsEditable = IsEditable;
        }

        #endregion

        #endregion

        #region Events

        // DataContext = CurrentPage
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            contentPage = e.NewValue as ContentPageModel;
            var oldPage = e.OldValue as ContentPageModel;

            if (oldPage != null) oldPage.Enabled = false;

            // Even if contentPage is null, will unregister (if not null) old page, that's why we should call it here.

            ValidateSendActionRegistration(oldPage, contentPage);

            if (contentPage == null) // Wrong DataContext (probably MainWindowViewModel) or empty page
            { 
                Items = null;
                return; 
            }

            contentPage.Enabled = true;

            UpdateModelEditable();
            UpdateModelMaxSize();

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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // If MaxHeight property will be modified before loading - it could make binding problems.
            // It's can be fixed easily, just adding binding after loading!

            var binding = new Binding(nameof(MaxHeight));
            binding.Source = this;
            binding.FallbackValue = ContentPageModel.PageHeight;

            SetBinding(HeightProperty, binding);
        }

        private void itemsControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftCtrl)) return;

            var focused = Keyboard.FocusedElement;

            // Check if ListBoxItem is focused, not the content inside it, but border.
            // Instead of ListBoxItem, the ListBox itself could be focused too (for paste).

            if (!(focused is ListBoxItem) && !(focused is ListBox)) return;

            if (Keyboard.IsKeyDown(Key.C))
            {
                var serializable = itemsControl.SelectedItem as ILsnSerializable;

                if (serializable == null) return;

                ClipboardService.CopySerializable(serializable);

                return;
            }

            if (Keyboard.IsKeyDown(Key.V))
            {
                if (!IsEditable) return;

                var copiedContent = ClipboardService.GetStoredSerializable() as IContentControl;

                if (copiedContent == null) return;

                contentPage.Add(copiedContent);
            }
        }

        private void itemsControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            // Creates new MouseWheelEvent

            var newEvent = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent
            };

            // Raises created event.
            // Therefore handlers attached to ContentPageControl.MouseWheel will be able to receive input from ListBox.

            RaiseEvent(newEvent);
        }

        private void ListBoxItem_LostFocus(object sender, RoutedEventArgs e)
        {
            var focusedObject = Keyboard.FocusedElement as DependencyObject;

            if (focusedObject == null) return;

            var control = focusedObject.FindParent<IContentControl>();
            if (control == null) return;

            var listBoxItem = sender as ListBoxItem;

            listBoxItem.IsSelected = false;
        }

        /// <summary>
        /// Performs all necessary setup once ListBoxItem is loaded.
        /// </summary>
        private void ListBoxItem_Loaded(object sender, RoutedEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            var contentControl = listBoxItem.Content as FrameworkElement;

            var totalBorderWidth = listBoxItem.BorderThickness.Left + listBoxItem.BorderThickness.Right;
            var totalPadding = listBoxItem.Padding.Left + listBoxItem.Padding.Right;
            var totalOffset = totalBorderWidth + totalPadding;

            var distanceWidth = MathHelper.DistanceBetweenElements(itemsControl, listBoxItem, Coordinate.X);
            var distanceHeight = MathHelper.DistanceBetweenElements(itemsControl, listBoxItem, Coordinate.Y);

            contentControl.MaxWidth = itemsControl.MaxWidth - totalOffset - distanceWidth * 2; // multiply by 2 to count both Left and Right space out.
            contentControl.MaxHeight = itemsControl.MaxHeight - distanceHeight;
        }

        #endregion

        #region Dependency Properties

        public ObservableCollection<IContentControl> Items
        {
            get { return (ObservableCollection<IContentControl>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
            typeof(ContentPageControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(ContentPageControl), new PropertyMetadata(true, (sender, e) => 
            {
                var pageControl = sender as ContentPageControl;

                pageControl.UpdateModelEditable();
            }));

        #endregion

        #region IActionSender

        public event SendActionEventHandler SendAction;

        #endregion
    }
}
