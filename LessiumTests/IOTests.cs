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
using System.Threading.Tasks;
using Lessium.Classes.Wrappers;
using System.Windows.Threading;
using System.Diagnostics;

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
        public async Task TestLoad()
        {
            // Should be called to avoid deadlock.

            SetupTestDispatcher();

            // Creates expected Lesson's Model

            var expectedLessonModel = DispatcherUtility.Dispatcher.Invoke(() => CreateExpectedLessonModel());

            // Checks if code doesn't throws any exceptions during load, and also Asserts results.

            (IOResult, LessonModel)? loadResult = null;

            try
            {
                loadResult = await IOTools.LoadLesson(filePath);
            }

            catch (Exception e)
            {
                Assert.Fail("Exception occured during loading: {0}", e.ToString());
            }

            ShutdownDispatcher();

            Assert.AreEqual(true, loadResult.HasValue);

            // Asserts results

            Assert.AreEqual(IOResult.successful, loadResult.Value.Item1);
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
            // Should be called to avoid deadlock.

            SetupTestDispatcher();

            // Loads Lesson and retrieves it's LessonModel

            var loadResult = await IOTools.LoadLesson(filePath);
            var serializedModel = loadResult.Item2;

            // Saving

            IOResult saveResult = IOResult.Null;

            try
            {
                var countData = await LsnWriter.CountDataAsync(serializedModel, filePath);

                // New window

                var progressView = IOTools.CreateProgressView(null, "TestSave", countData, IOType.Write);
                var progress = IOTools.CreateProgressForProgressView(progressView);

                progressView.Show();

                // Awaits until SaveAsync method is completed asynchronously.

                saveResult = await Task.Run(async () => await LsnWriter.SaveAsync(serializedModel, tempPath, progress));

                // Closes window

                progressView.Close();
            }

            catch (Exception e)
            {
                Assert.Fail("Exception occured during saving: {0}", e.ToString());
            }
            
            Assert.AreEqual(IOResult.successful, saveResult);

            var tempLoadResult = await IOTools.LoadLesson(tempPath);
            var tempSerializedModel = tempLoadResult.Item2;

            ShutdownDispatcher();

            // Asserts temp file is loaded successfully and serializedModel is the same.

            Assert.AreEqual(IOResult.successful, tempLoadResult.Item1);
            AssertHelper.AreEqual(serializedModel, tempSerializedModel);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task TestBigLoad()
        {
            // Should be called to avoid deadlock.

            SetupTestDispatcher();

            // Check if loaded large file without exceptions. For more detail testing of Load, use TestLoad().

            try
            {
                await IOTools.LoadLesson(bigLessonPath);
            }

            catch (Exception e)
            {
                Assert.Fail("Exception occured during loading: {0}", e.ToString());
            }

            ShutdownDispatcher();
        }

        #endregion

        #region Private Methods

        private void SetupTestDispatcher()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            Thread thr = new Thread(() =>
            {
                DispatcherUtility.Dispatcher = new DispatcherWrapper(Dispatcher.CurrentDispatcher);

                resetEvent.Set();
                while (!DispatcherUtility.Dispatcher.GetUnderlyingDispatcher().HasShutdownStarted)
                {
                    Dispatcher.Run();
                }

                Dispatcher.ExitAllFrames();
                Debug.WriteLine("[INFO] Dispatcher thread terminated.");
            });

            thr.IsBackground = false;
            thr.SetApartmentState(ApartmentState.STA);
            thr.Start();

            resetEvent.WaitOne();

            var context = new DispatcherSynchronizationContext(DispatcherUtility.Dispatcher.GetUnderlyingDispatcher());
            SynchronizationContext.SetSynchronizationContext(context);
        }

        private void ShutdownDispatcher()
        {
            DispatcherUtility.Dispatcher.GetUnderlyingDispatcher().InvokeShutdown();
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
                        var firstPage = new ContentPageModel(ContentType.Material);

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
                        var firstPage = new ContentPageModel(ContentType.Test);

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
