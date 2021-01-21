using Lessium.Interfaces;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Linq;
using Lessium.Utility;

namespace Lessium.ContentControls.MaterialControls
{
    /// <summary>
    /// Simple TextBlock with wrapping.
    /// </summary>
    [Serializable]
    public partial class TextControl : UserControl, IMaterialControl
    {
        #region Constructors

        public TextControl()
        {
            Initialize();
        }

        // For serialization
        protected TextControl(SerializationInfo info, StreamingContext context)
        {
            // Initializes component

            Initialize();

            // Serializes properties

            SetText(info.GetString("Text"));
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

        #region ISerializable

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Text", textBox.Text);
        }

        #endregion

        #region IContentControl

        public void SetEditable(bool editable)
        {
            // ReadOnly

            textBox.IsReadOnly = !editable;

            // Border

            var converter = new ThicknessConverter();

            // Size 0 if not editable, size 1 if editable.

            var thickness = (Thickness)converter.ConvertFrom(editable);

            textBox.BorderThickness = thickness;

            // Button

            if (GetShowRemoveButton(this))
            {
                removeButton.IsEnabled = editable;
            }

            // Tooltip

            ToolTipService.SetIsEnabled(textBox, editable);
        }

        public void SetMaxWidth(double width)
        {
            var adjusted = width - removeButton.Width;

            raiseResizeEvent = false;

            textBox.MaxWidth = adjusted;
            textBox.Width = adjusted;

            raiseResizeEvent = true;
        }

        public void SetMaxHeight(double height)
        {
            // We do not calculate adjustedHeight here because of design. Don't want to consider removeButton.Height here.

            textBox.MaxHeight = height;
        }

        public event RoutedEventHandler RemoveControl;
        public event SizeChangedEventHandler Resize;

        private bool raiseResizeEvent = true;

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

        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }

        public static void SetText(DependencyObject obj, string text)
        {
            obj.SetValue(TextProperty, text);
        }

        public string GetText()
        {
            return GetText(this);
        }

        public void SetText(string text)
        {
            SetText(this, text);
        }

        public string Text
        {
            get { return GetText(); }
            set { SetText(value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextControl), 
                new FrameworkPropertyMetadata(Properties.Resources.TextControl_DefaultText));

        public static void SetShowRemoveButton(DependencyObject obj, bool show)
        {
            obj.SetValue(ShowRemoveButtonProperty, show);
        }

        public static bool GetShowRemoveButton(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowRemoveButtonProperty);
        }


        public void SetShowRemoveButton(bool show)
        {
            SetShowRemoveButton(this, show);
        }

        public static readonly DependencyProperty ShowRemoveButtonProperty =
            DependencyProperty.Register("ShowRemoveButton", typeof(bool), typeof(TextControl), new PropertyMetadata(true));

        #endregion
    }
}