using Lessium.Models;
using Prism.Mvvm;

namespace Lessium.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private MainWindowModel model;

        public string Title
        {
            get { return model.title; }
            set { SetProperty(ref model.title, value); }
        }

        // Constructs ViewModel with Model as parameter.
        public MainWindowViewModel(MainWindowModel model = null)
        {
            // In case we don't provide model (for example when Prism wires ViewModel automatically), creates new Model.

            if(model == null)
            {
                model = new MainWindowModel();
                return;
            }

            this.model = model;
        }
    }
}
