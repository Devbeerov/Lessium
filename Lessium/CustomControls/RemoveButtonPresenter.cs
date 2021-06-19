using Lessium.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.CustomControls
{
    public class RemoveButtonPresenter : ContentPresenter
    {
        public RemoveButtonPresenter() : base()
        {
            Height = 16d;
            Width = 16d;

            WeakEventManager<RemoveButtonPresenter, RoutedEventArgs>.AddHandler(this, nameof(Loaded), OnLoaded);
        }

        #region Events

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            var bind = !GetUsesCustomRemoveOnClick(Parent);
            var button = ContentPageControlService.RequestRemoveButtonCopy(bind);

            HorizontalAlignment = button.HorizontalAlignment;
            VerticalAlignment = button.VerticalAlignment;

            button.Width = ActualWidth;
            button.Height = ActualHeight;

            Content = button;
            WeakEventManager<Button, RoutedEventArgs>.AddHandler(button, nameof(Button.Click), OnButtonClick);
        }

        private void OnButtonClick(object sender, RoutedEventArgs args)
        {
            var newArgs = new RoutedEventArgs(OnClickEvent, sender);

            RaiseEvent(newArgs);
        }

        #endregion

        #region Routed Events

        public event RoutedEventHandler OnClick
        {
            add
            {
                AddHandler(OnClickEvent, value);
            }

            remove
            {
                RemoveHandler(OnClickEvent, value);
            }
        }

        public static void AddOnClickHandler(DependencyObject element, RoutedEventHandler handler)
        {
            var uiElement = element as UIElement;

            uiElement.AddHandler(OnClickEvent, handler);
        }

        public static void RemoveOnClickHandler(DependencyObject element, RoutedEventHandler handler)
        {
            var uiElement = element as UIElement;

            uiElement.RemoveHandler(OnClickEvent, handler);
        }

        public static readonly RoutedEvent OnClickEvent =
            EventManager.RegisterRoutedEvent("OnClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RemoveButtonPresenter));

        #endregion

        #region Dependency Properties

        public static void SetUsesCustomRemoveOnClick(DependencyObject obj, bool value)
        {
            obj.SetValue(UsesCustomRemoveOnClickProperty, value);
        }

        public static bool GetUsesCustomRemoveOnClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(UsesCustomRemoveOnClickProperty);
        }

        /// <summary>
        /// If set to true, will not bind Button to use default hanlder, which will just remove Control entirely.
        /// </summary>
        public static readonly DependencyProperty UsesCustomRemoveOnClickProperty =
            DependencyProperty.RegisterAttached("UsesCustomRemoveOnClick", typeof(bool), typeof(RemoveButtonPresenter), new PropertyMetadata(false,
                new PropertyChangedCallback(OnUsesCustomRemoveOnClickChanged)));

        private static void OnUsesCustomRemoveOnClickChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var element = sender as FrameworkElement;

            if (element == null) throw new NotSupportedException("Sender should be FrameworkElement.");
            if (element.IsLoaded) throw new NotSupportedException("Should be setup before loaded!");

            var parentPanel = element.Parent as Panel;
            foreach (var child in parentPanel)
            {
                if (child is RemoveButtonPresenter presenter)
                {
                    presenter.SetValue(UsesCustomRemoveOnClickProperty, args.NewValue);
                    break;
                }
            }
        }

        #endregion
    }
}
