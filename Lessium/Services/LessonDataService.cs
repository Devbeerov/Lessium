using System;
using System.Linq;
using System.Windows;
using Lessium.Views;
using Lessium.ViewModels;

namespace Lessium.Services
{
    /// <summary>
    /// Helps to receive runtime data, which can be useful for getting runtime data from ViewModels.
    /// </summary>
    public static class RuntimeDataService
    {
        private static MainWindowViewModel mainViewModel = null;
        private static MainWindowViewModel MainViewModel
        {
            get
            {
                if (mainViewModel == null)
                {
                    var mainView = Application.Current.Windows.OfType<MainWindow>().Single();

                    if (mainView == null) throw new NullReferenceException("Main Window is not created yet (is null).");

                    mainViewModel = mainView.DataContext as MainWindowViewModel;
                }

                return mainViewModel;
            }
        }

        public static bool IsLessonInReadOnly()
        {
            return MainViewModel.ReadOnly;
        }
    }
}
