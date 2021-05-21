using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System;

namespace Lessium.Utility
{
    public class WidthToMaxWidthBehavior : Behavior<FrameworkElement>
    {
        public FrameworkElement ElementToSubstractWidth;

        protected override void OnAttached()
        {
            if (AssociatedObject != null)
            {
                base.OnAttached();

                var maxWidthDescriptor = DependencyPropertyDescriptor.FromProperty(FrameworkElement.MaxWidthProperty, typeof(double));

                //maxWidthDescriptor.AddValueChanged(AssociatedObject, new DependencyPropertyChangedEventHandler(OnWidthChanged);
            }
        }


        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                
                base.OnDetaching();
            }
        }

        private void OnWidthChanged(object sender, DependencyPropertyChangedEventArgs args)
        {

        }
    }
}
