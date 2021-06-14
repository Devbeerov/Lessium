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
using Settings = Lessium.Properties.Settings;

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

        #endregion

        #region IContentControl

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set 
            { 
                SetValue(IsEditableProperty, value);

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
        }

        // Using a DependencyProperty as the backing store for IsEditable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(TextControl), new PropertyMetadata(false));

        #endregion

        #region Events

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Sets source to TextControl, not Border

            e.Source = this;
        }

        private void RemoveButtonPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            RequestRemoveButton?.Invoke(this, new RemoveButtonRequestEventArgs(sender as ContentPresenter));
        }

        #endregion

        #region Dependency Properties

        #region Text

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextControl), 
                new FrameworkPropertyMetadata(Properties.Resources.TextControl_DefaultText));

        #endregion

        #region DefaultMaxLineCount

        public int DefaultMaxLineCount
        {
            get { return (int)GetValue(DefaultMaxLineCountProperty); }
            set { SetValue(DefaultMaxLineCountProperty, value); }
        }

        // 38 for FontSize = 12
        public static readonly DependencyProperty DefaultMaxLineCountProperty =
            DependencyProperty.Register("DefaultMaxLineCount", typeof(int), typeof(TextControl), new PropertyMetadata(38));

        #endregion

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

        #region IRemoveButtonRequestor

        public event RemoveButtonRequestEventHandler RequestRemoveButton;

        #endregion
    }
}