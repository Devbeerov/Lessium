using Lessium.Interfaces;
using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Linq;
using Lessium.Utility;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Lessium.Classes.IO;
using System.Collections.Generic;

namespace Lessium.ContentControls.MaterialControls
{
    /// <summary>
    /// Simple TextBlock with wrapping.
    /// </summary>
    [Serializable]
    public partial class TextControl : UserControl, IMaterialControl
    {
        private IDispatcher dispatcher;
        private bool raiseResizeEvent = true;

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
            InitializeComponent();
            this.DataContext = this;
        }

        public void RemoveBehavior<T>() where T : Behavior
        {
            var behaviors = Interaction.GetBehaviors(textBox);
            var behavior = GetBehavior<T>();
            if (behavior != null)
            {
                behaviors.Remove(behavior);
            }
        }

        public T GetBehavior<T>() where T : Behavior
        {
            var behaviors = Interaction.GetBehaviors(textBox);
            return behaviors.OfType<T>().FirstOrDefault();
        }

        #endregion

        #endregion

        #region IContentControl

        public void SetMaxWidth(double width)
        {
            var adjusted = width - removeButton.Width;

            raiseResizeEvent = false;

            this.MaxWidth = width;
            textBox.MaxWidth = adjusted;
            textBox.Width = adjusted;

            raiseResizeEvent = true;
        }

        public void SetMaxHeight(double height)
        {
            // We do not calculate adjustedHeight here because of design. Don't want to consider removeButton.Height here.

            this.MaxHeight = height;
            textBox.MaxHeight = height;
        }

        public event RoutedEventHandler RemoveControl;
        public event SizeChangedEventHandler Resize;

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set 
            { 
                SetValue(IsReadOnlyProperty, value);

                // ReadOnly

                textBox.IsReadOnly = value;

                // Size 0 if not editable, size 1 if editable.

                Thickness thickness;

                if (value)
                {
                    thickness = new Thickness(0);
                }

                else
                {
                    thickness = new Thickness(1);
                }

                textBox.BorderThickness = thickness;

                // Button

                if (ShowRemoveButton)
                {
                    removeButton.IsEnabled = !value;
                }

                // Tooltip

                ToolTipService.SetIsEnabled(textBox, !value);
            }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TextControl), new PropertyMetadata(true));

        #endregion

        #region Events

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Sets source to TextControl, not Button

            e.Source = this;

            // Invokes event

            RemoveControl?.Invoke(sender, e);
        }

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!raiseResizeEvent)
            {
                e.Handled = true;
                return;
            }

            // Sets source to TextControl, not Border

            e.Source = this;

            // Invokes event

            Resize?.Invoke(sender, e);
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

        #region ShowRemoveButton

        public bool ShowRemoveButton
        {
            get { return (bool)GetValue(ShowRemoveButtonProperty); }
            set { SetValue(ShowRemoveButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowRemoveButtonProperty =
            DependencyProperty.Register("ShowRemoveButton", typeof(bool), typeof(TextControl), new PropertyMetadata(true));

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
    }
}