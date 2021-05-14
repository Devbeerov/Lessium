using Lessium.ContentControls;
using Lessium.Interfaces;
using Lessium.Models;
using Lessium.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lessium.Services
{
   public static class PageValidationHelperService
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

        public static bool IsElementFits(ContentPageModel model, FrameworkElement contentElement)
        {
            if (contentElement == null) throw new ArgumentNullException(nameof(contentElement));
            if (!PageControl.IsControlsModel(model)) throw new ArgumentException("ContentPageControl does not control this model.");

            return PageControl.IsElementFits(contentElement);
        }
   }
}
