using Lessium.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.ContentControls
{
    public class ContentPage : WrapPanel
    {
        public const double PageWidth = 795;
        public const double PageHeight = 610;

        private bool editable = false;

        public ContentPage() : base()
        {
            Initialize();
        }

        private readonly ObservableCollection<IContentControl> items = new ObservableCollection<IContentControl>();

        #region Dependency Properties Methods

        #region Items

        public static ObservableCollection<IContentControl> GetItems(DependencyObject obj)
        {
            return (ObservableCollection<IContentControl>)obj.GetValue(Items);
        }

        protected static void SetItems(DependencyObject obj, ObservableCollection<IContentControl> items)
        {
            obj.SetValue(Items, items);
        }

        public ObservableCollection<IContentControl> GetItems()
        {
            return GetItems(this);
        }

        protected void SetItems(ObservableCollection<IContentControl> items)
        {
            SetItems(this, items);
        }

        #endregion

        #endregion

        #region Methods

        #region Public

        public void Initialize()
        {
            Width = PageWidth;
            Height = PageHeight;

            Orientation = Orientation.Vertical;
        }

        public void Add(IContentControl element)
        {
            items.Add(element);
            UpdateItemEditable(element);
        }

        public void Remove(IContentControl element)
        {
            items.Remove(element);
        }

        public bool GetEditable()
        {
            return editable;
        }

        public void SetEditable(bool editable)
        {
            this.editable = editable;

            UpdateItemsEditable();
        }

        private void UpdateItemEditable(IContentControl contentControl)
        {
            if (contentControl != null)
            {
                contentControl.SetEditable(editable);
            }
        }

        private void UpdateItemsEditable()
        {
            foreach (var childControl in items)
            {
                UpdateItemEditable(childControl);
            }
        }

        #endregion

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty Items =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
            typeof(Section), new PropertyMetadata(null));

        #endregion
    }
}
