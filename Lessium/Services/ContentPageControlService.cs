using Lessium.ContentControls;
using Lessium.Interfaces;
using Lessium.Models;
using Lessium.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.Services
{
    public static class ContentPageControlService
   {
        private static ContentPageControl pageControl;
        private static ContentPageControl PageControl
        {
            get
            {
                if (pageControl == null)
                {
                    var mainView = Application.Current.Windows.OfType<MainWindow>().Single();

                    if (mainView == null) throw new NullReferenceException("Main Window is not created yet (is null).");

                    var retrievedPageControl = mainView.FindName("contentPageControl") as ContentPageControl;

                    if (retrievedPageControl == null) throw new NullReferenceException("contentPageControl is not found.");

                    pageControl = retrievedPageControl;
                }

                return pageControl;
            }
        }

        public static double MaxHeight
        {
            get { return PageControl.MaxHeight; }
        }

        public static bool IsControlFits(IContentControl control)
        {
            if (control == null) throw new ArgumentNullException(nameof(control));
            if (!PageControl.IsModelContainsControl(control)) throw new InvalidOperationException("ContentPage is not currently controlling control");

            var element = control as FrameworkElement;

            if (element == null) throw new InvalidOperationException($"{control} is not FrameworkElement");

            return PageControl.IsElementFits(element);
        }

        public static Button RequestRemoveButtonCopy()
        {
            return PageControl.RequestRemoveButtonCopy();
        }

        // elementToTranslate.TranslatePoint(point, PageControl);
        public static Point TranslatePoint(UIElement elementToTranslate, Point point = default)
        {
            return elementToTranslate.TranslatePoint(point, PageControl);
        }
   }
}
