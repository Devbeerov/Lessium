using Lessium.Interfaces;
using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using Lessium.Utility;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Lessium.Classes.IO;
using Lessium.Utility.Behaviors;
using System.Windows.Media;

namespace Lessium.ContentControls.MaterialControls
{
    /// <summary>
    /// Simple TextBlock with wrapping.
    /// </summary>
    [Serializable]
    public partial class TextControl : UserControl, IMaterialControl
    {
        private readonly IDispatcher dispatcher;

        #region Constructors

        public TextControl()
        {
            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;

            Initialize();
        }

        public TextControl(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;

            Initialize();
        }

        // For serialization
        protected TextControl(SerializationInfo info, StreamingContext context)
        {
            dispatcher = DispatcherUtility.Dispatcher;

            // Initializes component

            Initialize();

            // Serializes properties

            Text = info.GetString(nameof(Text));
        }

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            DataContext = this;
            InitializeComponent();
        }

        #endregion

        #region Private

        private void RemovePresenter()
        {
            grid.Children.Remove(removeButtonPresenter);
        }

        #endregion

        #endregion

        #region IContentControl

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set
            {
                SetValue(IsEditableProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for IsEditable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(TextControl), new PropertyMetadata(false, new PropertyChangedCallback(OnEditableChanged)));

        private static void OnEditableChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var textControl = sender as TextControl;
            var textBox = textControl.textBox;
            var value = (bool)args.NewValue;

            textBox.IsReadOnly = !value;

            // Size 0 if not editable, size 1 if editable.

            Thickness thickness;

            if (value)
            {
                thickness = new Thickness(1);
            }

            else
            {
                thickness = new Thickness(0);
            }

            textBox.BorderThickness = thickness;

            // Tooltip

            ToolTipService.SetIsEnabled(textBox, value);
        }

        #endregion

        #region Events

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Sets source to TextControl, not Border

            e.Source = this;
        }

        #endregion

        #region Dependency Properties

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public bool UsesCutBehavior
        {
            get { return (bool)GetValue(UsesCutBehaviorProperty); }
            set { SetValue(UsesCutBehaviorProperty, value); }
        }

        public bool UseRemoveButtonPresenter
        {
            get { return (bool)GetValue(UseRemoveButtonPresenterProperty); }
            set { SetValue(UseRemoveButtonPresenterProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextControl),
                new FrameworkPropertyMetadata(Properties.Resources.TextControl_DefaultText));

        public static readonly DependencyProperty UsesCutBehaviorProperty =
            DependencyProperty.Register("UsesCutBehavior", typeof(bool), typeof(TextControl), new PropertyMetadata(true,
                new PropertyChangedCallback(OnCutBehaviorUseChanged)));

        public static readonly DependencyProperty UseRemoveButtonPresenterProperty =
            DependencyProperty.Register("UseRemoveButtonPresenter", typeof(bool), typeof(TextControl), new PropertyMetadata(true,
                new PropertyChangedCallback(OnUseRemoveButtonPresenterChanged)));

        private static void OnCutBehaviorUseChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as TextControl;

            if (control.UsesCutBehavior)
            {
                control.textBox.AddBehavior(new TextBoxCutBehavior());
                return;
            }

            control.textBox.RemoveBehavior<TextBoxCutBehavior>();
        }

        private static void OnUseRemoveButtonPresenterChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as TextControl;

            if (control.IsLoaded) throw new NotSupportedException("Should be setup before TextControl is loaded!");
            if (control.removeButtonPresenter == null) throw new NotSupportedException("RemoveButtonPresenter is already removed.");

            var usesRemovePresenter = (bool)args.NewValue;

            if (!usesRemovePresenter) control.RemovePresenter();
        }

        #endregion

        #region ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dispatcher.Invoke(() =>
            {
                info.AddValue(nameof(Text), Text);
            });
        }

        #endregion

        #region ILsnSerializable

        public async Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Reports to process new Content.

            progress.Report(ProgressType.Content);

            #region TextControl

            await writer.WriteStartElementAsync(GetType().Name);

            await dispatcher.InvokeAsync(async () =>
            {
                await writer.WriteStringAsync(Text);
            });

            await writer.WriteEndElementAsync();

            #endregion
        }

        public async Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Reports to process new Content.

            progress.Report(ProgressType.Content);

            // Content of TextControl is string. So we just extracts it entirely.

            await dispatcher.InvokeAsync(async () =>
            {
                Text = await reader.ReadElementContentAsStringAsync();
            });
        }

        #endregion
    }
}