using Lessium.ContentControls;
using Lessium.ContentControls.MaterialControls;
using Lessium.ContentControls.TestControls;
using Lessium.ContentControls.TestControls.AnswerModels;
using Lessium.Interfaces;
using Lessium.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lessium.Utility
{
    public static class EqualsHelper
    {
        public static bool AreEqual(IList<Section> first, IList<Section> second)
        {
            if (first.Count != second.Count) return false;

            // Iterates over Sections

            for (int i = 0; i < first.Count; i++)
            {
                var firstSection = first[i];
                var secondSection = second[i];

                if (!AreEqual(firstSection, secondSection)) return false;
            }

            return true;
        }

        public static bool AreEqual(IList<ContentPageModel> first, IList<ContentPageModel> second)
        {
            if (first.Count != second.Count) return false;

            // Iterates over Pages

            for (int i = 0; i < first.Count; i++)
            {
                var firstPage = first[i];
                var secondPage = second[i];

                if (!firstPage.Equals(secondPage)) return false;
            }

            return true;
        }

        public static bool AreEqual(Section first, Section second)
        {
            return first.Title == second.Title &&
                   first.ContentType == second.ContentType &&
                   AreEqual(first.Pages, second.Pages);
        }

        public static bool AreEqual(IList<IContentControl> first, IList<IContentControl> second)
        {
            if (first.Count != second.Count) return false;

            // Iterates over Controls

            for (int i = 0; i < first.Count; i++)
            {
                var firstControl = first[i];
                var secondControl = second[i];

                if (!AreEqual(firstControl,secondControl)) return false;
            }

            return true;
        }

        public static bool AreEqual(IContentControl first, IContentControl second)
        {
            var type = first.GetType();
            if (type != second.GetType()) return false;

            switch (first)
            {
                case TextControl firstControl:
                    return AreEqual(firstControl, second as TextControl);

                case SimpleTest firstControl:
                    return AreEqual(firstControl, second as SimpleTest);

                default: throw new NotSupportedException(type.ToString());
            }
        }

        public static bool AreEqual(TextControl first, TextControl second)
        {
            return first.Text == second.Text;
        }

        public static bool AreEqual(SimpleTest first, SimpleTest second)
        {
            return first.Question == second.Question &&
                AreEqual(first.Answers, second.Answers) &&
                first.SelectedAnswers.SequenceEqual(second.SelectedAnswers) &&
                first.TrueAnswers.SequenceEqual(second.TrueAnswers);
        }

        public static bool AreEqual(IList<SimpleAnswerModel> first, IList<SimpleAnswerModel> second)
        {
            if (first.Count != second.Count) return false;

            // Iterates over Answers

            for (int i = 0; i < first.Count; i++)
            {
                var firstAnswer = first[i];
                var secondAnswer = second[i];

                if (!firstAnswer.Equals(secondAnswer)) return false;
            }

            return true;
        }
    }
}
