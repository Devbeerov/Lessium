using Lessium.ContentControls;
using Lessium.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Schema;

namespace Lessium.Classes.IO
{
    public static class LsnReader
    {
        private static CancellationTokenSource cts;
        private static bool canceledManually;
        private static bool disposed = true; // Not initialized yet, so we can count it as disposed, considering design of class.

        private static XmlReaderSettings settings = new XmlReaderSettings()
        {
            Async = true,
            ValidationType = ValidationType.Schema,
        };

        static LsnReader()
        {
            try
            {
                settings.Schemas.Add("", Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "data", "lsn.xsd"));
            }

            catch (XmlSchemaException e)
            {
                Debug.WriteLine($"[CRITICAL ERROR] lsn.xsd is corrupted. Line {e.LineNumber} position {e.LinePosition}{Environment.NewLine}{e.Message}");
            }
        }

        public static void Cancel()
        {
            if(!disposed)
            {
                cts.Cancel();
            }
        }

        public async static Task<(IOResult, SerializedLessonModel)> LoadAsync(string fileName, IProgress<ProgressType> progress)
        {
            canceledManually = false;
            cts = new CancellationTokenSource();
            disposed = false;
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
                        Debug.WriteLine($"Error while loading file - {e.ToString()}");
                    }
                }

                finally
                {
                    disposed = true;
                }

                if (task.IsCompleted)
                {
                    result = IOResult.Sucessful;
                    model = task.Result;
                }
            }

            disposed = true;
            cts = null;

            return (result, model);
        }

        private static async Task<SerializedLessonModel> LoadInternalAsync(string fileName, CancellationToken token, IProgress<ProgressType> progress)
        {
            token.ThrowIfCancellationRequested();

            var model = new SerializedLessonModel();
            using (XmlReader reader = XmlReader.Create(fileName, settings))
            {
                try
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
                }

                catch (XmlSchemaValidationException e)
                {
                    Debug.WriteLine($"Line {e.LineNumber} position {e.LinePosition} caused validation error.{Environment.NewLine}{e.Message}");
                }

                finally
                {
                    reader.Close();
                }
            }

            token.ThrowIfCancellationRequested();

            return model;
        }

        private static async Task CountPages(XmlReader reader, CountData data, int sectionIndex, CancellationToken token)
        {
            int pageIndex = 0;

            while (await reader.ReadToFollowingAsync("Page") && reader.NodeType == XmlNodeType.Element)
            {
                if (token.IsCancellationRequested) break;

                data.AddPage(sectionIndex, pageIndex, await reader.CountChildsAsync());
                pageIndex++;
            }
        }

        private static async Task CountSections(XmlReader reader, CountData data, CancellationToken token)
        {
            int sectionIndex = 0;

            while (await reader.ReadToFollowingAsync("Section") && reader.NodeType == XmlNodeType.Element)
            {
                if (token.IsCancellationRequested) break;

                data.AddSection(sectionIndex);
                await CountPages(reader.ReadSubtree(), data, sectionIndex, token);
                sectionIndex++;
            }
        }

        /// <summary>
        /// Iterates over Lsn file to create CountData for specified fileName.
        /// </summary>
        public static async Task<Dictionary<ContentType, CountData>> CountData(string fileName)
        {
            cts = new CancellationTokenSource();
            var token = cts.Token;
            Dictionary<ContentType, CountData> result = new Dictionary<ContentType, CountData>();
 
            using (XmlReader reader = XmlReader.Create(fileName, settings))
            {
                try
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
                }

                catch (XmlSchemaValidationException e)
                {
                    Debug.WriteLine($"Line {e.LineNumber} position {e.LinePosition} caused validation error.{Environment.NewLine}{e.Message}");
                }

                finally
                {
                    reader.Close();
                }
            }

            cts = null;

            return result;
        }

        private static async Task<Collection<Section>> ReadTab(XmlReader reader, CancellationToken token, IProgress<ProgressType> progress,
            ContentType type)
        {
            var sections = new Collection<Section>();

            while (await reader.ReadToFollowingAsync("Section") && reader.NodeType == XmlNodeType.Element)
            {
                // Creates Section instance, but not initializing it yet.

                var section = new Section(type, false);
                await section.ReadXmlAsync(reader, progress, token);

                // Now after loading XML, we can initialize Section properly.

                section.Initialize();
                sections.Add(section);
            }

            // Reports that current Tab is read.

            progress.Report(ProgressType.Tab);

            // Returns collection of Sections relative to this ContentType (tab).

            return sections;
        }

        //private static async Task ExecuteOnUIThreadAsync(Delegate code, params object[] args)
        //{
        //    Debugger.Break();
        //    await uiDispatcher.BeginInvoke(code, DispatcherPriority.Send, args);
        //}

        //private static async Task ExecuteOnUIThreadAsync(Func<Task> code)
        //{
        //    Debugger.Break();
        //    await uiDispatcher.InvokeAsync(code);
        //}

        //private static async Task ExecuteOnUIThreadAsync(/*Func<XmlReader, CancellationToken, IProgress<ProgressType>, ContentType, Collection<Section>, Task> */code)
        //{
        //    await uiDispatcher.InvokeAsync(code, DispatcherPriority.Normal, cts.Token);
        //}

        //private static async Task ExecuteOnUIThreadAsync(Action code)
        //{
        //    Debugger.Break();
        //    try
        //    {
        //        await uiDispatcher.InvokeAsync(code, DispatcherPriority.Normal, cts.Token);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e.ToString());
        //    }
        //}
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
        public int GetSectionsCount()
        {
            return data.Keys.Count;
        }

        public int GetPagesCount(int sectionIndex)
        {
            return data[sectionIndex].Keys.Count;
        }

        public int GetContentsCount(int sectionIndex, int pageIndex)
        {
            return data[sectionIndex][pageIndex];
        }

        public void AddSection(int sectionIndex)
        {
            data.Add(sectionIndex, new Dictionary<int, int>());
        }

        public void AddPage(int sectionIndex, int pageIndex, int contentAmount)
        {
            data[sectionIndex].Add(pageIndex, contentAmount);
        }


        /// <summary>
        /// (int) index of Section
        /// (Dictionary) Section's content by Page.
        /// 1st - PageIndex
        /// 2nd - Amount of ContentControls which specified Page contains.
        /// </summary>
        private readonly Dictionary<int, Dictionary<int, int>> data = new Dictionary<int, Dictionary<int, int>>();
    }

    public enum ProgressType
    {
        Tab, Section, Page, Content
    }
}
