using Lessium.ContentControls;
using Lessium.ContentControls.Models;
using Lessium.Utility;
using Lessium.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

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
                var task = LoadInternal(fileName, cts.Token, progress);

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

        private static async Task<SerializedLessonModel> LoadInternal(string fileName, CancellationToken token, IProgress<int> progress)
        {
            var model = new SerializedLessonModel();

            using (XmlReader reader = XmlReader.Create(fileName, settings))
            {
                token.ThrowIfCancellationRequested();
                while(await reader.ReadAsync())
                {
                    if(token.IsCancellationRequested) { break; }

                    #region Lesson

                    switch(reader.NodeType)
                    {
                        case XmlNodeType.Element:

                            if(reader.Name == "Materials" || reader.Name == "Tests")
                            {
                                var type = (SectionType)Enum.Parse(typeof(SectionType), reader.Name);
                                var sections = await ReadTab(reader.ReadSubtree(), token, progress, type);

                                if(type == SectionType.MaterialSection)
                                {
                                    model.MaterialSections.AddRange(sections);
                                }

                                else
                                {
                                    model.TestSections.AddRange(sections);
                                }
                            }
                            break;
                    }

                    #endregion
                }

                reader.Close();
            }

            return model;
        }

        private static async Task<Collection<Section>> ReadTab(XmlReader reader, CancellationToken token, IProgress<int> progress,
            SectionType type)
        {
            var sections = new Collection<Section>();
            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Section")
                {
                    sections.Add(await ReadSection(reader.ReadSubtree(), token, progress, type));
                }
            }
            return sections;
        }

        private static async Task<Section> ReadSection(XmlReader reader, CancellationToken token, IProgress<int> progress,
            SectionType type)
        {
            var section = new Section(type);

            while(await reader.ReadAsync())
            {
                if(reader.NodeType == XmlNodeType.Element && reader.Name == "Page")
                {
                    var page = await ReadPage(reader.ReadSubtree(), token, progress);
                    section.Add(page);
                }
            }

            if(section.GetPages().Count == 0)
            {
                throw new InvalidOperationException("Section must have at least 1 Page. Something is wrong with Section.");
            }

            return section;
        }

        private static async Task<ContentPage> ReadPage(XmlReader reader, CancellationToken token, IProgress<int> progress)
        {
            return null; //TODO:
        }

            //var t = Type.GetType($"Lessium.ContentControls.MaterialControls.{"TextControl"}");
            //var ob = Activator.CreateInstance(t);
    }

    public class SerializedLessonModel
    {
        public Collection<Section> MaterialSections { get; private set; } = new Collection<Section>();
        public Collection<Section> TestSections { get; private set; } = new Collection<Section>();
    }
}
