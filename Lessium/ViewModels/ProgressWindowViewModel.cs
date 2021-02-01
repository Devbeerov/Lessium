using Prism.Mvvm;

namespace Lessium.ViewModels
{
    public class ProgressWindowViewModel : BindableBase
    {
        private int progressValue;
        public int ProgressValue
        {
            get { return progressValue; }
            set { SetProperty(ref progressValue, value); }
        }

        public ProgressWindowViewModel()
        {

        }

        public void SetProgressValue(int value)
        {
            ProgressValue = value;
        }
    }
}
