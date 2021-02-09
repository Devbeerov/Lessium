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
            token.ThrowIfCancellationRequested();

            var model = new SerializedLessonModel();

            using (XmlReader reader = XmlReader.Create(fileName, settings))
            {
                while (await reader.ReadAsync())
                {
                    if (token.IsCancellationRequested) break;

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

            while (await reader.ReadToDescendantAsync("Page"))
            {
                if (token.IsCancellationRequested) break;

                data.ContentCount.Add(pageIndex, await reader.CountChildsAsync());
                pageIndex++;
            }

            /// If we increment sectionIndex by one (pageIndex++) at the end of cycle,
            /// it will show actual count amount before next iteration, so no need for pageIndex + 1.
            
            data.PageCount.Add(sectionIndex, pageIndex);
            
        }

        private static async Task CountSections(XmlReader reader, CountData data, CancellationToken token)
        {
            int sectionIndex = 0;

            while (await reader.ReadToDescendantAsync("Section"))
            {
                if (token.IsCancellationRequested) break;

                await CountPages(reader.ReadSubtree(), data, sectionIndex, token);
                sectionIndex++;
            }

            /// If we increment sectionIndex by one (sectionIndex++) at the end of cycle,
            /// it will show actual count amount before next iteration, so no need for sectionIndex + 1.
            
            data.SectionCount = sectionIndex;
        }

        /// <summary>
        /// Iterates over Lsn file to create CountData for specified fileName.
        /// </summary>
        public static async Task<Dictionary<ContentType, CountData>> CountData(string fileName)
        {
            cts = new CancellationTokenSource();
            var token = cts.Token;
            Dictionary<ContentType, CountData> result = new Dictionary<ContentType, CountData>();

            try
            {
                using (XmlReader reader = XmlReader.Create(fileName, settings))
                {
                    // Reads materials
                    if (!token.IsCancellationRequested)
                    {
                        var materialsData = new CountData();
                        await reader.ReadToFollowingAsync("Materials");
                        await CountSections(reader.ReadSubtree(), materialsData, token);

                        result.Add(ContentType.Material, materialsData);
                    }

                    // Reads tests
                    if (!token.IsCancellationRequested)
                    {
                        var testsData = new CountData();
                        await reader.ReadToFollowingAsync("Tests");
                        await CountSections(reader.ReadSubtree(), testsData, token);

                        result.Add(ContentType.Test, testsData);
                    }

                    if (token.IsCancellationRequested) result = null;

                    reader.Close();
                }
            }

            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            cts = null;

            return result;
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

            // Reports that current Tab is read.

            progress.Report(ProgressType.Tab);

            // Returns collection of Sections relative to this ContentType (tab).

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
        public int SectionCount { get; set; } = 0;

        /// <summary>
        /// 1st - SectionIndex
        /// 2nd - Amount of pages which specified Section contains.
        /// </summary>
        public Dictionary<int, int> PageCount { get; private set; } = new Dictionary<int, int>();

        /// <summary>
        /// 1st - PageIndex
        /// 2nd - Amount of ContentControls which specified Page contains.
        /// </summary>
        public Dictionary<int, int> ContentCount { get; private set; } = new Dictionary<int, int>();
    }

    public enum ProgressType
    {
        Tab, Section, Page, Content
    }
}
