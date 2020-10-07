using System.Windows;
using System.Windows.Controls;

namespace Lessium.ContentControls
{
    public class Section : StackPanel
    {
        public Section() : base()
        {
        }

        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(Title);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(Title, value);
        }

        // Used externally.
        public static readonly DependencyProperty Title =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(Section), new PropertyMetadata(string.Empty));


    }
}
