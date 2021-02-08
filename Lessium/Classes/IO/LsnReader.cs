using Lessium.ContentControls;
using Lessium.Utility;
using System;
using System.Collections.Generic;
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

        public async static Task<(IOResult, SerializedLessonModel)> LoadAsync(string fileName, IProgress<ProgressType> progress)
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

        private static async Task<SerializedLessonModel> LoadInternalAsync(string fileName, CancellationToken token, IProgress<ProgressType> progress)
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

        private static async Task CountPages(XmlReader reader, CountData data, int sectionIndex, CancellationToken token)
        {
            int pageIndex = 0;

            while (await reader.ReadAsync())
            {
                if (token.IsCancellationRequested) break;

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Page")
                {
                    data.ContentCount[pageIndex] = await reader.CountChildsAsync();
                    pageIndex++;
                }

                data.PageCount[sectionIndex] += pageIndex + 1;
            }
        }

        private static async Task CountSections(XmlReader reader, CountData data, ContentType type, CancellationToken token)
        {
            int sectionIndex = 0;

            while (await reader.ReadAsync())
            {
                if (token.IsCancellationRequested) break;

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Section")
                {
                    await CountPages(reader.ReadSubtree(), data, sectionIndex, token);
                    sectionIndex++;
                }

                data.SectionCount[type] += sectionIndex + 1;
            }
        }

        /// <summary>
        /// Iterates over Lsn file to create CountData for specified fileName.
        /// </summary>
        public static async Task<CountData> CountData(string fileName)
        {
            cts = new CancellationTokenSource();
            var token = cts.Token;
            CountData data = new CountData();

            using (XmlReader reader = XmlReader.Create(fileName, settings))
            {
                while (await reader.ReadAsync())
                {
                    if(token.IsCancellationRequested)
                    {
                        break;
                    }

                    // Reads materials

                    await reader.ReadToFollowingAsync("Materials");
                    await CountSections(reader.ReadSubtree(), data, ContentType.Material, token);

                    // Reads tests

                    await reader.ReadToFollowingAsync("Tests");
                    await CountSections(reader.ReadSubtree(), data, ContentType.Test, token);
                }

                if (token.IsCancellationRequested) data = null;

                reader.Close();
            }

            cts = null;

            return data;
        }

        private static async Task<Collection<Section>> ReadTab(XmlReader reader, CancellationToken token, IProgress<ProgressType> progress,
            ContentType type)
        {
            var sections = new Collection<Section>();
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Section")
                {
                    // Creates Section instance, but not initializing it yet.

                    var section = new Section(type, false);

                    await section.ReadXmlAsync(reader, progress, token);

                    // Now after loading XML, we can initialize Section properly.

                    section.Initialize();
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

    /// <summary>
    /// Contains all required data for ProgressWindowViewModel. 
    /// NOTE: Should be class instead of struct, because it contains dictionaries.
    /// </summary>
    public class CountData
    {
        public Dictionary<ContentType, int> SectionCount { get; private set; }

        /// <summary>
        /// 1st - SectionIndex
        /// 2nd - Amount of pages which specified Section contains.
        /// </summary>
        public Dictionary<int, int> PageCount { get; private set; }

        /// <summary>
        /// 1st - PageIndex
        /// 2nd - Amount of ContentControls which specified Page contains.
        /// </summary>
        public Dictionary<int, int> ContentCount { get; private set; }
    }

    public enum ProgressType
    {
        Section, Page, Content
    }
}
