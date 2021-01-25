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

        public void Add(IContentControl control)
        {
            // To skip unwanted validating.
            if(items.Count == 0)
            {
                Insert(0, control);
                return;
            }

            BindControl(control);

            items.Add(control);

            // If control is already loaded (for example, moved from another page to this one), validates it
            if ((control as FrameworkElement).IsLoaded)
            {
                ValidateContentPlacement(control);
            }
        }

        // Won't validate control if it's in zero position.
        public void Insert(int position, IContentControl control)
        {
            BindControl(control);

            Items.Insert(position, control);

            // If control is already loaded (for example, moved from another page to this one), validates forward
            if ((control as FrameworkElement).IsLoaded)
            {
                bool ignore = position == 0; // Only skips check if inserted at zero position.
                ValidateAllForward(control, ignore);
            }
        }

        public void Remove(IContentControl element)
        {
            element.RemoveControl -= OnRemove;
            element.Resize -= OnContentResized;

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

        public bool IsContentFits(IContentControl content)
        {
            var contentElement = content as FrameworkElement;

            // Ensures that content childs properly updated.

            contentElement.UpdateLayout();

            // Ensures that all items are properly placed

            pageControl.UpdateLayout();

            // Checks if it fits

            var contentLocation = contentElement.TranslatePoint(default(Point), pageControl);
            return contentElement.ActualHeight + contentLocation.Y <= maxHeight;
        }

        public void SetPageControl(ContentPageControl newPageControl)
        {
            pageControl = newPageControl;
            if (!pageControl.IsLoaded)
            {
                pageControl.Loaded += (s, e) =>
                {
                    SetMaxHeight(pageControl.MaxHeight);
                };
            }
            else
            {
                SetMaxHeight(pageControl.MaxHeight);
            }
        }

        public ContentPageControl GetPageControl()
        {
            return pageControl;
        }

        public void ValidatePage()
        {
            if (Items.Count > 0)
            {
                ValidateAllForward(Items[0], true);
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
            foreach (var childControl in Items)
            {
                UpdateItemEditable(childControl);
            }
        }

        /// <summary>
        /// Binds control to Page, updates its properties to match Page, also attaches event handlers
        /// </summary>
        /// <param name="control"></param>
        private void BindControl(IContentControl control)
        {
            control.SetMaxWidth(maxWidth);
            control.SetMaxHeight(maxHeight);

            control.Resize += OnContentResized;
            control.RemoveControl += OnRemove;

            UpdateItemEditable(control);
        }

        /// <summary>
        /// Validates all controls after specified.
        /// <param name="ignoreControl">Should given control be validated or not.</param>
        /// </summary>
        private void ValidateAllForward(IContentControl control, bool ignoreControl = false)
        {
            var collection = Items;
            var lastControlPosition = collection.Count - 1;
            var lastControl = collection[lastControlPosition];

            // Checks LAST control on this page, if it's not fits, validates backwards to this control.
            // If lastControl is given control and we ignoreControl, then ignores whole cycle, because there's nothing after Last.
            if ((ignoreControl && control == lastControl) || ValidateContentPlacement(lastControl))
            {
                var controlPos = collection.IndexOf(control);

                // In case (current)control was lastControl, it will be deleted during validation, therefore sets position to zero.
                if (controlPos == -1)
                {
                    controlPos = 0;
                }

                for (int pos = lastControlPosition - 1; pos >= controlPos; pos--)
                {
                    if(ignoreControl && pos == controlPos) { return; }
                    if (pos == 0) { return; } // Don't check if its in zero position, it will be always valid.
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
            if (e.Handled) { return; }

            ValidateAllForward(e.Source as IContentControl);
            
        }

        /// <summary>
        /// Checks if item position is fits to Page.
        /// If it's not valid, throws event for further handling.
        /// </summary>
        /// <param name="item">Element to check</param>
        /// <returns>True if event throwed, otherwise - false.</returns>
        private bool ValidateContentPlacement(IContentControl item)
        {
            if (!IsContentFits(item))
            {
                var args = new ExceedingContentEventArgs(item);
                AddedExceedingContent?.Invoke(this, args);

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
