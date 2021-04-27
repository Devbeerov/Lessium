using Lessium.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Lessium.Utility;
using Lessium.Classes.IO;

namespace Lessium.ContentControls.Models
{
    // Model
    [Serializable]
    public class ContentPage : ILsnSerializable
    {
        // Public

        public const double PageWidth = 796d;
        public const double PageHeight = 637d;

        // Private

        private double maxWidth = PageWidth;
        private double maxHeight = PageHeight;

        private bool editable = false;
        private ContentPageControl pageControl;
        private IDispatcher dispatcher;

        // Serialization

        private List<IContentControl> storedItems;

        #region CLR Properties

        public ObservableCollection<IContentControl> Items { get; } = new ObservableCollection<IContentControl>();
        public ContentType ContentType { get; set; }

        #endregion

        #region Methods

        #region Public

        public ContentPage(ContentType contentType, IDispatcher dispatcher = null)
        {
            this.ContentType = contentType;
            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;
        }

        public static ContentPage CreateWithPageControlInjection(ContentPage oldPage, ContentType? contentType = null)
        {
            var newPage = new ContentPage(contentType ?? oldPage.ContentType);
            newPage.SetPageControl(oldPage.pageControl);
            return newPage;
        }

        public void Add(IContentControl control)
        {
            if (!IsContentControlTypeValid(control)) { throw new InvalidOperationException
                    ("You can only add ContentControls with equivalent interface!"); }

            // To skip unwanted validating.
            if (Items.Count == 0)
            {
                Insert(0, control);
                return;
            }

            BindControl(control);

            Items.Add(control);

            dispatcher.Invoke(() =>
            {
                // If control is already loaded (for example, moved from another page to this one), validates it
                if ((control as FrameworkElement).IsLoaded)
                {
                    ValidateContentPlacement(control);
                }
            });
        }

        // Won't validate control if it's in zero position.
        public void Insert(int position, IContentControl control)
        {
            BindControl(control);

            Items.Insert(position, control);

            dispatcher.Invoke(() =>
            {
                // If control is already loaded (for example, moved from another page to this one), validates forward
                if ((control as FrameworkElement).IsLoaded)
                {
                    bool ignore = position == 0; // Only skips check if inserted at zero position.
                    ValidateAllForward(control, ignore);
                }
            });
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
            if (maxWidth == width) { return; }

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

            foreach (var childControl in Items)
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

        #region Protected

        protected ContentPage(SerializationInfo info, StreamingContext context)
        {
            storedItems = info.GetValue("Items", typeof(List<IContentControl>)) as List<IContentControl>;
        }

        #endregion

        #region Private

        private void UpdateItemEditable(IContentControl contentControl)
        {
            if (contentControl != null)
            {
                dispatcher.Invoke(() =>
                {
                    contentControl.IsReadOnly = !editable;
                });
            }
        }

        private void UpdateItemsEditable()
        {
            foreach (var childControl in Items)
            {
                UpdateItemEditable(childControl);
            }
        }

        private bool IsContentControlTypeValid(IContentControl control)
        {
            bool isValid;
            switch (ContentType)
            {
                case ContentType.Material:
                    isValid = control is IMaterialControl;
                    break;
                case ContentType.Test:
                    isValid = control is ITestControl;
                    break;
                default:
                    throw new InvalidCastException("Invalid control type detected.");
            }
            return isValid;
        }

        /// <summary>
        /// Binds control to Page, updates its properties to match Page, also attaches event handlers
        /// </summary>
        /// <param name="control"></param>
        private void BindControl(IContentControl control)
        {
            dispatcher.Invoke(() =>
            {
                control.SetMaxWidth(maxWidth);
                control.SetMaxHeight(maxHeight);

                control.Resize += OnContentResized;
                control.RemoveControl += OnRemove;

                UpdateItemEditable(control);
            });
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            foreach (var item in storedItems)
            {
                Add(item);
            }

            // Clears and sets to null, so GC will collect List instance.

            storedItems.Clear();
            storedItems = null;
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
                    if (ignoreControl && pos == controlPos) { return; }
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

        #region ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Items", Items.ToList());
        }

        #endregion

        #region ILsnSerializable

        public async Task WriteXmlAsync(XmlWriter writer, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Reports to process new Page.

            progress.Report(ProgressType.Page);

            #region Page

            await writer.WriteStartElementAsync("Page");

            foreach (var item in Items)
            {
                if (token.HasValue && token.Value.IsCancellationRequested) { break; }

                await item.WriteXmlAsync(writer, progress, token);
            }

            await writer.WriteEndElementAsync();

            #endregion
        }

        public async Task ReadXmlAsync(XmlReader reader, IProgress<ProgressType> progress, CancellationToken? token)
        {
            // Converts ContentType to relative Namespace equivalent.
            string controlTypeNamespace = null;
            switch (ContentType)
            {
                case ContentType.Material:
                    controlTypeNamespace = "MaterialControls";
                    break;
                case ContentType.Test:
                    controlTypeNamespace = "TestControls";
                    break;
                default: throw new NotSupportedException($"{ContentType.ToString()} is not supported.");
            }

            // Reports to process new Page.

            progress.Report(ProgressType.Page);

            // Gets subtree reader to iterate over Page's childs.

            var subtreeReader = reader.ReadSubtree();
            while (await subtreeReader.ReadAsync())
            {
                if (token.HasValue && token.Value.IsCancellationRequested) break;
                if (subtreeReader.NodeType != XmlNodeType.Element || subtreeReader.Name == "Page") continue;

                var controlType = Type.GetType($"Lessium.ContentControls.{controlTypeNamespace}.{subtreeReader.Name}");
                if (controlType == null) { throw new InvalidDataException($"Invalid control type detected - {subtreeReader.Name}"); }

                var control = dispatcher.Invoke(() =>
                {
                    return (IContentControl)Activator.CreateInstance(controlType);
                });
                await control.ReadXmlAsync(subtreeReader, progress, token);

                Add(control);
            }
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
