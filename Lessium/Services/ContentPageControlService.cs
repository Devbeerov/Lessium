using Lessium.ContentControls;
using Lessium.Converters;
using Lessium.CustomControls;
using Lessium.Interfaces;
using Lessium.Models;
using Lessium.Views;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.Services
{
    public static class ContentPageControlService
    {
        private static readonly UIElementsDistanceConverter distanceConverter = new UIElementsDistanceConverter();
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

        public static Button RequestRemoveButtonCopy(bool shouldRemoveControlOnClick)
        {
            return PageControl.RequestRemoveButtonCopy(shouldRemoveControlOnClick);
        }

        public static bool IsManagingControl(IContentControl control)
        {
            return PageControl.IsModelContainsControl(control);
        }


        /// <summary>
        /// Translates UIElement with Point to PageControl.
        /// </summary>
        public static Point TranslatePoint(UIElement elementToTranslate, Point point = default)
        {
            return elementToTranslate.TranslatePoint(point, PageControl);
        }

        public static double CalculateDistanceToElement(UIElement element, Coordinate coordinate)
        {
            var inputElements = new object[] { PageControl, element };

            return (double)distanceConverter.Convert(inputElements, null, Coordinate.Y.ToString(), CultureInfo.InvariantCulture);
        }
    }
}
