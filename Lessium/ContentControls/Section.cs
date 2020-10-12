using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lessium.ContentControls
{
    public class Section : StackPanel
    {
        
        public Section() : base()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;
            Width = double.NaN;
            Height = double.NaN;

            SetItems(InternalChildren);
        }

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

        public static UIElementCollection GetItems(DependencyObject obj)
        {
            return (UIElementCollection)obj.GetValue(Items);
        }

        protected static void SetItems(DependencyObject obj, UIElementCollection items)
        {
            obj.SetValue(Items, items);
        }

        public UIElementCollection GetItems()
        {
            return GetItems(this);
        }

        protected void SetItems(UIElementCollection items)
        {
            SetItems(this, items);
        }

        #endregion

        public void Add(UIElement element)
        {
            InternalChildren.Add(element);
        }

        public void Remove(UIElement element)
        {
            InternalChildren.Remove(element);
        }

        #region Dependency Properties

        // Used externally.
        public static readonly DependencyProperty Title =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(Section), new PropertyMetadata(string.Empty));

        // Used externally.
        public static readonly DependencyProperty Items =
            DependencyProperty.RegisterAttached("Items", typeof(UIElementCollection), typeof(Section), new PropertyMetadata(null));

        #endregion

    }
}
