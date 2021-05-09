using System.Collections.Generic;

namespace Lessium.Interfaces
{
    public interface ITestControl : IContentControl
    {
        IList<object> SelectedAnswers { get; set; }

        IList<object> TrueAnswers { get; set; }

        bool CheckAnswers();
    }
}
