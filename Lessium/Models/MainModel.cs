using Lessium.ContentControls;
using Lessium.ContentControls.Models;
using Lessium.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable S1104 // Fields should not have public accessibility

namespace Lessium.Models
{
    public class MainModel
    {
        // NOTE: Avoid const fields!

        #region Window

        public string title = "Lessium";

        #endregion

        #region Lesson Menu

        public bool HasChanges = false;
        public bool ReadOnly = true;

        #endregion

        #region Tabs

        public ContentType SelectedContentType = ContentType.Material;

        #endregion

        #region Pages

        // Updates only on SelectedContentType change!
        public Dictionary<ContentType, Section> LastSelectedSection = new Dictionary<ContentType, Section>()
        {
            { ContentType.Material, null },
            { ContentType.Test, null }
        };

        // Updates only on CurrentSection change!
        public Dictionary<Section, ContentPage> LastSelectedPage = new Dictionary<Section, ContentPage>();

        #endregion

        #region Sections

        /// This could look confusing, but it's simple.
        /// To access section with key "Section 1" in "Materials" we can do this:
        /// Section section = Sections["Materials"]["Section 1"];
        public Dictionary<ContentType, ObservableCollection<Section>> Sections =
            new Dictionary<ContentType, ObservableCollection<Section>>()
            {
                { ContentType.Material, new ObservableCollection<Section>() },
                { ContentType.Test, new ObservableCollection<Section>() }
            };

        public Dictionary<ContentType, Section> CurrentSection = new Dictionary<ContentType, Section>()
        {
            { ContentType.Material, null },
            { ContentType.Test, null }
        };
        public Dictionary<ContentType, int> CurrentSectionID = new Dictionary<ContentType, int>()
        {
            { ContentType.Material, -1 },
            { ContentType.Test, -1 }
        };

        #endregion

        #region Localisation

        #region Windows

        public string ProgressWindowTitle_Saving = Resources.SavingHeader;
        public string ProgressWindowTitle_Loading = Resources.LoadingHeader;

        #endregion

        #region Lesson Menu

        public string LessonHeader = Resources.LessonHeader;
        public string EditHeader = Resources.EditHeader;
        public string StopEditingHeader = Resources.StopEditingHeader;
        public string UndoChangesHeader = Resources.UndoChangesHeader;
        public string RedoChangesHeader = Resources.RedoChangesHeader;
        public string RecentHeader = Resources.RecentHeader;
        public string NewLessonHeader = Resources.NewLessonHeader;
        public string SaveLessonHeader = Resources.SaveLessonHeader;
        public string LoadLessonHeader = Resources.LoadLessonHeader;
        public string CloseLessonHeader = Resources.CloseLessonHeader;
        public string PrintLessonHeader = Resources.PrintLessonHeader;
        public string ExitHeader = Resources.ExitHeader;

        #endregion

        #region MessageBoxes, Dialog-related

        public string NewLessonMessageHeader = Resources.NewLessonMessageHeader;
        public string NewLessonMessageText = Resources.NewLessonMessageText;

        public string LessonFilter = Resources.LessonFilter;

        #endregion

        #region Tabs

        public string Materials = Resources.Materials;
        public string Tests = Resources.Tests;

        #endregion

        #region Sections

        #region MaterialControls

        public string AudioHeader = Resources.AudioHeader;
        public string ImageHeader = Resources.ImageHeader;
        public string JokeHeader = Resources.JokeHeader;
        public string TextHeader = Resources.TextHeader;
        public string VideoHeader = Resources.VideoHeader;

        #endregion

        #region TestControls

        public string LinkTogetherHeader = Resources.LinkTogetherHeader; // 1 = A, 2 = B, 3 = C, 4 = D
        public string PrioritiesHeader = Resources.PrioritiesHeader; // A > B, B > C, C > D
        public string SimpleTestHeader = Resources.SimpleTestHeader;
        public string TrickyQuestionHeader = Resources.TrickyQuestionHeader;
        public string TrueFalseHeader = Resources.TrueFalseHeader;
        public string DifferencesHeader = Resources.DifferencesHeader;

        #endregion

        #endregion

        #region Buttons

        public string ButtonAddHeader = Resources.ButtonAddHeader;
        public string ButtonRemoveHeader = Resources.ButtonRemoveHeader;
        public string AddSectionText = Resources.AddSectionText;
        public string NewSection = Resources.NewSection;
        public string AddContentHeader = Resources.AddContentHeader;

        #endregion

        #region ToolTips

        public string ReadOnlyToolTip = Resources.ReadOnlyToolTip;

        #endregion

        #endregion

    }
}

#pragma warning restore S1104 // Fields should not have public accessibility