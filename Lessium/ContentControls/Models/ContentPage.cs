using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
namespace Lessium.ContentControls.Models
{
    // Model
    public class ContentPage
    {
        // Public

        public const double PageWidth = 796d;
        public const double PageHeight = 637d;

        // Private

        private double maxWidth = PageWidth;
        private double maxHeight = PageHeight;

        private bool editable = false;

        private readonly ObservableCollection<IContentControl> items = new ObservableCollection<IContentControl>();

        private ContentPageControl pageControl;

        #region Properties

        public ObservableCollection<IContentControl> Items => items;

        #endregion

        #region Methods

        #region Public

        public ContentPage()
        {

        }

        public static ContentPage CreateWithPageControlInjection(ContentPage oldPage)
        {
            var newPage = new ContentPage();
            newPage.SetPageControl(oldPage.pageControl);
            return newPage;
        }

        public void Add(IContentControl element, bool intoBeginning = false)
        {
            element.SetMaxWidth(maxWidth);
            element.SetMaxHeight(maxHeight);

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
                if (intoBeginning)
                {
                    ValidateAllForward(element);
                }

                else
                {
                    ValidateContentPlacement(element);
                }
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
            if(maxWidth == width) { return; }

            maxWidth = width;

            foreach (var childControl in Items)
            {
                childControl.SetMaxWidth(maxWidth);
            }
        }

        public void SetMaxHeight(double height)
        {
            if (maxHeight == height) { return; }

            maxHeight = height;

            foreach(var childControl in Items)
            {
                childControl.SetMaxHeight(maxHeight);
            }
        }

        public bool IsContentFits(IContentControl content, bool ignoreLocation = false)
        {
            var contentElement = content as FrameworkElement;
            Point contentLocation = default(Point);
            if (!ignoreLocation) 
            {
                contentLocation = contentElement.TranslatePoint(default(Point), pageControl);
            }

            return contentElement.ActualHeight + contentLocation.Y < maxHeight;
        }

        public void SetPageControl(ContentPageControl newPageControl)
        {
            pageControl = newPageControl;
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

        /// <summary>
        /// Validates all controls after specified.
        /// </summary>
        private void ValidateAllForward(IContentControl control)
        {
            var collection = Items;
            var lastControlPosition = collection.Count - 1;
            var lastControl = collection[lastControlPosition];

            // Checks LAST control on this page, if it's not fits, validates backwards to this control including.
            if (ValidateContentPlacement(lastControl))
            {
                var controlPos = collection.IndexOf(control);

                // In case (current)control was lastControl, it will be deleted during validation, therefore sets position to zero.
                if (controlPos == -1)
                {
                    controlPos = 0;
                }

                for (int pos = lastControlPosition - 1; pos >= controlPos; pos--)
                {
                    var item = collection[pos];
                    ValidateContentPlacement(item);
                }
            }
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler<ExceedingContentEventArgs> AddedExceedingContent;

        private void OnRemove(object sender, RoutedEventArgs e)
        {
            Remove(e.Source as IContentControl);
        }

        private void OnContentResized(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                ValidateAllForward(e.Source as IContentControl);
            }
        }

        /// <summary>
        /// Checks if item position is fits to Page.
        /// If it's not valid, throws event for further handling.
        /// </summary>
        /// <param name="item">Element to check</param>
        /// <returns>True if event throwed, otherwise - false.</returns>
        private bool ValidateContentPlacement(IContentControl item, bool ignoreLocation = false)
        {
            if (!IsContentFits(item, ignoreLocation))
            {
                var args = new ExceedingContentEventArgs(item);
                AddedExceedingContent?.Invoke(this, args);

                // Updates position of elements after moving item forward.

                pageControl.UpdateLayout();

                return true;
            }

            return false;
        }

        #endregion

    }

    public class ExceedingContentEventArgs : EventArgs
    {
        public IContentControl ExceedingItem { get; private set; }

        public ExceedingContentEventArgs(IContentControl ExceedingItem)
        {
            this.ExceedingItem = ExceedingItem;
        }
    }
}
