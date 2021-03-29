using Lessium.Properties;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lessium.Models
{
    public class SettingsModel
    {
        // NOTE: Avoid const fields!

        #region Fields (could be used for referencing)

        public string selectedSectionKey;

        #endregion

        #region Collections

        public ObservableCollection<LocalizedString> SectionsStrings = new ObservableCollection<LocalizedString>()
        {
            new LocalizedString(nameof(Resources.Generic), Resources.Generic),
            new LocalizedString(nameof(Resources.Editing), Resources.Editing)
        };

        #endregion

        #region Localisation

        #region Headers

        public string SettingsHeader { get; set; } = Resources.SettingsHeader;

        public string FontSliderHeader { get; set; } = Resources.FontSliderHeader;

        public string UndoLimitHeader { get; set; } = Resources.UndoLimitHeader;

        #endregion

        #endregion
    }
}