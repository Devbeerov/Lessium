namespace Lessium.Interfaces
{
    public interface ITestControl : IContentControl
    {
        object[] SelectedAnswers { get; set; }

        object[] TrueAnswers { get; set; }

        bool CheckAnswers();
    }
}
