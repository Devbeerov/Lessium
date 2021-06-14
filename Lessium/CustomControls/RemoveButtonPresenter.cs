using Lessium.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.CustomControls
{
    public class RemoveButtonPresenter : ContentPresenter
    {
        private static void OnRequestRemoveButtonChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var presenter = sender as RemoveButtonPresenter;

            //if (presenter.RequestRemoveButton == null) return;

            presenter.RequestRemoveButton?.Invoke(new RemoveButtonRequestEventArgs(presenter));
        }

        public RemoveButtonRequestEventHandler RequestRemoveButton
        {
            get { return (RemoveButtonRequestEventHandler)GetValue(RequestRemoveButtonProperty); }
            set { SetValue(RequestRemoveButtonProperty, value); }
        }

        public static readonly DependencyProperty RequestRemoveButtonProperty =
            DependencyProperty.Register("RequestRemoveButton", typeof(RemoveButtonRequestEventHandler), typeof(RemoveButtonPresenter), new PropertyMetadata(null, new PropertyChangedCallback(OnRequestRemoveButtonChanged)));
    }
}
