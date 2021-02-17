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

namespace LessiumTests
{
    [TestFixture]
    public class IOTests
    {
        private readonly string filePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "lessons", "test2.lsn");

        #region TestMethods

        [Test]
        public void TestCountData()
        {
            var task = LsnReader.CountData(filePath);
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

            Assert.DoesNotThrowAsync(async () =>
            {
                var countData = await LsnReader.CountData(filePath);

                // Creates ProgressView

                var progressView = IOTools.CreateProgressView(null, "TestLoad", countData, IOType.Read);
                var progress = IOTools.CreateProgressForProgressView(progressView);

                progressView.Show();

                // Tests Lesson's loading, in case it will throw any Exceptions during load,
                // Assert.DoesNotThrowAsync will fail test.

                var loadResult = await LsnReader.LoadAsync(filePath, progress); // Nuget.System.ValueTuple

                // Asserts results

                Assert.AreEqual(IOResult.Sucessful, loadResult.Item1);
                AssertHelper.AreEqual(expectedLessonModel, loadResult.Item2);
            });
        }

        [Test]
        public void TestSave()
        {
            var savedModel = CreateExpectedLessonModel();
        }

        #endregion

        #region Private Methods

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
        private SerializedLessonModel CreateExpectedLessonModel()
        {
            if (filePath.EndsWith("test2.lsn"))
            {
                return CreateExpectedLessonModelFor_test2();
            }

            throw new NotSupportedException($"{filePath} not supported.");
        }

        private SerializedLessonModel CreateExpectedLessonModelFor_test2()
        {
            var model = new SerializedLessonModel();

            #region Materials
            {
                #region Section 0
                {
                    var firstSection = new Section(ContentType.Material, false);
                    firstSection.SetTitle("Раздел 1");

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
                    firstSection.SetTitle("Раздел 2");

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
                    firstSection.SetTitle("Раздел 1");

                    model.TestSections.Add(firstSection);
                }
                #endregion

                #region Section 1
                {
                    var firstSection = new Section(ContentType.Test);
                    firstSection.SetTitle("Раздел 2");

                    model.TestSections.Add(firstSection);
                }
                #endregion

                #region Section 2
                {
                    var firstSection = new Section(ContentType.Test, false);
                    firstSection.SetTitle("Раздел 3");

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
