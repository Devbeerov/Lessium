using Lessium.Classes.IO;
using Lessium.Models;
using NUnit.Framework;
using System;

namespace LessiumTests.Extensions
{
    public static class AssertHelper
    {
        public static void AreEqual(CountData expected, CountData actual)
        {
            CheckParametersForAreEqual(expected, actual);

            Assert.IsTrue(expected.Equals(actual));
        }

        public static void AreEqual(LessonModel expected, LessonModel actual)
        {
            CheckParametersForAreEqual(expected, actual);

            Assert.IsTrue(expected.Equals(actual));
        }

        private static void CheckParametersForAreEqual(object expected, object actual)
        {
            if (expected == null) throw new ArgumentNullException("expected");
            if (actual == null) throw new ArgumentNullException("actual");
            if (ReferenceEquals(expected, actual)) throw new InvalidOperationException("Both expected and actual are the same reference!");
        }
    }
}
