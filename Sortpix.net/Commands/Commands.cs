using System;
using System.Windows.Input;
using SortPix.ViewModels;

namespace SortPix.Commands
{
    internal class OpenSelectedPhotoCommand : ICommand
    {
        public OpenSelectedPhotoCommand(SortPixMainWindowsVM viewModel)
        {
            _ViewModel = viewModel;
        }
        private SortPixMainWindowsVM _ViewModel;

        #region ICommand Members
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            _ViewModel.OpenSelectedPhoto();
        }
        #endregion
    }
    internal class OpenJobFolderCommand : ICommand
    {
        public OpenJobFolderCommand(SortPixMainWindowsVM viewModel)
        {
            _ViewModel = viewModel;
        }
        private SortPixMainWindowsVM _ViewModel;

        #region ICommand Members
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            _ViewModel.OpenJobFolder();
        }
        #endregion
    }
    internal class BrowseSourceDirCommand : ICommand
    {
        public BrowseSourceDirCommand (SortPixMainWindowsVM viewModel)
        {
            _ViewModel = viewModel;
        }
        private SortPixMainWindowsVM _ViewModel;

        #region ICommand Members
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            _ViewModel.BrowseSourceDir();
        }
        #endregion
    }
    internal class BrowseJobFolderParentDirCommand : ICommand
    {
        public BrowseJobFolderParentDirCommand(SortPixMainWindowsVM viewModel)
        {
            _ViewModel = viewModel;
        }
        private SortPixMainWindowsVM _ViewModel;

        #region ICommand Members
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            _ViewModel.BrowseJobFolderParentDir();
        }
        #endregion
    }
    internal class JobNumberAddCharCommand : ICommand
    {
        public JobNumberAddCharCommand(SortPixMainWindowsVM viewModel)
        {
            _ViewModel = viewModel;
        }
        private SortPixMainWindowsVM _ViewModel;

        #region ICommand Members
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            _ViewModel.JobNumberAddChar(parameter.ToString());
        }
        #endregion
    }
    internal class JobNumberDelCharCommand : ICommand
    {
        public JobNumberDelCharCommand(SortPixMainWindowsVM viewModel)
        {
            _ViewModel = viewModel;
        }
        private SortPixMainWindowsVM _ViewModel;

        #region ICommand Members
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            _ViewModel.JobNumberDelChar();
        }
        #endregion
    }
    internal class ExitCommand : ICommand
    {
        public ExitCommand(SortPixMainWindowsVM viewModel)
        {
            _ViewModel = viewModel;
        }
        private SortPixMainWindowsVM _ViewModel;

        #region ICommand Members
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            _ViewModel.Exit();
        }
        #endregion
    }
}

