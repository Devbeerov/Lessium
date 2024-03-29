﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lessium.Utility
{
    public static class WpfExtensions
    {
        public static T FindParent<T>(this DependencyObject child) where T : class
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            var parent = parentObject as T;

            if (parent != null) return parent;

            return FindParent<T>(parentObject);
        }

        public static FrameworkElement FindParentOfName(this DependencyObject child, string parentName)
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            var parent = parentObject as FrameworkElement;

            if (parent != null && parent.Name == parentName) return parent;

            return FindParentOfName(parentObject, parentName);
        }

        #region TextBox

        public static double CalculateLineHeight(this TextBox textBox)
        {
            var formattedText = new FormattedText
            (
                "1",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBox.FontFamily, textBox.FontStyle,
                textBox.FontWeight, textBox.FontStretch),
                textBox.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1
            );

            return formattedText.Height;
        }

        public static Size MeasureText(this TextBlock textBlock)
        {
            var typeface = new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);

            return MeasureText(textBlock.Text, typeface, textBlock.FontSize);
        }

        public static Size MeasureText(this TextBox textBox)
        {
            var typeface = new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight, textBox.FontStretch);

            return MeasureText(textBox.Text, typeface, textBox.FontSize);
        }

        public static Size MeasureText(string text, Typeface typeface, double fontSize)
        {
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return new Size(formattedText.Width, formattedText.Height);
        }

        #endregion

        #region Attached Properties

        public static readonly DependencyProperty DynamicContentKeyProperty = DependencyProperty.RegisterAttached("DynamicContentKey",
            typeof(object), typeof(WpfExtensions), new PropertyMetadata(null, DynamicContentKeyChanged));

        public static readonly DependencyProperty DynamicContentResourceDictionaryProperty = DependencyProperty.RegisterAttached("DynamicContentResourceDictionary",
            typeof(ResourceDictionary), typeof(WpfExtensions), new PropertyMetadata(null, DynamicContentResourceDictionaryChanged));

        public static readonly RoutedEvent DynamicContentChangedEvent = EventManager.RegisterRoutedEvent("DynamicContentChanged",
            RoutingStrategy.Bubble, typeof(DynamicContentEventHandler), typeof(WpfExtensions));

        public static void AddDynamicContentChangedHandler(DependencyObject element, DynamicContentEventHandler handler)
        {
            var uiElement = element as UIElement;

            uiElement.AddHandler(DynamicContentChangedEvent, handler);
        }

        public static void RemoveDynamicContentChangedHandler(DependencyObject element, DynamicContentEventHandler handler)
        {
            var uiElement = element as UIElement;

            uiElement.RemoveHandler(DynamicContentChangedEvent, handler);
        }

        private static void DynamicContentKeyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var resourceDictionary = GetDynamicContentResourceDictionary(obj);

            if (resourceDictionary == null)
            {
                var element = obj as FrameworkElement;

                // If not FrameworkElement or not loaded yet, just returns.

                if (element == null || !element.IsLoaded) return;

                // Otherwise, throws an error.

                throw new NullReferenceException($"{nameof(DynamicContentResourceDictionaryProperty)} should not be null!");
            }

            var control = (ContentControl)obj;
            var resourceKey = e.NewValue as string;

            // Presenter in ResourceDictionary associated with resourceKey.

            var associatedControl = resourceDictionary[resourceKey];
            var oldContent = control.GetValue(ContentControl.ContentProperty);

            // Sets ContentProperty to associatedControl (dynamic) resource.

            control.SetValue(ContentControl.ContentProperty, associatedControl);

            // Raises event
            var args = new DynamicContentChangedArgs(oldContent, associatedControl);
            control.RaiseEvent(args);
        }

        private static void DynamicContentResourceDictionaryChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // Invalidates DynamicContentKey, therefore it will be updated as DynamicContentResourceDictionary changed.

            obj.InvalidateProperty(DynamicContentKeyProperty);
        }

        public static void SetDynamicContentKey(DependencyObject obj, object value)
        {
            obj.SetValue(DynamicContentKeyProperty, value);
        }

        public static object GetDynamicContentKey(DependencyObject obj)
        {
            return obj.GetValue(DynamicContentKeyProperty);
        }

        public static void SetDynamicContentResourceDictionary(DependencyObject obj, ResourceDictionary dictionary)
        {
            obj.SetValue(DynamicContentResourceDictionaryProperty, dictionary);
        }

        public static ResourceDictionary GetDynamicContentResourceDictionary(DependencyObject obj)
        {
            return obj.GetValue(DynamicContentResourceDictionaryProperty) as ResourceDictionary;
        }

        #endregion
    }

    public delegate void DynamicContentEventHandler(object sender, DynamicContentChangedArgs e);

    public class DynamicContentChangedArgs : RoutedEventArgs
    {
        public object OldContent { get; private set; }
        public object NewContent { get; private set; }

        public DynamicContentChangedArgs(object oldContent, object newContent)
        {
            OldContent = oldContent;
            NewContent = newContent;

            base.RoutedEvent = WpfExtensions.DynamicContentChangedEvent;
        }
    }
}
