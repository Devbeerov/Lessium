namespace Lessium.Interfaces
{
    public interface ITestControl : IContentControl
    {
        AnswersMode AnswersMode { get; set; }

        object SelectedAnswer { get; set; }
        object[] SelectedAnswers { get; set; }

        object TrueAnswer { get; set; }
        object[] TrueAnswers { get; set; }

        bool CheckAnswers();
    }

    public enum AnswersMode
    {
        Single, Multiple
    }
}
