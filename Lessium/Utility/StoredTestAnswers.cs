using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Lessium.Utility
{
    /// <summary>
    /// Stores AnswerModel Lists, because their values will be null at constructor.
    /// They will be already loaded at time of OnDeserialization.
    /// </summary>
    public class StoredTestAnswers<AnswerModelType>
    {
        private readonly IList<AnswerModelType> answers;
        private readonly IList<AnswerModelType> trueAnswers;
        private readonly IList<AnswerModelType> selectedAnswers;

        public StoredTestAnswers(SerializationInfo info, string answersKey, string trueAnswersKey, string selectedAnswersKey)
        {
            answers = (IList<AnswerModelType>)info.GetValue(answersKey, typeof(IList<AnswerModelType>));
            trueAnswers = (IList<AnswerModelType>)info.GetValue(trueAnswersKey, typeof(IList<AnswerModelType>));
            selectedAnswers = (IList<AnswerModelType>)info.GetValue(selectedAnswersKey, typeof(IList<AnswerModelType>));
        }

        public void AddStoredListsTo(IList<AnswerModelType> answers, IList<AnswerModelType> trueAnswers, IList<AnswerModelType> selectedAnswers)
        {
            answers?.AddRange(this.answers);
            trueAnswers?.AddRange(this.trueAnswers);
            selectedAnswers?.AddRange(this.selectedAnswers);
        }

        /// <summary>
        /// Will clear all ILists.
        /// </summary>
        public void Clear()
        {
            answers?.Clear();
            trueAnswers?.Clear();
            selectedAnswers?.Clear();
        }
    }
}
