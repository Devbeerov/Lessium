using Lessium.ContentControls;
using Lessium.ContentControls.Models;
using Lessium.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#pragma warning disable S1104 // Fields should not have public accessibility

namespace Lessium.Models
{
    public class MainWindowModel
    {
        // NOTE: Avoid static fields!

        #region Window

        public string title = "Lessium";

        #endregion

        #region Lesson Menu

        public bool HasChanges = false;
        public bool ReadOnly = true;

        #endregion

        #region Tabs

        public enum Tab
        {
            Materials, Tests
        }

        public string SelectedTab = Tab.Materials.ToString();

        #endregion

        #region Pages

        // Updates only on SelectedTab change!
        public Dictionary<string, Section> LastSelectedSection = new Dictionary<string, Section>()
        {
            { "Materials", null },
            { "Tests", null }
        };

        // Updates only on CurrentSection change!
        public Dictionary<Section, ContentPage> LastSelectedPage = new Dictionary<Section, ContentPage>();

        #endregion

        #region Sections

        /// This could look confusing, but it's simple.
        /// To access section with key "Section 1" in "Materials" we can do this:
        /// Section section = Sections["Materials"]["Section 1"];
        public Dictionary<string, ObservableCollection<Section>> Sections =
            new Dictionary<string, ObservableCollection<Section>>()
            {
                { "Materials", new ObservableCollection<Section>() },
                { "Tests", new ObservableCollection<Section>() }
            };

        public Dictionary<string, Section> CurrentSection = new Dictionary<string, Section>()
        {
            { "Materials", null },
            { "Tests", null }
        };
        public Dictionary<string, int> CurrentSectionID = new Dictionary<string, int>()
        {
            { "Materials", -1 },
            { "Tests", -1 }
        };

        #endregion

        #region Localisation

        #region Headers

        // Lesson Menu

        public string LessonHeader = Resources.LessonHeader;
        public string EditHeader = Resources.EditHeader;
        public string StopEditingHeader = Resources.StopEditingHeader;
        public string UndoChangesHeader = Resources.UndoChangesHeader;
        public string RecentHeader = Resources.RecentHeader;
        public string NewLessonHeader = Resources.NewLessonHeader;
        public string SaveLessonHeader = Resources.SaveLessonHeader;
        public string LoadLessonHeader = Resources.LoadLessonHeader;
        public string CloseLessonHeader = Resources.CloseLessonHeader;
        public string PrintLessonHeader = Resources.PrintLessonHeader;
        public string ExitHeader = Resources.ExitHeader;

        // Tabs

        public string MaterialHeader = Resources.MaterialHeader;
        public string TestsHeader = Resources.TestsHeader;

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

        public string ActionsInCaseHeader = Resources.ActionsInCaseHeader;
        public string CompareHeader = Resources.CompareHeader;
        public string DifferencesHeader = Resources.DifferencesHeader;
        public string LinkTogetherHeader = Resources.LinkTogetherHeader;
        public string MiniGameHeader = Resources.MiniGameHeader;
        public string PrioritiesHeader = Resources.PrioritiesHeader;
        public string SelectCorrectHeader = Resources.SelectCorrectHeader;
        public string SIGameHeader = Resources.SIGameHeader;
        public string SimpleTestHeader = Resources.SimpleTestHeader;
        public string TrickyQuestionHeader = Resources.TrickyQuestionHeader;
        public string TrueFalseHeader = Resources.TrueFalseHeader;

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