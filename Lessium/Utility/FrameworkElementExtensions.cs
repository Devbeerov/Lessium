using System.Windows;

namespace Lessium.Utility
{
    public static class FrameworkElementExtensions
    {
        public static Rect GetRect(this FrameworkElement element)
        {
            return element.RenderTransform.TransformBounds(new Rect(element.RenderSize));
        }
    }
}
