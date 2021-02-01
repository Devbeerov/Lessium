using Lessium.ViewModels;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lessium.Classes
{
    public static class LsnWriter
    {
        private static CancellationTokenSource cts;
        private static bool canceledManually;

        public enum Result
        {
            Null, Error, Sucessful, Cancelled, Timeout
        }

        private static XmlWriterSettings settings = new XmlWriterSettings()
        {
            Indent = true,
            Async = true,
        };

        public static void Cancel()
        {
            cts?.Cancel();
        }

        public async static Task<Result> SaveAsync(MainWindowViewModel viewModel, string fileName, IProgress<int> progress)
        {
            canceledManually = false;
            cts = new CancellationTokenSource();
            var result = Result.Null;

            using (cts)
            {
                var task = SaveInternal(viewModel, fileName, cts.Token, progress);

                try
                {
                    await task;
                }

                catch (Exception e)
                {
                    if (e is TaskCanceledException)
                    {
                        result = canceledManually ? Result.Cancelled : Result.Error;
                    }
                    else
                    {
                        Console.WriteLine($"Error while saving file - {e.ToString()}");
                    }
                }

                if (task.IsCompleted) { result = Result.Sucessful; }
            }

            cts = null;

            return result;
        }

        private static async Task SaveInternal(MainWindowViewModel viewModel, string fileName, CancellationToken token,
            IProgress<int> progress)
        {
            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                token.ThrowIfCancellationRequested();
                await writer.WriteStartDocumentAsync();

                //TODO: 
                await writer.WriteStartElementAsync("Lesson");

                //writer.WriteStartElement("user");
                //writer.WriteAttributeString("age", "42");
                //writer.WriteString("John Doe");
                //writer.WriteEndElement();

                //writer.WriteStartElement("user");
                //writer.WriteAttributeString("age", "39");
                //writer.WriteString("Jane Doe");

                await writer.WriteEndElementAsync();
                progress.Report(50);

                await writer.WriteEndDocumentAsync();

                writer.Close();
            }
        }

        
    }

    static class XmlWriterExtensions
    {
        public async static Task WriteStartElementAsync(this XmlWriter writer, string localName)
        {
            var task = writer.WriteStartElementAsync(null, localName, null);
            await task;
        }
    }
}
