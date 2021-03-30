using Lessium.Properties;
using Prism.Mvvm;
using System.Reflection;

namespace Lessium.ViewModels
{
    public class AboutViewModel : BindableBase
    {
        #region CLR Properties

        public string AboutHeader
        {
            get { return Resources.AboutHeader; }
        }

        public string VersionText
        {
            get { return string.Format("Lessium {0} {1}", Resources.Version.ToLower(), Assembly.GetExecutingAssembly().GetName().Version); }
        }

        public string CopyrightText
        {
            get { return string.Format("Copyright © 2021 Devbeerov. {0}.", Resources.AllRightsReserved); }
        }

        #endregion
    }
}
