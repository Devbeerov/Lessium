using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using TriggerAction = System.Windows.TriggerAction;

namespace Lessium.Utility.Behaviors
{
    public class NoSelectionListBoxBehavior : Behavior<ListBox>
    {

        public DataTrigger Trigger
        {
            get { return (DataTrigger)GetValue(TriggerProperty); }
            set { SetValue(TriggerProperty, value); }
        }

        public static readonly DependencyProperty TriggerProperty =
            DependencyProperty.Register("Trigger", typeof(DataTrigger), typeof(NoSelectionListBoxBehavior), new PropertyMetadata(null,
                new PropertyChangedCallback(OnTriggerChanged)));

        private static void OnTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBox;
            var newTrigger = e.NewValue as DataTrigger;

            if (newTrigger == null) return;

            newTrigger.EnterActions.Add(new InvokerTriggerAction());
            // setters. add, setters remove
            var template = listBox.Template;

            var triggers = template.Triggers;


            foreach (var trigger in triggers)
            {

            }


            //var newStyle = new Style(typeof(ListBox), listBox.Style);
            //newStyle.Triggers.ad
        }
    }
}
