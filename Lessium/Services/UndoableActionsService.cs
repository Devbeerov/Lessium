using Lessium.Interfaces;
using Lessium.Properties;
using Lessium.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Lessium.Services
{
    public class UndoableActionsService
    {
        #region Constructors

        /// <summary>
        /// Create service for managing undo/redo operations.
        /// </summary>
        /// <param name="genericCallback">genericCallback will be called on ExecuteAction and each successful Undo or Redo.</param>
        /// <param name="registerService">If true, will register service automatically in UndoableActionsServiceLocator.</param>
        public UndoableActionsService(Window window, Action genericCallback = null, bool registerService = true)
        {
            this.genericCallback = genericCallback;

            // Register service to access it from any DependencyObject which have Window parent.

            if (registerService) UndoableActionsServiceLocator.RegisterService(this, window);

            // Subscribes to limit change

            Settings.Default.PropertyChanged += OnSettingsPropertyChanged;
        }

        #endregion

        #region Fields

        private LinkedList<IUndoableAction> executedActions = new LinkedList<IUndoableAction>();
        private LinkedList<IUndoableAction> undoneActions = new LinkedList<IUndoableAction>();

        private ushort limit = Settings.Default.UndoLimit;
        private readonly Action genericCallback;

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

        public void ExecuteAction(IUndoableAction action)
        {
            ExecuteActionInternal(action);
        }

        /// <summary>
        /// Tries to undo last action.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public bool TryUndo()
        {
            if (executedActions.Last == null) return false;

            var lastActionExecuted = executedActions.Last.Value;

            if (Keyboard.FocusedElement is IContentControl) return false; // No IContentControl should be focused.

            // Undo

            UndoActionInternal(lastActionExecuted);

            // Removes undone action (last) from executed.

            executedActions.RemoveLast();

            return true;
        }

        /// <summary>
        /// Tries to redo last action.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        public bool TryRedo()
        {
            if (undoneActions.Last == null) return false;

            var lastActionUndone = undoneActions.Last.Value;

            if (Keyboard.FocusedElement is IContentControl) return false; // No IContentControl should be focused.

            // Redo

            ExecuteActionInternal(lastActionUndone);

            // Removes executed action (last) from undone.

            undoneActions.RemoveLast();

            return true;
        }

        public void Clear()
        {
            executedActions.Clear();
            undoneActions.Clear();
        }

        #endregion

        #region Private Methods

        private void ExecuteActionInternal(IUndoableAction action)
        {
            action.ExecuteDo();
            genericCallback?.Invoke();

            AddAction(executedActions, action);
        }

        private void UndoActionInternal(IUndoableAction action)
        {
            action.Undo();
            genericCallback?.Invoke();

            AddAction(undoneActions, action);
        }

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
            executedActions.RemoveExcess(limit);
            undoneActions.RemoveExcess(limit);
        }

        private void AddAction(LinkedList<IUndoableAction> actionsList, IUndoableAction action)
        {
            if (actionsList.Count >= limit) return;

            // If fits to limit, adds to relative actionsList

            actionsList.AddLast(action);
        }

        #endregion
    }
}
