using Lessium.CustomControls;
using System;
using System.Collections.Generic;

namespace Lessium.Services
{
    public class AnswersMappingHelper<AnswerModelType>
    {
        private IList<AnswerModelType> trueAnswers;
        private IList<AnswerModelType> selectedAnswers;

        public AnswersMappingHelper(IList<AnswerModelType> trueAnswers, IList<AnswerModelType> selectedAnswers)
        {
            this.trueAnswers = trueAnswers;
            this.selectedAnswers = selectedAnswers;
        }

        public List<DynamicCheckBoxType> GetCheckBoxTypes(IList<AnswerModelType> answers)
        {
            if (answers == trueAnswers) return GetCheckBoxTypes(AnswersType.TrueAnswers);
            if (answers == selectedAnswers) return GetCheckBoxTypes(AnswersType.SelectedAnswers);

            throw new InvalidOperationException($"\"{nameof(answers)}|\" is not mapped.");
        }

        private static List<DynamicCheckBoxType> GetCheckBoxTypes(AnswersType type)
        {
            List<DynamicCheckBoxType> list = new List<DynamicCheckBoxType>();

            switch (type)
            {
                case AnswersType.TrueAnswers:
                    list.Add(DynamicCheckBoxType.CheckBox);
                    break;

                case AnswersType.SelectedAnswers:
                    list.Add(DynamicCheckBoxType.RadioSingle);
                    list.Add(DynamicCheckBoxType.RadioMultiple);
                    break;

                default: throw new NotSupportedException();
            }

            return list;
        }

        private enum AnswersType
        {
            TrueAnswers, SelectedAnswers
        }
    }
}
