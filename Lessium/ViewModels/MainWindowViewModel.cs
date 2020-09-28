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

        #region Program

        public string Title
        {
            get { return model.title; }
            set { SetProperty(ref model.title, value); }
        }

        #endregion

        #region LessonMenu

        public string LessonHeader
        {
            get { return model.LessonHeader; }
            set { model.LessonHeader = value; }
        }

        public string EditHeader
        {
            get { return model.EditHeader; }
            set { model.EditHeader = value; }
        }

        public string UndoChangesHeader
        {
            get { return model.UndoChangesHeader; }
            set { model.UndoChangesHeader = value; }
        }

        public string RecentHeader
        {
            get { return model.RecentHeader; }
            set { model.RecentHeader = value; }
        }

        public string NewLessonHeader
        {
            get { return model.NewLessonHeader; }
            set { model.NewLessonHeader = value; }
        }

        public string SaveLessonHeader
        {
            get { return model.SaveLessonHeader; }
            set { model.SaveLessonHeader = value; }
        }

        public string LoadLessonHeader
        {
            get { return model.LoadLessonHeader; }
            set { model.LoadLessonHeader = value; }
        }

        public string CloseLessonHeader
        {
            get { return model.CloseLessonHeader; }
            set { model.CloseLessonHeader = value; }
        }

        public string PrintLessonHeader
        {
            get { return model.PrintLessonHeader; }
            set { model.PrintLessonHeader = value; }
        }

        public string ExitHeader
        {
            get { return model.ExitHeader; }
            set { model.ExitHeader = value; }
        }

        #endregion

        #region Tabs

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
