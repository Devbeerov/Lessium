using Lessium.Interfaces;
using System.Collections.Generic;

namespace Lessium.Classes
{
    public abstract class Section
    {
        public string Title = string.Empty;

        // Both for MaterialControls and TestControls
        protected List<IContentControl> contentControls = new List<IContentControl>();

        public List<IContentControl> ContentControls
        {
            get { return contentControls; }
            set { contentControls = value; }
        }
    }
}
