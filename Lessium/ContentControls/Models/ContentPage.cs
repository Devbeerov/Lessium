using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

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
        private double actualHeight = PageHeight;
        
        private bool editable = false;

        private readonly ObservableCollection<IContentControl> items = new ObservableCollection<IContentControl>();

        #region Properties

        public ObservableCollection<IContentControl> Items => items;

        #endregion

        #region Methods

        #region Public

        public ContentPage()
        {

        }

        public void Add(IContentControl element, bool intoBeginning = false)
        {
            element.SetMaxWidth(actualWidth);
            element.SetMaxHeight(actualHeight);

            element.Resize += OnContentResized;
            element.RemoveControl += OnRemove;

            if (!intoBeginning)
            {
                Items.Add(element);
            }

            else
            {
                Items.Insert(0, element);
            }

            UpdateItemEditable(element);

            // If Element is already loaded (for example, moved from another page to this one), validates it's position
            if ((element as FrameworkElement).IsLoaded)
            {
                ValidateContentPlacement(element);
            }
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

        public void SetMaxWidth(double width)
        {
            if(actualWidth == width) { return; }

            actualWidth = width;

            foreach (var childControl in Items)
            {
                childControl.SetMaxWidth(actualWidth);
            }
        }

        public void SetMaxHeight(double height)
        {
            if (actualHeight == height) { return; }

            actualHeight = height;

            foreach(var childControl in Items)
            {
                childControl.SetMaxHeight(actualHeight);
            }
        }

        public bool IsContentFits(IContentControl content)
        {
            var contentElement = content as FrameworkElement;
            var contentParent = VisualTreeHelper.GetParent(contentElement) as UIElement; // Assuming ContentPageControl
            var contentLocation = contentElement.TranslatePoint(new Point(0, 0), contentParent);

            return contentElement.ActualHeight + contentLocation.Y > this.actualHeight;
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
            foreach (var childControl in Items)
            {
                UpdateItemEditable(childControl);
            }
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler<ExceedingContentEventArgs> AddedExceedingContent;

        public class ExceedingContentEventArgs : EventArgs
        {
            public ContentPage Page { get; set; }
            public IContentControl ExceedingItem { get; set; }
        }

        private void OnRemove(object sender, RoutedEventArgs e)
        {
            Remove(e.Source as IContentControl);
        }

        private void OnContentResized(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                var control = e.Source as IContentControl;

                var collection = Items;
                var controlPos = collection.IndexOf(control);

                if (ValidateContentPlacement(control))
                {
                    // 0 1 2 3 4 5 6 7
                    //           | <-
                    // 0 1 2 3 4 5 6
                    // item with index 6 become item with index 5
                    // we checked previous 5, so we check new 5 too

                    // If content gone to the next page, we also check all content from upper bound to previous content position.

                    for (int pos = collection.Count - 1; pos >= controlPos; pos--)
                    {
                        var item = collection[pos];
                        ValidateContentPlacement(item);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if item position is fits to Page.
        /// If it's not valid, throws event for further handling.
        /// </summary>
        /// <param name="item">Element to check</param>
        /// <returns>True if event throwed, otherwise - false.</returns>
        private bool ValidateContentPlacement(IContentControl item)
        {
            if (IsContentFits(item))
            {
                var args = new ExceedingContentEventArgs()
                {
                    ExceedingItem = item,
                    Page = this
                };

                AddedExceedingContent?.Invoke(this, args);

                return true;
            }

            return false;
        }

        #endregion

    }
}
