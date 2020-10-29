using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.ContentControls
{
    public class Section : StackPanel
    {
        private bool editable = false;

        public Section() : base()
        {
            Width = double.NaN;
            Height = double.NaN;

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;

            SetItems(items);
        }

        private readonly ObservableCollection<UIElement> items = new ObservableCollection<UIElement>();

        #region Dependency Properties Methods

        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(Title);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(Title, value);
        }

        public string GetTitle()
        {
            return GetTitle(this);
        }

        public void SetTitle(string title)
        {
            SetTitle(this, title);
        }

        public static ObservableCollection<UIElement> GetItems(DependencyObject obj)
        {
            return (ObservableCollection<UIElement>)obj.GetValue(Items);
        }

        protected static void SetItems(DependencyObject obj, ObservableCollection<UIElement> items)
        {
            obj.SetValue(Items, items);
        }

        public ObservableCollection<UIElement> GetItems()
        {
             return GetItems(this);
        }

        protected void SetItems(ObservableCollection<UIElement> items)
        {
            SetItems(this, items);
        }

        #endregion

        #region Methods

        #region Private

        private void UpdateItemEditable(object item)
        {
            var contentControl = item as IContentControl;

            if (contentControl != null)
            {
                contentControl.SetEditable(editable);
            }
        }

        private void UpdateItemsEditable()
        {
            foreach (object childControl in items)
            {
                UpdateItemEditable(childControl);
            }
        }

        #endregion

        #region Public

        public void Add(UIElement element)
        {
            items.Add(element);
            UpdateItemEditable(element);
        }

        public void Remove(UIElement element)
        {
            items.Remove(element);
        }

        public bool GetEditable()
        {
            return editable;
        }

        public void SetEditable(bool editable)
        {
            this.editable = editable;

            UpdateItemsEditable();
        }

        #endregion

        #endregion

        #region Dependency Properties

        // Used externally.
        public static readonly DependencyProperty Title =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(Section), new PropertyMetadata(string.Empty));

        // Used externally.
        public static readonly DependencyProperty Items =
            DependencyProperty.Register("Items", typeof(ObservableCollection<UIElement>),
                typeof(Section), new PropertyMetadata(null));

        #endregion

        #endregion

    }
}
