﻿using Lessium.ContentControls;
using Lessium.Utility;
using Lessium.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lessium.Classes.IO
{
    public static class LsnWriter
    {
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

        public async static Task<IOResult> SaveAsync(MainWindowViewModel viewModel, string fileName, IProgress<ProgressType> progress)
        {
            canceledManually = false;
            cts = new CancellationTokenSource();
            var result = IOResult.Null;

            using (cts)
            {
                var task = SaveInternalAsync(viewModel, fileName, cts.Token, progress);

                try
                {
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
                        Console.WriteLine($"Error while saving file - {e.ToString()}");
                    }
                }

                if (task.IsCompleted) { result = IOResult.Sucessful; }
            }

            cts = null;

            return result;
        }

        private static async Task SaveInternalAsync(MainWindowViewModel viewModel, string fileName, CancellationToken token,
            IProgress<ProgressType> progress)
        {
            token.ThrowIfCancellationRequested();

            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                await writer.WriteStartDocumentAsync();

                #region Lesson

                await writer.WriteStartElementAsync("Lesson");

                await WriteTabAsync(writer, viewModel, token, progress, ContentType.Material);
                await WriteTabAsync(writer, viewModel, token, progress, ContentType.Test);

                await writer.WriteEndElementAsync();

                #endregion

                await writer.WriteEndDocumentAsync();

                writer.Close();
            }

            token.ThrowIfCancellationRequested();
        }

        private static async Task WriteTabAsync(XmlWriter writer, MainWindowViewModel viewModel, CancellationToken token,
            IProgress<ProgressType> progress, ContentType tabType)
        {
            if (!token.IsCancellationRequested)
            {
                // Reports to process new Tab.

                progress.Report(ProgressType.Tab);

                #region TabString

                await writer.WriteStartElementAsync(tabType.ToTabString(true));

                // Iterating over sections in tab.
                var sections = viewModel.SectionsByType[tabType];
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

        private static void CountPages(Section section, int sectionIndex, CountData data, CancellationToken token)
        {
            int pageIndex = 0;

            foreach (var page in section.GetPages())
            {
                if(token.IsCancellationRequested) { break; }

                data.AddPage(sectionIndex, pageIndex, page.Items.Count);
                pageIndex++;
            }
        }

        private static void CountSections(MainWindowViewModel viewModel, CountData data, ContentType type, CancellationToken token)
        {
            int sectionIndex = 0;

            foreach (var section in viewModel.SectionsByType[type])
            {
                if (token.IsCancellationRequested) { break; }

                data.AddSection(sectionIndex);
                CountPages(section, sectionIndex, data, token);
                sectionIndex++;
            }
        }

        public static Dictionary<ContentType, CountData> CountData(MainWindowViewModel viewModel, string fileName)
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

                    CountSections(viewModel, materialsData, type, token); // Executed synchronously.
                    result.Add(type, materialsData);
                }

                // Reads tests
                if (!token.IsCancellationRequested)
                {
                    var testsData = new CountData();
                    var type = ContentType.Test;

                    CountSections(viewModel, testsData, type, token); // Executed synchronously.
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
