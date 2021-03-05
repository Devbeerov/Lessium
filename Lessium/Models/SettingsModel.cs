using Lessium.Properties;
using System.Collections.ObjectModel;

#pragma warning disable S1104 // Fields should not have public accessibility

namespace Lessium.Models
{
    public class SettingsModel
    {
        // NOTE: Avoid const fields!

        #region Collections

        public ObservableCollection<string> SectionHeaders = new ObservableCollection<string>()
        {
            Resources.Generic, Resources.Editing
        };

        #endregion

        #region Localisation

        #region Headers

        public string SettingsHeader { get; set; } = Resources.SettingsHeader;
        public string FontSliderHeader { get; set; } = Resources.FontSliderHeader;

        #endregion

        #endregion

    }
}

#pragma warning restore S1104 // Fields should not have public accessibility