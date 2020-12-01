﻿using Lessium.ContentControls.Models;
using Lessium.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.ContentControls
{
    public class ContentPageControl : WrapPanel
    {

        [Obsolete("This constructed used for creating control in XAML. Use constructor with model instead.", true)]
        public ContentPageControl() : base()
        {
            Initialize(null);
        }

        public ContentPageControl(ContentPage model) : base()
        {
            Initialize(model);
        }

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

        public void Initialize(ContentPage model)
        {
            // Model

            if (model == null)
            {
                model = new ContentPage();
            }

            // Visual

            Width = ContentPage.PageWidth;
            Height = ContentPage.PageHeight;

            Orientation = Orientation.Vertical;

            // Sets Items property to model items

            SetItems(model.Items);
        }

        #endregion

        //#region Overrides

        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    base.OnRenderSizeChanged(sizeInfo);

        //    var items = GetItems();

        //    foreach(var item in items)
        //    {
        //        if (sizeInfo.WidthChanged)
        //        {
        //            item.SetMaxWidth(sizeInfo.NewSize.Width);
        //        }

        //        if (sizeInfo.HeightChanged)
        //        {
        //            item.SetMaxHeight(sizeInfo.NewSize.Height);
        //        }
        //    }
        //}

        //#endregion

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty Items =
            DependencyProperty.Register("Items", typeof(ObservableCollection<IContentControl>),
            typeof(ContentPageControl), new PropertyMetadata(null));

        #endregion
    }
}
