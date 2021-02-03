using Prism.Mvvm;

namespace Lessium.ViewModels
{
    public class ProgressWindowViewModel : BindableBase
    {
        #region CLR Properties

        public string ProgressText
        {
            get { return $"{ProgressValue} / {progressCount}"; }
        }

        public string TitleWithProgress
        {
            get { return $"{OriginalTitle} ({ProgressText})"; }
        }

        private int progressValue = 0;
        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                SetProperty(ref progressValue, value);
                UpdateProgressText(); // Will update title too.
            }
        }

        private int progressCount = 0;
        public int ProgressCount
        {
            get { return progressCount; }
            set
            {
                SetProperty(ref progressCount, value);
                UpdateProgressText(); // Will update title too.
            }
        }

        private string originalTitle;
        public string OriginalTitle
        {
            get { return originalTitle; }
            set
            {
                SetProperty(ref originalTitle, value);
                UpdateTitle();
            }
        }

        #endregion

        #region Methods

        #region Public

        public ProgressWindowViewModel()
        {

        }

        public void SetProgressValue(int value)
        {
            ProgressValue = value;
        }

        #endregion

        #region Private

        private void UpdateProgressText()
        {
            RaisePropertyChanged(nameof(ProgressText));
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            RaisePropertyChanged(nameof(TitleWithProgress));
        }

        #endregion

        #endregion
    }
}
