﻿using Lessium.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Controls;

namespace Lessium.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private properties

        private MainWindowModel model;

        #endregion

        #region Public CLR Properties

        #region Program

        public string Title
        {
            get { return model.title; }
            set { SetProperty(ref model.title, value); }
        }

        #endregion

        #region LessonMenu

        public string LessonHeader
        {
            get { return model.LessonHeader; }
            set { SetProperty(ref model.LessonHeader, value); }
        }

        public string EditHeader
        {
            get { return model.EditHeader; }
            set { SetProperty(ref model.EditHeader, value); }
        }

        public string UndoChangesHeader
        {
            get { return model.UndoChangesHeader; }
            set { SetProperty(ref model.UndoChangesHeader, value); }
        }

        public string RecentHeader
        {
            get { return model.RecentHeader; }
            set { SetProperty(ref model.RecentHeader, value); }
        }

        public string NewLessonHeader
        {
            get { return model.NewLessonHeader; }
            set { SetProperty(ref model.NewLessonHeader, value); }
        }

        public string SaveLessonHeader
        {
            get { return model.SaveLessonHeader; }
            set { SetProperty(ref model.SaveLessonHeader, value); }
        }

        public string LoadLessonHeader
        {
            get { return model.LoadLessonHeader; }
            set { SetProperty(ref model.LoadLessonHeader, value); }
        }

        public string CloseLessonHeader
        {
            get { return model.CloseLessonHeader; }
            set { SetProperty(ref model.CloseLessonHeader, value); }
        }

        public string PrintLessonHeader
        {
            get { return model.PrintLessonHeader; }
            set { SetProperty(ref model.PrintLessonHeader, value); }
        }

        public string ExitHeader
        {
            get { return model.ExitHeader; }
            set { SetProperty(ref model.ExitHeader, value); }
        }

        #region Internal

        public bool HasChanges
        {
            get { return model.HasChanges; }
            set { SetProperty(ref model.HasChanges, value); }
        }

        public bool ReadOnly
        {
            get { return model.ReadOnly; }
            set { SetProperty(ref model.ReadOnly, value); }
        }


        #endregion

        #endregion

        #region Tabs

        public string MaterialHeader
        {
            get { return model.MaterialHeader; }
            set { SetProperty(ref model.MaterialHeader, value); }
        }

        public string TestHeader
        {
            get { return model.TestHeader; }
            set { SetProperty(ref model.TestHeader, value); }
        }

        #endregion

        #endregion

        #region Methods

        // Constructs ViewModel with Model as parameter.
        public MainWindowViewModel(MainWindowModel model = null)
        {
            // In case we don't provide model (for example when Prism wires ViewModel automatically), creates new Model.

            if(model == null)
            {
                model = new MainWindowModel();
            }

            this.model = model;
        }

        #endregion

        #region Commands

        // Lesson_EditCommand

        private DelegateCommand Lesson_EditCommand;
        public DelegateCommand Lesson_Edit =>
            Lesson_EditCommand ?? (Lesson_EditCommand = new DelegateCommand(ExecuteLesson_Edit, CanExecuteLesson_Edit));

        void ExecuteLesson_Edit()
        {
            ReadOnly = false;
        }

        // Enables editing only when at ReadOnly
        bool CanExecuteLesson_Edit()
        {
            return ReadOnly;
        }

        // Lesson_UndoChangesCommand

        private DelegateCommand Lesson_UndoChangesCommand;
        public DelegateCommand Lesson_UndoChanges =>
            Lesson_UndoChangesCommand ?? (Lesson_UndoChangesCommand = new DelegateCommand(ExecuteLesson_UndoChanges, CanExecuteLesson_UndoChanges));

        void ExecuteLesson_UndoChanges()
        {
            HasChanges = false;
        }

        bool CanExecuteLesson_UndoChanges()
        {
            return HasChanges;
        }

        // Lesson_SaveCommand

        private DelegateCommand Lesson_SaveCommand;
        public DelegateCommand Lesson_Save =>
            Lesson_SaveCommand ?? (Lesson_SaveCommand = new DelegateCommand(ExecuteLesson_Save, CanExecuteLesson_Save));

        void ExecuteLesson_Save()
        {
            // TODO: Implement save
            HasChanges = false;
        }

        bool CanExecuteLesson_Save()
        {
            return !ReadOnly && HasChanges;
        }

        // Lesson_NewCommand

        private DelegateCommand Lesson_NewCommand;
        public DelegateCommand Lesson_New =>
            Lesson_NewCommand ?? (Lesson_NewCommand = new DelegateCommand(ExecuteLesson_New));

        void ExecuteLesson_New()
        {
            // TODO: Implement new lesson
            HasChanges = true;
        }

        #endregion
    }
}
