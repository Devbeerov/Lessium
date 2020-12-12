using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using Lessium.Utility;

namespace Lessium.ContentControls.Models
{
    // Model
    public class ContentPage
    {
        // Public

        public const double PageWidth = 796d;
        public const double PageHeight = 637d;

        // Private

        private double actualWidth = PageWidth;
        private double actualHeight = PageWidth;
        
        private bool editable = false;

        private readonly ObservableCollection<IContentControl> items = new ObservableCollection<IContentControl>();

        // Events

        public event SizeChangedEventHandler ContentResized;

        #region Properties

        public ObservableCollection<IContentControl> Items => items;

        #endregion

        #region Methods

        #region Public

        public ContentPage()
        {

        }

        public void Add(IContentControl element)
        {
            element.SetMaxWidth(actualWidth);
            element.SetMaxHeight(actualHeight);

            element.RemoveControl += OnRemove;
            element.Resize += OnItemResize;

            Items.Add(element);
            UpdateItemEditable(element);
        }

        public void Remove(IContentControl element)
        {
            element.RemoveControl -= OnRemove;
            element.Resize -= OnItemResize;
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

        public void SetMaxWidth(double width)
        {
            actualWidth = width;

            foreach (var childControl in items)
            {
                childControl.SetMaxWidth(actualWidth);
            }
        }

        public void SetMaxHeight(double height)
        {
            actualHeight = height;

            foreach(var childControl in items)
            {
                childControl.SetMaxHeight(actualHeight);
            }
        }

        #endregion

        #region Private

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

        #region Events

        public void OnRemove(object sender, RoutedEventArgs e)
        {
            Remove(e.Source as IContentControl);
        }

        public void OnItemResize(object sender, SizeChangedEventArgs e)
        {
            ContentResized?.Invoke(sender, e);
        }

        #endregion

    }
}
