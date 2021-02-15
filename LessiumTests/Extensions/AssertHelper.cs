using Lessium.Classes.IO;
using NUnit.Framework;
using System;

namespace LessiumTests.Extensions
{
    public static class AssertHelper
    {
        public static void AreEqual(CountData expected, CountData actual)
        {
            if (ReferenceEquals(expected, actual)) throw new InvalidOperationException("Both expected and actual are the same reference!");
            if (expected == null) throw new ArgumentNullException("expected");
            if (actual == null) throw new ArgumentNullException("actual");

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
    }
}
