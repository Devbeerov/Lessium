using Lessium.Utility;
using Lessium.ViewModels;
using System;
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

        public async static Task<IOResult> SaveAsync(MainWindowViewModel viewModel, string fileName, IProgress<int> progress)
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
            IProgress<int> progress)
        {
            token.ThrowIfCancellationRequested();

            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                await writer.WriteStartDocumentAsync();

                #region Lesson

                await writer.WriteStartElementAsync("Lesson");

                #region Materials

                if (!token.IsCancellationRequested)
                {
                    await writer.WriteStartElementAsync("Materials");

                    // Iterating over sections in Materials tab.
                    var materialTabs = viewModel.SectionsByTab["Materials"];
                    for (int i = 0; i < materialTabs.Count; i++)
                    {
                        if (token.IsCancellationRequested) { break; }

                        var section = materialTabs[i];
                        await section.WriteXmlAsync(writer, token, progress);
                    }

                    await writer.WriteEndElementAsync();
                }

                #endregion

                #region Tests

                if (!token.IsCancellationRequested)
                {
                    await writer.WriteStartElementAsync("Tests");

                    // Iterating over sections in Tests tab.
                    var testsTab = viewModel.SectionsByTab["Tests"];
                    for (int i = 0; i < testsTab.Count; i++)
                    {
                        if (token.IsCancellationRequested) { break; }

                        var section = testsTab[i];
                        await section.WriteXmlAsync(writer, token, progress);
                    }

                    await writer.WriteEndElementAsync();
                }

                #endregion

                await writer.WriteEndElementAsync();

                #endregion

                await writer.WriteEndDocumentAsync();

                writer.Close();
            }

            token.ThrowIfCancellationRequested();
        }
    }
}
