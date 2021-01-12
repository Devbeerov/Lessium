using Lessium.Interfaces;
using Prism.Commands;
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
            var contentLocation = contentElement.TranslatePoint(default(Point), pageControl);

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

        #endregion

        #endregion

        #region Events

        public event EventHandler<ExceedingContentEventArgs> AddedExceedingContent;

        public class ExceedingContentEventArgs : EventArgs
        {
            public IContentControl ExceedingItem { get; set; }

            public ExceedingContentEventArgs(IContentControl ExceedingItem)
            {
                this.ExceedingItem = ExceedingItem;
            }
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

                var lastControlPosition = collection.Count - 1;
                var lastControl = collection[lastControlPosition];

                // Checks LAST control on this page, if it's not fits, validates backwards to this control including.
                if (ValidateContentPlacement(lastControl))
                {
                    // If lastControl was (current)control, it won't iterate.
                    for (int pos = lastControlPosition - 1; pos >= controlPos; pos--)
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
            if (!IsContentFits(item))
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

        #region Event-Commands

        private DelegateCommand<double?> UpdateMaxHeightCommand;
        public DelegateCommand<double?> UpdateMaxHeight =>
            UpdateMaxHeightCommand ?? (UpdateMaxHeightCommand = new DelegateCommand<double?>(ExecuteUpdateMaxHeight));

        void ExecuteUpdateMaxHeight(double? maxHeight)
        {
            if(!maxHeight.HasValue) { throw new ArgumentNullException(); }
            this.maxHeight = maxHeight.Value;
        }

        #endregion

    }
}
