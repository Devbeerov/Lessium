using Lessium.Classes.IO;
using Lessium.ContentControls;
using Lessium.ContentControls.Models;
using Lessium.Interfaces;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Linq;

namespace LessiumTests.Extensions
{
    public static class AssertHelper
    {
        private static FormatterConverter converter = new FormatterConverter();
        private static StreamingContext context = new StreamingContext();

        public static void AreEqual(CountData expected, CountData actual)
        {
            CheckParametersForAreEqual(expected, actual);

            var expectedSectionsCount = expected.GetSectionsCount();
            var actualSectionsCount = actual.GetSectionsCount();

            Assert.AreEqual(expectedSectionsCount, actualSectionsCount);

            for (int i = 0; i < expectedSectionsCount; i++)
            {
                var expectedPagesCount = expected.GetPagesCount(i);
                var actualPagesCount = actual.GetPagesCount(i);

                Assert.AreEqual(expectedPagesCount, actualPagesCount);

                for (int j = 0; j < expectedPagesCount; j++)
                {
                    var expectedContentCount = expected.GetContentsCount(i, j);
                    var actualContentCount = actual.GetContentsCount(i, j);

                    Assert.AreEqual(expectedContentCount, actualContentCount);
                }
            }
        }

        /// <summary>
        /// Asserts each Section in each Tab, therefore all it's items using the Polymorphism.
        /// </summary>
        public static void AreEqual(LessonModel expected, LessonModel actual)
        {
            CheckParametersForAreEqual(expected, actual);
            
            AreEqual(expected.MaterialSections, actual.MaterialSections);
            AreEqual(expected.TestSections, actual.TestSections);
        }

        public static void AreEqual(IList<Section> expected, IList<Section> actual)
        {
            CheckParametersForAreEqual(expected, actual);

            // Checks size first

            Assert.AreEqual(expected.Count, actual.Count);

            // Iterates over Sections

            for (int s = 0; s < expected.Count; s++)
            {
                var expectedSection = expected[s];
                var actualSection = actual[s];

                AreEqual(expectedSection, actualSection);
            }
        }

        public static void AreEqual(Section expected, Section actual)
        {
            CheckParametersForAreEqual(expected, actual);

            /// Explanation: Asserting implemented through SerializationInfo and GetObjectData.
            /// Why? - Simple and efficient way to check values which should be equal.
            /// Because Section is Control, and Control won't be the same,
            /// WPF uses them for rendering and so on, that's why.
            /// So with this approach, we can just check values which are used for Serialization.
            /// This way, nothing except them will be checked so Assert will be done on them properly.
            /// More info on my blog: https://simpleandefficient.wordpress.com - Asserting WPF Controls

            // Creates SerializationInfo for expectedSection and fills it with Section's data.

            var expectedInfo = new SerializationInfo(expected.GetType(), converter);
            expected.GetObjectData(expectedInfo, context);

            // Same but for actualSection

            var actualInfo = new SerializationInfo(actual.GetType(), converter);
            actual.GetObjectData(actualInfo, context);

            // Enumerates stored object's data.

            var enumerator = expectedInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                // If it's Pages entry, we check collection with custom AreEqual.

                if (enumerator.Name == "Pages" && enumerator.Value is IList)
                {
                    var expectedPages = (IList<ContentPage>)enumerator.Value;
                    var actualPages = (IList<ContentPage>)actualInfo.GetValue(enumerator.Name, enumerator.ObjectType);

                    AreEqual(expectedPages, actualPages);
                }

                // Otherwise, uses Assert's AreEqual for them.

                else
                {
                    var expectedValue = expectedInfo.GetValue(enumerator.Name, enumerator.ObjectType);
                    var actualValue = actualInfo.GetValue(enumerator.Name, enumerator.ObjectType);

                    Assert.AreEqual(expectedValue, actualValue);
                }
            }
        }

        public static void AreEqual(IList<ContentPage> expected, IList<ContentPage> actual)
        {
            CheckParametersForAreEqual(expected, actual);

            // Asserts size

            Assert.AreEqual(expected.Count, actual.Count);

            // Iterates over Pages

            for (int p = 0; p < expected.Count; p++)
            {
                var expectedPage = expected[p];
                var actualPage = actual[p];

                AreEqual(expectedPage, actualPage);
            }
        }

        public static void AreEqual(ContentPage expected, ContentPage actual)
        {
            CheckParametersForAreEqual(expected, actual);

            // Asserts size

            Assert.AreEqual(expected.Items.Count, actual.Items.Count);

            var expectedInfo = new SerializationInfo(expected.GetType(), converter);
            expected.GetObjectData(expectedInfo, context);

            var actualInfo = new SerializationInfo(actual.GetType(), converter);
            actual.GetObjectData(actualInfo, context);

            var enumerator = expectedInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "Items" && enumerator.Value is IList<IContentControl>)
                {
                    var expectedItems = enumerator.Value as IEnumerable<ILsnSerializable>;
                    var actualItems = actualInfo.GetValue(enumerator.Name, enumerator.ObjectType) as IEnumerable<ILsnSerializable>;

                    AreEqual(expectedItems.ToList(), actualItems.ToList());
                }

                else
                {
                    var expectedValue = expectedInfo.GetValue(enumerator.Name, enumerator.ObjectType);
                    var actualValue = actualInfo.GetValue(enumerator.Name, enumerator.ObjectType);

                    Assert.AreEqual(expectedValue, actualValue);
                }
            }
        }

        public static void AreEqual(IList<ILsnSerializable> expected, IList<ILsnSerializable> actual)
        {
            CheckParametersForAreEqual(expected, actual);

            // Asserts size

            Assert.AreEqual(expected.Count, actual.Count);

            // Iterates over Controls

            for (int c = 0; c < expected.Count; c++)
            {
                var expectedControl = expected[c];
                var actualControl = actual[c];

                AreEqual(expectedControl, actualControl);
            }
        }

        // If unique AreEqual is not exist for particular ContentControl, we use this method.
        public static void AreEqual(ILsnSerializable expected, ILsnSerializable actual)
        {
            CheckParametersForAreEqual(expected, actual);

            var expectedInfo = new SerializationInfo(expected.GetType(), converter);
            expected.GetObjectData(expectedInfo, context);

            var actualInfo = new SerializationInfo(actual.GetType(), converter);
            actual.GetObjectData(actualInfo, context);

            var enumerator = expectedInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var expectedItems = enumerator.Value as IEnumerable<ILsnSerializable>;

                // Checks if entry can be casted to IEnumerable<ILsnSerializable>

                if (expectedItems != null)
                {
                    var actualItems = actualInfo.GetValue(enumerator.Name, enumerator.ObjectType) as IEnumerable<ILsnSerializable>;

                    AreEqual(expectedItems.ToList(), actualItems.ToList());
                }

                else
                {
                    var expectedValue = expectedInfo.GetValue(enumerator.Name, enumerator.ObjectType);
                    var actualValue = actualInfo.GetValue(enumerator.Name, enumerator.ObjectType);

                    Assert.AreEqual(expectedValue, actualValue);
                }
            }
        }

        private static void CheckParametersForAreEqual(object expected, object actual)
        {
            if (expected == null) throw new ArgumentNullException("expected");
            if (actual == null) throw new ArgumentNullException("actual");
            if (ReferenceEquals(expected, actual)) throw new InvalidOperationException("Both expected and actual are the same reference!");
        }
    }
}
