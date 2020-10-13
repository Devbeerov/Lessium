using Lessium.ContentControls;
using Lessium.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lessium.Models
{
    public class MainWindowModel
    {
        // Window

        public string title = "Lessium";

        // Lesson Menu

            // Headers

            public string LessonHeader = Resources.LessonHeader;
            public string EditHeader = Resources.EditHeader;
            public string UndoChangesHeader = Resources.UndoChangesHeader;
            public string RecentHeader = Resources.RecentHeader;
            public string NewLessonHeader = Resources.NewLessonHeader;
            public string SaveLessonHeader = Resources.SaveLessonHeader;
            public string LoadLessonHeader = Resources.LoadLessonHeader;
            public string CloseLessonHeader = Resources.CloseLessonHeader;
            public string PrintLessonHeader = Resources.PrintLessonHeader;
            public string ExitHeader = Resources.ExitHeader;

            // Internal

            public bool HasChanges = false;
            public bool ReadOnly = true;

        // Tabs

        public string MaterialHeader = Resources.MaterialHeader;
        public string TestsHeader = Resources.TestsHeader;

        public ObservableDictionary<string, Section> Sections = new ObservableDictionary<string, Section>();
        public Section CurrentSection;
        public int CurrentSectionID = -1;

        // Buttons

        public string ButtonAddHeader = Resources.ButtonAddHeader;
        public string ButtonRemoveHeader = Resources.ButtonRemoveHeader;
        public string AddSectionText = Resources.AddSectionText;
        public string NewSection = Resources.NewSection;

        // Messages

        public string Message_NotEnabledInReadOnly = Resources.Message_NotEnabledInReadOnly;




    }
}
