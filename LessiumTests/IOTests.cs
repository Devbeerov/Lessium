using Lessium.Classes.IO;
using System.IO;
using Lessium.ContentControls;
using System;
using LessiumTests.Extensions;
using Lessium.Utility;
using System.Threading;
using NUnit.Framework;
using Lessium.ContentControls.Models;
using Lessium.ContentControls.MaterialControls;
using Lessium.ContentControls.TestControls;
using Lessium;
using System.Windows;
using System.Threading.Tasks;

namespace LessiumTests
{
    [TestFixture]
    public class IOTests
    {
        private readonly string filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "lessons", "test2.lsn");
        private readonly string tempPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "lessons", "temp.lsn");
        private readonly string bigLessonPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "lessons", "biglesson.lsn");

        public IOTests()
        {
            // Manually creates Lessium App for proper set resources.
            var app = new Lessium.App();
            app.InitializeComponent();
        }

        #region TestMethods

        [Test]
        public void TestCountData()
        {
            var task = LsnReader.CountDataAsync(filePath);
            var result = task.Result; // Will block until get result.

            // Checks data.

            var expectedMaterialData = CreateExpectedCountData(ContentType.Material);
            var expectedTestData = CreateExpectedCountData(ContentType.Test);

            var actualMaterialData = result[ContentType.Material];
            var actualTestData = result[ContentType.Test];

            AssertHelper.AreEqual(expectedMaterialData, actualMaterialData);
            AssertHelper.AreEqual(expectedTestData, actualTestData);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestLoad()
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                throw new ThreadStateException("The current threads apartment state is not STA");
            }

            // Validates Lessium's App. It's required for proper work of Lessium's Views.
            var app = (App) Application.Current;
            if (app == null)
            {
                app = new App();
                app.InitializeComponent();
            }

            // Creates expected Lesson's Model

            var expectedLessonModel = CreateExpectedLessonModel();

            // Checks if code doesn't throws any exceptions during load, and also Asserts results.

            (IOResult, LessonModel)? loadResult = null;

            Assert.DoesNotThrowAsync(async () =>
            {
                loadResult = await IOTools.LoadLesson(filePath);
            });

            Assert.AreEqual(true, loadResult.HasValue);

            // Asserts results

            Assert.AreEqual(IOResult.Sucessful, loadResult.Value.Item1);
            AssertHelper.AreEqual(expectedLessonModel, loadResult.Value.Item2);
        }

        /// <summary>
        /// Loads data from filePath, saves it as filePath_temp and loads saved file and compares data.
        /// </summary>
        /// <returns></returns>
        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task TestSave()
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                throw new ThreadStateException("The current threads apartment state is not STA");
            }

            var loadResult = await IOTools.LoadLesson(filePath);
            var serializedModel = loadResult.Item2;

            IOResult saveResult = IOResult.Null;
            Assert.DoesNotThrowAsync(async () =>
            {
                // New window
                var progressView = IOTools.CreateProgressView(null, "TestSave",
                    await LsnWriter.CountDataAsync(serializedModel, filePath), IOType.Write);
                var progress = IOTools.CreateProgressForProgressView(progressView);

                progressView.Show();

                // Pauses method until SaveAsync method is completed.
                saveResult = await LsnWriter.SaveAsync(serializedModel, tempPath, progress);
            });

            Assert.AreEqual(IOResult.Sucessful, saveResult);

            var tempLoadResult = await IOTools.LoadLesson(tempPath);
            var tempSerializedModel = tempLoadResult.Item2;

            // Asserts temp file is loaded sucessfully and serializedModel is the same.

            Assert.AreEqual(IOResult.Sucessful, tempLoadResult.Item1);
            AssertHelper.AreEqual(serializedModel, tempSerializedModel);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestBigLoad()
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                await IOTools.LoadLesson(bigLessonPath);
            });
        }

        #endregion

        #region Private Methods
        public static Task StartSTATask(Action func)
        {
            var tcs = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
            {
                try
                {
                    func();
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Using current filePath field.
        /// </summary>
        private CountData CreateExpectedCountData(ContentType type)
        {
            if (filePath.EndsWith("test2.lsn"))
            {
                return CreateExpectedCountDataFor_test2(type);
            }

            throw new NotSupportedException($"{filePath} not supported.");
        }

        private CountData CreateExpectedCountDataFor_test2(ContentType type)
        {
            var data = new CountData();
            switch (type)
            {
                case ContentType.Material:
                    {
                        /// 2 Sections
                        /// Section(0) - 1 Page
                        /// Page(0) - 2 Contents
                        /// 
                        /// Section(1) - 1 Page
                        /// Page(0) - 0 Contents

                        data.AddSection(0);
                        data.AddPage(0, 0, 2);

                        data.AddSection(1);
                        data.AddPage(1, 0, 0);
                    }
                    break;

                case ContentType.Test:
                    {
                        /// 3 Sections
                        /// Section(0) - 1 Page
                        /// Page(0) - 0 Contents
                        /// 
                        /// Section(1) - 1 Page
                        /// Page(0) - 0 Contents
                        ///
                        /// Section(2) - 1 Page
                        /// Page(0) - 1 Contents

                        data.AddSection(0);
                        data.AddPage(0, 0, 0);

                        data.AddSection(1);
                        data.AddPage(1, 0, 0);

                        data.AddSection(2);
                        data.AddPage(2, 0, 1);
                    }
                    break;
                default: throw new NotSupportedException($"{type.ToString()} not supported.");
            }
            return data;
        }

        /// <summary>
        /// Using current filePath field.
        /// </summary>
        private LessonModel CreateExpectedLessonModel()
        {
            if (filePath.EndsWith("test2.lsn"))
            {
                return CreateExpectedLessonModelFor_test2();
            }

            throw new NotSupportedException($"{filePath} not supported.");
        }

        private LessonModel CreateExpectedLessonModelFor_test2()
        {
            var model = new LessonModel();

            #region Materials
            {
                #region Section 0
                {
                    var firstSection = new Section(ContentType.Material, false);
                    firstSection.Title = "Раздел 1";

                    #region Page 0
                    {
                        var firstPage = new ContentPage(ContentType.Material);

                        // Two same TextControls
                        firstPage.Add(
                            new TextControl() { Text = "Введите текст здесь." }
                        );
                        firstPage.Add(
                            new TextControl() { Text = "Введите текст здесь." }
                        );

                        firstSection.Add(firstPage);
                        firstSection.Initialize();
                    }
                    #endregion

                    model.MaterialSections.Add(firstSection);
                }
                #endregion

                #region Section 1
                {
                    var firstSection = new Section(ContentType.Material);
                    firstSection.Title = "Раздел 2";

                    model.MaterialSections.Add(firstSection);
                }
                #endregion
            }
            #endregion

            #region Tests
            {
                #region Section 0
                {
                    var firstSection = new Section(ContentType.Test);
                    firstSection.Title = "Раздел 1";

                    model.TestSections.Add(firstSection);
                }
                #endregion

                #region Section 1
                {
                    var firstSection = new Section(ContentType.Test);
                    firstSection.Title = "Раздел 2";

                    model.TestSections.Add(firstSection);
                }
                #endregion

                #region Section 2
                {
                    var firstSection = new Section(ContentType.Test, false);
                    firstSection.Title = "Раздел 3";

                    #region Page 0
                    {
                        var firstPage = new ContentPage(ContentType.Test);

                        firstPage.Add(
                            new SimpleTest()
                            {
                                Question = "Ввеzдите вопрос здесь.",
                                Answers = { new AnswerModel("zОтвет"), new AnswerModel("zОтвет") }
                            }
                        );

                        firstSection.Add(firstPage);
                        firstSection.Initialize();
                    }
                    #endregion

                    model.TestSections.Add(firstSection);
                }
                #endregion

            }
            #endregion

            return model;
        }

        #endregion



    }
}
