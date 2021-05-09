using Lessium.ContentControls;
using Lessium.Models;
using Lessium.Interfaces;
using Lessium.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lessium.Classes.IO
{
    public static class LsnWriter
    {
        private static IDispatcher dispatcher = null;
        public static IDispatcher Dispatcher
        {
            get
            {
                if (dispatcher == null)
                {
                    dispatcher = DispatcherUtility.Dispatcher;
                }

                return dispatcher;
            }

            set { dispatcher = value; }
        }

        private static CancellationTokenSource cts;
        private static bool canceledManually;

        private static XmlWriterSettings settings = new XmlWriterSettings()
        {
            Indent = true,
            Async = true,
        };

        public static void Cancel()
        {
            cts?.Cancel();
        }

        public async static Task<IOResult> SaveAsync(LessonModel lessonModel, string fileName, IProgress<ProgressType> progress)
        {
            if (cts != null) throw new ThreadStateException("Thread is already started.");

            canceledManually = false;
            cts = new CancellationTokenSource();
            var result = IOResult.Null;

            using (cts)
            {
                Task task = null;

                try
                {
                    task = SaveInternalAsync(lessonModel, fileName, cts.Token, progress);
                    await task;
                }

                catch (Exception e)
                {
                    if (e is TaskCanceledException)
                    {
                        result = canceledManually ? IOResult.Cancelled : IOResult.Error;
                    }
                    else
                    {
                        Console.WriteLine($"Error while saving file - {e}");
                    }
                }

                if (task.IsCompleted) { result = IOResult.successful; }
            }

            cts = null;

            return result;
        }

        private static async Task SaveInternalAsync(LessonModel lessonModel, string fileName, CancellationToken token,
            IProgress<ProgressType> progress)
        {
            token.ThrowIfCancellationRequested();

            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                await writer.WriteStartDocumentAsync();

                #region Lesson

                await writer.WriteStartElementAsync("Lesson");

                await WriteTabAsync(writer, lessonModel, token, progress, ContentType.Material);
                await WriteTabAsync(writer, lessonModel, token, progress, ContentType.Test);

                await writer.WriteEndElementAsync();

                #endregion

                await writer.WriteEndDocumentAsync();

                writer.Close();
            }

            token.ThrowIfCancellationRequested();
        }

        private static async Task WriteTabAsync(XmlWriter writer, LessonModel lessonModel, CancellationToken token,
            IProgress<ProgressType> progress, ContentType tabType)
        {
            if (!token.IsCancellationRequested)
            {
                // Reports to process new Tab.

                progress.Report(ProgressType.Tab);

                #region TabString

                await writer.WriteStartElementAsync(tabType.ToTabString(true));

                var sections = lessonModel.GetSectionsOfType(tabType);

                // Iterating over sections in tab.
                for (int i = 0; i < sections.Count; i++)
                {
                    if (token.IsCancellationRequested) { break; }

                    var section = sections[i];
                    await section.WriteXmlAsync(writer, progress, token);
                }

                await writer.WriteEndElementAsync();

                #endregion
            }
        }

        private static Task CountPagesAsync(Section section, int sectionIndex, CountData data, CancellationToken token)
        {
            int pageIndex = 0;

            ObservableCollection<ContentPageModel> pages = null;

            Dispatcher.Invoke(() =>
            {
                pages = section.Pages;
            });

            foreach (var page in pages)
            {
                if(token.IsCancellationRequested) { break; }

                data.AddPage(sectionIndex, pageIndex, page.Items.Count);
                pageIndex++;
            }

            return Task.CompletedTask;
        }

        private static async Task CountSectionsAsync(LessonModel lessonModel, CountData data, ContentType type, CancellationToken token)
        {
            int sectionIndex = 0;

            foreach (var section in lessonModel.GetSectionsOfType(type))
            {
                if (token.IsCancellationRequested) { break; }

                data.AddSection(sectionIndex);
                await CountPagesAsync(section, sectionIndex, data, token);
                sectionIndex++;
            }
        }

        public static async Task<Dictionary<ContentType, CountData>> CountDataAsync(LessonModel lessonModel, string fileName)
        {
            cts = new CancellationTokenSource();
            var token = cts.Token;
            Dictionary<ContentType, CountData> result = new Dictionary<ContentType, CountData>();

            try
            {
                // Reads materials
                if (!token.IsCancellationRequested)
                {
                    var materialsData = new CountData();
                    var type = ContentType.Material;

                    await CountSectionsAsync(lessonModel, materialsData, type, token); // Executed synchronously.
                    result.Add(type, materialsData);
                }

                // Reads tests
                if (!token.IsCancellationRequested)
                {
                    var testsData = new CountData();
                    var type = ContentType.Test;

                    await CountSectionsAsync(lessonModel, testsData, type, token); // Executed synchronously.
                    result.Add(type, testsData);
                }

                if (token.IsCancellationRequested) result = null;

            }

            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            cts = null;

            return result;
        }
    }
}
