using Lessium.Classes.IO;
using Lessium.ViewModels;
using Lessium.Views;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Lessium.Utility
{
    public static class IOTools
    {
        public static ProgressWindow CreateProgressView(Window owner, string title, IOType operationType)
        {
            var progressView = new ProgressWindow(title)
            {
                Owner = owner,
            };

            EventHandler onClose = null;

            switch (operationType)
            {
                case IOType.Read:
                    onClose = (s, a) => LsnReader.Cancel();
                    break;
                case IOType.Write:
                    onClose = (s, a) => LsnWriter.Cancel();
                    break;
            }

            progressView.Closed += onClose;

            return progressView;
        }

        public static Progress<ProgressType> CreateProgressForProgressView(ProgressWindow view)
        {
            return new Progress<ProgressType>((view.DataContext as ProgressWindowViewModel).UpdateProgress);
        }
    }
}
