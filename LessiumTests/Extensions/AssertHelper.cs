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

        public static void AreEqual(SerializedLessonModel expected, SerializedLessonModel actual)
        {
            CheckParametersForAreEqual(expected, actual);

            AreEqual(expected.MaterialSections, actual.MaterialSections);
            AreEqual(expected.TestSections, actual.TestSections);
        }

        private static void AreEqual(Collection<Section> expected, Collection<Section> actual)
        {
            // Checks size first

            Assert.AreEqual(expected.Count, actual.Count);

            // Iterates over Sections

            for (int s = 0; s < expected.Count; s++)
            {
                var expectedSection = expected[s];
                var actualSection = actual[s];

                /// Explanation: Asserting implemented through SerializationInfo and GetObjectData.
                /// Why? - Simple and efficient way to check values which should be equal.
                /// Because Section is Control, and Control won't be the same,
                /// WPF uses them for rendering and so on, that's why.
                /// So with this approach, we can just check values which are used for Serialization.
                /// This way, nothing except them will be checked so Assert will be done on them properly.
                /// More info on my blog: https://simpleandefficient.wordpress.com - Asserting WPF Controls

                // Creates SerializationInfo for expectedSection and fills it with Section's data.

                var expectedInfo = new SerializationInfo(typeof(Section), converter);
                expectedSection.GetObjectData(expectedInfo, context);

                // Same but for actualSection

                var actualInfo = new SerializationInfo(typeof(Section), converter);
                actualSection.GetObjectData(actualInfo, context);

                // Enumerates pages with same approach.

                EnumeratePages(expectedInfo, actualInfo);
            }
        }

        private static void EnumeratePages(SerializationInfo expectedInfo, SerializationInfo actualInfo)
        {
            var enumerator = expectedInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "Pages" && enumerator.ObjectType == typeof(List<ContentPage>))
                {
                    var expectedPages = (List<ContentPage>)enumerator.Value;
                    var actualPages = (List<ContentPage>)actualInfo.GetValue(enumerator.Name, enumerator.ObjectType);

                    // Asserts size

                    Assert.AreEqual(expectedPages.Count, actualPages.Count);

                    // Iterates over Pages

                    for (int p = 0; p < expectedPages.Count; p++)
                    {
                        var expectedPage = expectedPages[p];
                        var actualPage = actualPages[p];

                        var expectedPageInfo = new SerializationInfo(typeof(ContentPage), converter);
                        expectedPage.GetObjectData(expectedPageInfo, context);

                        var actualPageInfo = new SerializationInfo(typeof(ContentPage), converter);
                        actualPage.GetObjectData(actualPageInfo, context);

                        EnumerateContent(expectedPageInfo, actualPageInfo);
                    }
                }

                else
                {
                    var expectedValue = expectedInfo.GetValue(enumerator.Name, enumerator.ObjectType);
                    var actualValue = actualInfo.GetValue(enumerator.Name, enumerator.ObjectType);

                    Assert.AreEqual(expectedValue, actualValue);
                }
            }
        }

        private static void EnumerateContent(SerializationInfo expectedInfo, SerializationInfo actualInfo)
        {
            var enumerator = expectedInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                // Checks if entry is type which can be enumerated (for example: List)
                if (enumerator.ObjectType.IsAssignableFrom(typeof(IEnumerable)))
                {
                    var enumerable = (IEnumerable)enumerator.Value;
                    var contentItemsEnumerator = enumerable.GetEnumerator();
                    while(contentItemsEnumerator.MoveNext())
                    {
                        // TODO:
                    }
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
            if (ReferenceEquals(expected, actual)) throw new InvalidOperationException("Both expected and actual are the same reference!");
            if (expected == null) throw new ArgumentNullException("expected");
            if (actual == null) throw new ArgumentNullException("actual");
        }
    }
}
