using System.Collections.Generic;

namespace Lessium.Interfaces
{
    public interface ITestControl<AnswerType> : IContentControl
    {
        IList<AnswerType> Answers { get; set; }

        IList<AnswerType> TrueAnswers { get; set; }

        IList<AnswerType> SelectedAnswers { get; set; }

        bool CheckAnswers();
    }
}
