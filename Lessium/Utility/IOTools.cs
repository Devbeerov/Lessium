using Lessium.Classes.IO;
using Lessium.ContentControls;
using Lessium.Models;
using Lessium.ViewModels;
using Lessium.Views;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Lessium.Utility
{
    public static class IOTools
    {
        public static ProgressWindow CreateProgressView(Window owner, string title, Dictionary<ContentType, CountData> countDataDictionary, IOType operationType)
        {
            if (countDataDictionary.Count == 0) throw new ArgumentException("CountData is empty!");

            var progressView = new ProgressWindow(title, countDataDictionary)
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

        public static async Task<(IOResult, LessonModel)> LoadLesson(string filePath, Window owner = null)
        {
            var countData = await LsnReader.CountDataAsync(filePath);

            // Creates ProgressView

            var progressView = CreateProgressView(owner, $"Loading ...", countData, IOType.Read);
            var progress = CreateProgressForProgressView(progressView);

            progressView.Show();

            // Tests Lesson's loading, in case it will throw any Exceptions during load,
            // Assert.DoesNotThrowAsync will fail test.

            var loadResult = await Task.Run(async () => await LsnReader.LoadAsync(filePath, progress)); // Nuget.System.ValueTuple

            progressView.Close();

            return loadResult;
        }
    }
}
