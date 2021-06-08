using System;
using System.Windows;
using System.Windows.Interactivity;

namespace Lessium.Utility
{
    public class InvokerTriggerAction : TriggerAction<DependencyObject>
    {
        public InvokerTriggerAction()
        {

        }

        public InvokerTriggerAction(Action action)
        {
            InvokingAction = action;
        }

        protected override void Invoke(object parameter)
        {
            InvokingAction?.Invoke();
        }

        public Action InvokingAction
        {
            get { return (Action)GetValue(InvokingActionProperty); }
            set { SetValue(InvokingActionProperty, value); }
        }

        public static readonly DependencyProperty InvokingActionProperty =
            DependencyProperty.Register("InvokingAction", typeof(Action), typeof(InvokerTriggerAction), new PropertyMetadata(null));

    }
}
