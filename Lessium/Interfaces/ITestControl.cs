using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Lessium.Interfaces
{
    public interface ITestControl<AnswerType> : ITestControl
    {
        new IList<AnswerType> Answers { get; set; }

        new IList<AnswerType> TrueAnswers { get; set; }

        new IList<AnswerType> SelectedAnswers { get; set; }
    }

    public interface ITestControl : IContentControl
    {
        IList Answers { get; set; }

        IList TrueAnswers { get; set; }

        IList SelectedAnswers { get; set; }

        bool CheckAnswers();
        event NotifyCollectionChangedEventHandler AnswersChanged;
    }
}
