using Lessium.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace Lessium.ContentControls
{
    public class Section : StackPanel
    {
        private bool editable = false;
        private SectionType sectionType;

        [Obsolete("Used only in XAML (constructor without parameters). Please use another constructor.", true)]
        public Section() : base()
        {
            Initialize(SectionType.MaterialSection);
        }

        public Section(SectionType type) : base()
        {
            Initialize(type);
        }

        // Custom Serialization
        public Section(SectionSerializationInfo info) : base()
        {
            Initialize(info.sectionType);

            SetTitle(info.title);
            editable = info.editable;
            items.AddRange(info.items);
        }

        private readonly ObservableCollection<IContentControl> items = new ObservableCollection<IContentControl>();

        #region Dependency Properties Methods

        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(Title);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(Title, value);
        }

        public string GetTitle()
        {
            return GetTitle(this);
        }

        public void SetTitle(string title)
        {
            SetTitle(this, title);
        }

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

        #region Methods

        #region Private

        private void UpdateItemEditable(object item)
        {
            var contentControl = item as IContentControl;

            if (contentControl != null)
            {
                contentControl.SetEditable(editable);
            }
        }

        private void UpdateItemsEditable()
        {
            foreach (object childControl in items)
            {
                UpdateItemEditable(childControl);
            }
        }

        #endregion

        #region Public

        public void Initialize(SectionType sectionType)
        {
            // Internal

            this.sectionType = sectionType;

            // Visible

            Width = double.NaN;
            Height = double.NaN;

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Orientation = Orientation.Vertical;

            // Sets Items reference to internal items variable.

            SetItems(items);
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

        public SectionSerializationInfo GetSerializationInfo()
        {
            SectionSerializationInfo info = new SectionSerializationInfo();

            info.title = GetTitle();
            info.editable = editable;
            info.items = items.ToList(); // Linq does copying
            info.sectionType = sectionType;

            return info;
        }

        #endregion

        #endregion

        #region Dependency Properties

        // Used externally.
        public static readonly DependencyProperty Title =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(Section), new PropertyMetadata(string.Empty));

        // Used externally.
        public static readonly DependencyProperty Items =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
                typeof(Section), new PropertyMetadata(null));

        #endregion

    }

    [Serializable]
    public class SectionSerializationInfo
    {
        public string title;
        public bool editable;
        public List<IContentControl> items; // COPY of List of ObservableCollection!
        public SectionType sectionType;
    }

    public enum SectionType
    {
        MaterialSection, TestSection
    }
}
