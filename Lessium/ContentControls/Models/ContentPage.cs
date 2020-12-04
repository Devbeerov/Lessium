using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Lessium.ContentControls.Models
{
    // Model
    public class ContentPage
    {
        public const double PageWidth = 796d;
        public const double PageHeight = 637d;

        private bool editable = false;

        public ContentPage()
        {

        }

        private readonly ObservableCollection<IContentControl> items = new ObservableCollection<IContentControl>();

        public ObservableCollection<IContentControl> Items => items;

        #region Methods

        #region Public

        public void Add(IContentControl element)
        {
            element.SetMaxWidth(PageWidth);
            element.SetMaxHeight(PageHeight);
            element.RemoveControl += OnRemove;
            Items.Add(element);
            UpdateItemEditable(element);
        }

        public void Remove(IContentControl element)
        {
            element.RemoveControl -= OnRemove;
            Items.Remove(element);
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
            foreach (var childControl in Items)
            {
                UpdateItemEditable(childControl);
            }
        }

        #endregion

        #endregion

        #region Events

        public void OnRemove(object sender, RoutedEventArgs e)
        {
            Remove(e.Source as IContentControl);
        }

        #endregion

    }
}
