using Lessium.Services;
using System.Windows;
using System.Windows.Controls;

namespace Lessium.CustomControls
{
    public class RemoveButtonPresenter : ContentPresenter
    {
        public RemoveButtonPresenter() : base()
        {
            WeakEventManager<RemoveButtonPresenter, RoutedEventArgs>.AddHandler(this, nameof(Loaded), OnLoaded);
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            var button = ContentPageControlService.RequestRemoveButtonCopy();

            HorizontalAlignment = button.HorizontalAlignment;
            VerticalAlignment = button.VerticalAlignment;

            button.Width = ActualWidth;
            button.Height = ActualHeight;

            Content = button;
        }
    }
}
