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
using Lessium.ContentControls;
using Lessium.Services;

namespace Lessium.Models
{
    [Serializable]
    public class ContentPageModel : ILsnSerializable, IActionSender
    {
        // Public

        public const double PageWidth = 796d;
        public const double PageHeight = 637d;

        // Private

        private double maxWidth = PageWidth;
        private double maxHeight = PageHeight;

        private bool editable = false;
        private readonly IDispatcher dispatcher;

        // Serialization

        private List<IContentControl> storedItems;

        #region CLR Properties

        public double MaxWidth
        {
            get { return maxWidth; }
            set
            {
                if (maxWidth == value) { return; }

                maxWidth = value;

                foreach (var childControl in Items)
                {
                    childControl.SetMaxWidth(maxWidth);
                }
            }
        }

        public double MaxHeight
        {
            get { return maxHeight; }
            set
            {
                if (maxHeight == value) { return; }

                maxHeight = value;

                foreach (var childControl in Items)
                {
                    childControl.SetMaxHeight(maxHeight);
                }
            }
        }

        public ObservableCollection<IContentControl> Items { get; } = new ObservableCollection<IContentControl>();
        public ContentType ContentType { get; set; }

        #endregion

        #region Constructors

        public ContentPageModel(ContentType contentType, IDispatcher dispatcher = null)
        {
            this.ContentType = contentType;
            this.dispatcher = dispatcher ?? DispatcherUtility.Dispatcher;
        }

        // For serialization
        protected ContentPageModel(SerializationInfo info, StreamingContext context)
        {
            dispatcher =  DispatcherUtility.Dispatcher;

            storedItems = info.GetValue(nameof(Items), typeof(List<IContentControl>)) as List<IContentControl>;
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Adds IContentControl to the Items.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="directly">If set to true, will raise SendAction event instead of directly adding.</param>
        public void Add(IContentControl control, bool directly = false)
        {
            if (!IsContentControlTypeValid(control)) throw new InvalidOperationException ("You can only add ContentControls with equivalent interface!");

            // If there's no attached handlers, we will have to do it directly.

            if (SendAction == null) directly = true;

            // To skip unwanted validating.
            if (Items.Count == 0)
            {
                Insert(0, control, directly);
                return;
            }

            BindControl(control);

            if (directly)
            {
                Items.Add(control);
            }

            else
            {
                SendAction?.Invoke(this, new SendActionEventArgs(control, SendActionEventArgs.ContentAction.Add));
            }

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
        public void Insert(int position, IContentControl control, bool directly = false)
        {
            // If there's no attached handler, we will have to do it directly.

            if (SendAction == null) directly = true;

            BindControl(control);

            if (directly)
            {
                Items.Insert(position, control);
            }

            else
            {
                SendAction?.Invoke(this, new SendActionEventArgs(control, position));
            }

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

        public void Remove(IContentControl element, bool directly = false)
        {
            // If there's no attached handler, we will have to do it directly.

            if (SendAction == null) directly = true;

            element.RemoveControl -= OnRemove;
            element.Resize -= OnContentResized;

            if (directly)
            {
                Items.Remove(element);
            }

            else
            {
                var args = new SendActionEventArgs(element, SendActionEventArgs.ContentAction.Remove);
                SendAction?.Invoke(this, args);
            }
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

        public bool IsContentFits(IContentControl content)
        {
            return PageValidationHelperService.IsElementFits(this, content as FrameworkElement);
        }

        public void ValidatePage()
        {
            if (Items.Count > 0)
            {
                ValidateAllForward(Items[0], true);
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as ContentPageModel;

            if (other == null) return false;

            return EqualsHelper.AreEqual(Items, other.Items);
        }

        public override int GetHashCode()
        {
            int hash = 3;

            if (Items == null) return hash;
           
            hash = hash * 7 + Items.Count;

            for (int i = 0; i > Items.Count; i++)
            {
                hash = hash * 7 + Items[i].GetHashCode();
            }

            return hash;
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
                control.SetMaxWidth(MaxWidth);
                control.SetMaxHeight(MaxHeight);

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
                Add(item, true);
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

        public event ExceedingContentEventHandler AddedExceedingContent;

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
            info.AddValue(nameof(Items), Items.ToList());
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

                Add(control, true);
            }
        }

        #endregion

        #region IActionSender

        public event SendActionEventHandler SendAction;

        #endregion
    }

    public delegate void ExceedingContentEventHandler(object sender, ExceedingContentEventArgs args);

    public class ExceedingContentEventArgs : EventArgs
    {
        public IContentControl ExceedingItem { get; private set; }

        public ExceedingContentEventArgs(IContentControl ExceedingItem)
        {
            this.ExceedingItem = ExceedingItem;
        }
    }

    public delegate void SendActionEventHandler(object sender, SendActionEventArgs args);

    public class SendActionEventArgs : EventArgs
    {
        public IContentControl Content { get; private set; }
        public ContentAction Action { get; private set; }

        // Used for Insert
        public int Position { get; private set; } = -1;

        public SendActionEventArgs(IContentControl content, ContentAction action)
        {
            this.Content = content;
            this.Action = action;
        }

        public SendActionEventArgs(IContentControl content, int position)
        {
            this.Content = content;
            this.Action = ContentAction.Insert;
            this.Position = position;
        }

        public enum ContentAction
        {
            Add, Remove, Insert
        }
    }
}
