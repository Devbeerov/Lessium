﻿using System.Linq;
using System.Windows;
using System.Windows.Interactivity;

namespace Lessium.Utility
{
    public static class BehaviorUtility
    {
        public static void RemoveBehavior<T>(this DependencyObject obj) where T : Behavior
        {
            var behaviors = Interaction.GetBehaviors(obj);
            var behavior = GetBehavior<T>(obj);
            if (behavior != null)
            {
                behaviors.Remove(behavior);
            }
        }

        public static T GetBehavior<T>(this DependencyObject obj) where T : Behavior
        {
            var behaviors = Interaction.GetBehaviors(obj);
            return behaviors.OfType<T>().FirstOrDefault();
        }
    }
}
