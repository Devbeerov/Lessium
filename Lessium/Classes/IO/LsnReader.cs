using Lessium.ContentControls;
using Lessium.Utility;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lessium.Classes.IO
{
    public static class LsnReader
    {
        private static CancellationTokenSource cts;
        private static bool canceledManually;

        private static XmlReaderSettings settings = new XmlReaderSettings()
        {
            Async = true,
            ValidationType = ValidationType.Schema,
        };

        static LsnReader()
        {
            settings.Schemas.Add("", Path.Combine("data","lsn.xsd"));
        }

        public static void Cancel()
        {
            cts?.Cancel();
        }

        public async static Task<(IOResult, SerializedLessonModel)> LoadAsync(string fileName, IProgress<int> progress)
        {
            canceledManually = false;
            cts = new CancellationTokenSource();
            var result = IOResult.Null;
            SerializedLessonModel model = null;

            using (cts)
            {
                var task = LoadInternalAsync(fileName, cts.Token, progress);

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
                        Console.WriteLine($"Error while loading file - {e.ToString()}");
                    }
                }

                if (task.IsCompleted)
                {
                    result = IOResult.Sucessful;
                    model = task.Result;
                }
            }

            cts = null;

            return (result, model);
        }

        private static async Task<SerializedLessonModel> LoadInternalAsync(string fileName, CancellationToken token, IProgress<int> progress)
        {
            /// NOTE: Unlike in SaveInternalAsync, we always call token.ThrowIfCancellationRequested().
            /// We could save file incompletly, but still compatible, BUT NOT LOAD FILE IF ITS CANCELLED DURING READING!
            /// So in case of cancellation, we throw as soon as possible.
            
            token.ThrowIfCancellationRequested();

            var model = new SerializedLessonModel();

            using (XmlReader reader = XmlReader.Create(fileName, settings))
            {
                while (await reader.ReadAsync())
                {
                    token.ThrowIfCancellationRequested();

                    #region Lesson

                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "Materials":
                                {
                                    var sections = await ReadTab(reader.ReadSubtree(), token, progress, ContentType.Material);
                                    model.MaterialSections.AddRange(sections);
                                    break;
                                }
                            case "Tests":
                                {
                                    var sections = await ReadTab(reader.ReadSubtree(), token, progress, ContentType.Test);
                                    model.TestSections.AddRange(sections);
                                    break;
                                }
                        }
                    }

                    #endregion
                }

                reader.Close();
            }

            token.ThrowIfCancellationRequested();

            return model;
        }

        /// <summary>
        /// Iterates over Lsn file to get total pages count
        /// </summary>
        public static async Task<int> CountPages(string fileName)
        {
            cts = new CancellationTokenSource();
            bool emptyLesson = true;
            int pageCount = 0;

            using (XmlReader reader = XmlReader.Create(fileName, settings))
            {
                while (await reader.ReadAsync())
                {
                    if(cts.Token.IsCancellationRequested)
                    {
                        pageCount = -1;
                        break;
                    }

                    if (await reader.ReadToFollowingAsync("Section")) // Advances forward next Section, no matter which tab.
                    {
                        // Contains at least one Section
                        emptyLesson = false;
                    }

                    pageCount += await reader.CountChildsAsync();
                }
                reader.Close();
            }

            cts = null;

            if(pageCount < 1 && !emptyLesson) { throw new InvalidDataException("Non-empty Lesson should contain at least one page!"); }

            return pageCount;
        }

        private static async Task<Collection<Section>> ReadTab(XmlReader reader, CancellationToken token, IProgress<int> progress,
            ContentType type)
        {
            var sections = new Collection<Section>();
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Section")
                {
                    var section = new Section(type, Section.InitializationType.Mandatory);

                    // Do not pass reader.ReadSubstree() here! ReadSection will get Section title and handle further reading.
                    await section.ReadXmlAsync(reader, token, progress);

                    // Continues section initialization.

                    section.Initialize(Section.InitializationType.Pages);
                        
                    // Now we can be sure that section is fully initialized.

                    sections.Add(section);
                }
            }
            return sections;
        }
    }

    public class SerializedLessonModel
    {
        public Collection<Section> MaterialSections { get; private set; } = new Collection<Section>();
        public Collection<Section> TestSections { get; private set; } = new Collection<Section>();
    }
}
