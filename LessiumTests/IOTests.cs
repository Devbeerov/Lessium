using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lessium.Classes.IO;
using System.IO;
using Lessium.ContentControls;
using System;
using LessiumTests.Extensions;
using Lessium.Utility;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

namespace LessiumTests
{
    [TestClass]
    public class IOTests
    {
        private readonly string filePath = Path.Combine("lessons", "test2.lsn");

        #region TestMethods

        [TestMethod]
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

        [TestMethod]
        public void TestLoad()
        {
            var taskCount = LsnReader.CountData(filePath);
            var countData = taskCount.Result; // Will block until get result.

            // Creating STA thread for ProgressWindow
            Thread t = new Thread(() =>  {
                var progressView = IOTools.CreateProgressView(null, "TestLoad", countData, IOType.Read);
                var progress = IOTools.CreateProgressForProgressView(progressView);
                progressView.Show();

                try
                {
                    var task = LsnReader.LoadAsync(filePath, progress); // Nuget.System.ValueTuple
                    var result = task.Result; // Will block until get result.
                }

                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                Dispatcher.Run(); // required to make it work.
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
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

        #endregion

        //#region Section 0

        //var firstSection = new Section(type, false);
        //firstSection.SetTitle("Раздел 1");

        //#region Page 0

        //var firstPage = new ContentPage(type);

        //// Two same TextControls
        //firstPage.Add(
        //    new TextControl() { Text = "Введите текст здесь." }
        //);
        //firstPage.Add(
        //    new TextControl() { Text = "Введите текст здесь." }
        //);

        //#endregion

        //firstSection.Add(firstPage);

        //#endregion

        //#region Section 1

        //var secondSection = new Section(type, false);
        //secondSection.SetTitle("Раздел 2");

        //#region Page 1

        //var secondPage = new ContentPage(type);

        //#endregion

        //secondSection.Add(secondPage);

        //#endregion

    }
}
