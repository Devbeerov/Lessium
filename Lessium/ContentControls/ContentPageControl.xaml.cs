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

        private void ValidateSendActionRegistration(ContentPageModel oldPage, ContentPageModel newPage)
        {
            if (oldPage != null) oldPage.SendAction -= this.SendAction;
            if (newPage != null) newPage.SendAction += this.SendAction;
        }

        #endregion

        #endregion

        #region Events

        // DataContext = CurrentPage
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            contentPage = e.NewValue as ContentPageModel;

            // Even if contentPage is null, will unregister (if not null) old page, that's why we should call it here.

            ValidateSendActionRegistration(e.OldValue as ContentPageModel, contentPage);

            if (contentPage == null) // Wrong DataContext (probably MainWindowViewModel) or empty page
            { 
                Items = null;
                return; 
            }

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
                if (IsReadOnly) return;

                var copiedContent = ClipboardService.GetStoredSerializable() as IContentControl;

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
            //listBoxItem.isf
            listBoxItem.IsSelected = false;
        }

        private void evt(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Dependency Properties

        public ObservableCollection<IContentControl> Items
        {
            get { return (ObservableCollection<IContentControl>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
            typeof(ContentPageControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ContentPageControl), new PropertyMetadata(true));

        #endregion

        #region IActionSender

        public event SendActionEventHandler SendAction;

        #endregion
    }
}
