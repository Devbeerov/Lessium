using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System;

namespace Lessium.CustomControls
{
    public class ListBoxExtended : ListBox
    {
        private const string templateBorderName = "Bd";
        private ControlTemplate defaultTemplate;
        private ControlTemplate noSelectionTemplate;

        #region Constructors

        public ListBoxExtended()
        {
            Loaded += (s, a) =>
            {
                Setup();
            };
        }

        #endregion

        #region Private Methods

        private void Setup()
        {
            SetupTemplates();
        }

        //private void AddTriggers(ControlT style)
        //{
        //    var triggers = Template.Triggers;

        //    foreach (var trigger in triggers.OfType<Trigger>()) // TriggerBase doesn't contain Setters, unlike Trigger.
        //    {
        //        foreach (var setter in trigger.Setters.OfType<Setter>())
        //        {
        //            if (setter.TargetName != templateBorderName) continue;

        //            style.Triggers.Add(trigger);

        //            break;
        //        }
        //    }
        //}

        private void RemoveSetters(ControlTemplate template)
        {
            var triggers = template.Triggers;

            foreach (var trigger in triggers.OfType<Trigger>()) // TriggerBase doesn't contain Setters, unlike Trigger.
            {
                foreach (var setter in trigger.Setters.OfType<Setter>())
                {
                    if (setter.TargetName != templateBorderName) continue;
                    
                    trigger.Setters.Remove(setter);
                }
            }
        }

        //override template

        private void SetupTemplates()
        {
            defaultTemplate = Template;
            var t = defaultTemplate.LoadContent();

            noSelectionTemplate = new ControlTemplate(typeof(ListBoxExtended));
            //noSelectionTemplate.
            //noSelectionTemplate.Template = defaultTemplate.Template; // Looks weird, but it's TemplateContent

            foreach (var trigger in defaultTemplate.Triggers)
            {
                noSelectionTemplate.Triggers.Add(trigger);
            }

            var t2 = noSelectionTemplate.LoadContent();

            RemoveSetters(noSelectionTemplate);
        }

        private void RestoreSelection()
        {
            //Template = defaultTemplate;
        }

        private void RemoveSelection()
        {
        //    RemoveSetters();
            //Template = noSelectionTemplate;
        }

        #endregion

        #region Dependency Properties

        public bool ItemsSelectionEnabled
        {
            get { return (bool)GetValue(ItemsSelectionEnabledProperty); }
            set { SetValue(ItemsSelectionEnabledProperty, value); }
        }

        public static readonly DependencyProperty ItemsSelectionEnabledProperty =
            DependencyProperty.Register("ItemsSelectionEnabled", typeof(bool), typeof(ListBoxExtended), new PropertyMetadata(true, 
                new PropertyChangedCallback(OnItemsSelectionStateChanged)));

        private static void OnItemsSelectionStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBoxExtended;
            var enabled = (bool)e.NewValue;

            if (enabled)
            {
                listBox.RestoreSelection();
                return;
            }

            listBox.RemoveSelection();
        }

        #endregion
    }
}
