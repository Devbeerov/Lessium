using Lessium.Interfaces;
using Lessium.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Lessium.Services
{
    public class UndoableActionsService
    {
        #region Fields

        private LinkedList<IUndoableAction> executedActions = new LinkedList<IUndoableAction>();
        private LinkedList<IUndoableAction> undoneActions = new LinkedList<IUndoableAction>();

        private ushort limit = Settings.Default.UndoLimit;
        private readonly Action callback;

        #endregion

        #region Public Properties

        public int ExecutedActionsCount
        {
            get { return executedActions.Count; }
        }

        public int UndoneActionsCount
        {
            get { return undoneActions.Count; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates service for managing undo/redo operations.
        /// </summary>
        /// <param name="callback">Callback will be called on ExecuteAction and each successful Undo or Redo.</param>
        public UndoableActionsService(Action callback = null)
        {
            this.callback = callback;

            // Subscribes to limit change

            Settings.Default.PropertyChanged += OnSettingsPropertyChanged;
        }

        public void ExecuteAction(IUndoableAction action)
        {
            action.ExecuteDo();
            callback();

            AddAction(executedActions, action);
        }

        /// <summary>
        /// Tries to undo last action.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public bool TryUndo()
        {
            var lastActionExecuted = executedActions.Last?.Value;

            if (lastActionExecuted == null) return false;
            if (Keyboard.FocusedElement is IContentControl) return false; // No IContentControl should be focused.

            // Undo

            lastActionExecuted.Undo();
            executedActions.RemoveLast();

            // Adds to Undo

            AddAction(undoneActions, lastActionExecuted);

            // Invokes callback

            callback();

            return true;
        }

        /// <summary>
        /// Tries to redo last action.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public bool TryRedo()
        {
            var lastActionUndone = undoneActions.Last?.Value;

            if (lastActionUndone == null) return false;
            if (Keyboard.FocusedElement is IContentControl) return false; // No IContentControl should be focused.

            // Redo

            lastActionUndone.ExecuteDo();
            undoneActions.RemoveLast();

            // Adds to Executed

            AddAction(executedActions, lastActionUndone);

            // Invokes callback

            callback();

            return true;
        }

        public void Clear()
        {
            executedActions.Clear();
            undoneActions.Clear();
        }

        #endregion

        #region Private Methods

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Checks if changed Property is UndoLimit, returns if not

            if (e.PropertyName != nameof(Settings.Default.UndoLimit)) return;

            UpdateLimit();
        }

        private void UpdateLimit()
        {
            if (limit == Settings.Default.UndoLimit) return;

            limit = Settings.Default.UndoLimit;

            RemoveExcess();
        }

        private void RemoveExcess()
        {
            RemoveExcess(executedActions);
            RemoveExcess(undoneActions);
        }

        /// <summary>
        /// Removes excess actions beyond limit.
        /// </summary>
        /// <param name="actions"></param>
        private void RemoveExcess(LinkedList<IUndoableAction> actions)
        {
            // Returns if no excess.

            if (limit >= actions.Count) return;

            // Calculates difference between limit and actual count.

            var difference = Math.Abs(limit - actions.Count);

            // Removes actions from the end.

            for (int i = 0; i < difference; i++)
            {
                actions.RemoveLast();
            }
        }

        private void AddAction(LinkedList<IUndoableAction> actions, IUndoableAction action)
        {
            if (actions.Count >= limit) return;

            // If fits to limit, adds to relative actions

            actions.AddLast(action);
        }

        #endregion
    }
}
