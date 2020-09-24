using Lessium.Models;
using Prism.Mvvm;
using System.Windows.Controls;

namespace Lessium.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private properties

        private MainWindowModel model;

        #endregion

        #region Public CLR Properties

        public string Title
        {
            get { return model.title; }
            set { SetProperty(ref model.title, value); }
        }

        public string LessonHeader
        {
            get { return model.LessonHeader; }
            set { model.LessonHeader = value; }
        }

        public string MaterialHeader
        {
            get { return model.MaterialHeader; }
            set { model.MaterialHeader = value; }
        }

        public string TestHeader
        {
            get { return model.TestHeader; }
            set { model.TestHeader = value; }
        }

        #endregion

        #region Methods

        // Constructs ViewModel with Model as parameter.
        public MainWindowViewModel(MainWindowModel model = null)
        {
            // In case we don't provide model (for example when Prism wires ViewModel automatically), creates new Model.

            if(model == null)
            {
                model = new MainWindowModel();
            }

            this.model = model;
        }

        #endregion

        #region Events

        public void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion
    }
}
