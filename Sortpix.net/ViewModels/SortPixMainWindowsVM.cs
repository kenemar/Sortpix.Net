using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using SortPix.Commands;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Threading;

namespace SortPix.ViewModels
{
    public class SortPixMainWindowsVM : INotifyPropertyChanged
    {
        public SortPixMainWindowsVM()
        {
            SourceDirPath = Properties.Settings1.Default.SourceDirPath;
            JobFolderParentDirPath = Properties.Settings1.Default.JobFolderParentDirPath;
            JobNumberPrefix = Properties.Settings1.Default.JobNumberPrefix;
            JobNumber = Properties.Settings1.Default.JobNumber;
            JobNumberSuffix = Properties.Settings1.Default.JobNumberSuffix;
            MoveToSubfolder = Properties.Settings1.Default.MoveToSubfolder;
            StartAtRight = Properties.Settings1.Default.StartAtRight;

            OpenSelectedPhotoCommand = new OpenSelectedPhotoCommand(this);
            OpenJobFolderCommand = new OpenJobFolderCommand(this);
            BrowseSourceDirCommand = new BrowseSourceDirCommand(this);
            BrowseJobFolderParentDirCommand = new BrowseJobFolderParentDirCommand(this);
            JobNumberAddCharCommand = new JobNumberAddCharCommand(this);
            JobNumberDelCharCommand = new JobNumberDelCharCommand(this);
            ExitCommand = new ExitCommand(this);

            MonitorSourceDir();
            MonitorDestDir();

        }
        #region Private Fields
        private ObservableCollection<string> _photoPaths;
        private string _sourceDirPath;
        private string _jobNumber;
        private bool _pathsValid;
        private readonly string[] JobPhotoLocations = { "\\pictures", "\\photos", "\\pics" };
        #endregion

        #region Public Properties
        public string JobNumber
        {
            get => _jobNumber;
            set
            {
                _jobNumber = value;
                OnPropertyChanged(nameof(PathsValid));
                OnPropertyChanged(nameof(PathsValidColor));
                if (Directory.Exists(JobFolderPath))
                {
                    Task.Run(() => GetPhotoPathsAsync(JobFolderPath));
                    //Task.Run(() => { Dispatcher.InvokeAsync(() => LongOperation())});
                    MonitorDestDir();
                    //GetPhotoPathsAsync(JobFolderPath);                   
                }
                else PhotoPaths = new ObservableCollection<string>();           
            }
        }
        public string PathsValid
        {
            get
            {
                if (Directory.Exists(JobFolderPath))
                {
                    _pathsValid = true;
                    OnPropertyChanged(nameof(PathsValidColor));
                    return "Job number & folders OK. Ready to sort pictures.";
                }
                else
                {
                    _pathsValid = false;
                    OnPropertyChanged(nameof(PathsValidColor));
                    return "Folders are Invalid/Inaccessible. Check path names and job number.";
                }   
            }
        }
        public Brush PathsValidColor
        {
            get
            {
                if (_pathsValid)
                {
                    return Brushes.Green;
                }
                else
                {
                    return Brushes.Red;
                }
            }
        }
        public string SourceDirPath
        {
            get => _sourceDirPath;
            set
            {
                _sourceDirPath = value;
                if(sourceDirWatcher != null)
                {
                    sourceDirWatcher.Path = value;
                } else
                {
                    MonitorSourceDir();
                }
            }
        }
        public string JobFolderParentDirPath { get; set; }
        public string JobNumberPrefix { get; set; }
        public string JobNumberSuffix { get; set; }
        public string JobFolderPath
        {
            get
            {
                return JobFolderParentDirPath + "\\" + JobNumberPrefix + JobNumber + JobNumberSuffix;
            }

        }
        public string PhotoFolderPath
        {
            get
            {
                if (!MoveToSubfolder) { return JobFolderPath; }
                else
                {
                    foreach(string location in JobPhotoLocations)
                    {
                        if (Directory.Exists(JobFolderPath + location))
                        {
                            return JobFolderPath + location;
                        }
                    }
                    _ = Directory.CreateDirectory(JobFolderPath + "\\Pictures");
                    return JobFolderPath + "\\Pictures";
                }
            }
        }
        public ObservableCollection<string> PhotoPaths
        {
            get
            {
                return _photoPaths;      
            }
            private set
            {
                _photoPaths = value;
                OnPropertyChanged(nameof(PhotoPaths));
            }
        }
        public string SelectedPhoto { get; set; }
        public bool MoveToSubfolder { get; set; }
        public bool StartAtRight { get; set; }
        #endregion

        #region Methods
        private void GetPhotoPathsAsync(string path)
        {
            
            try
            {
                var photoPathsList = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg")).OrderByDescending(d => new FileInfo(d).CreationTime).Take(4);
                PhotoPaths = new ObservableCollection<string>(photoPathsList);
            }
            catch
            {
                PhotoPaths = new ObservableCollection<string>();
            }
            
        }
        private FileSystemWatcher sourceDirWatcher;
        private FileSystemWatcher destDirWatcher;
        public void MonitorSourceDir()
        {
            if(Directory.Exists(SourceDirPath))
            {
                if (sourceDirWatcher == null)
                {
                    sourceDirWatcher = new FileSystemWatcher();
                    sourceDirWatcher.Path = SourceDirPath;
                    sourceDirWatcher.NotifyFilter = NotifyFilters.FileName;
                    sourceDirWatcher.Created += OnCreated;
                    sourceDirWatcher.EnableRaisingEvents = true;
                }
                sourceDirWatcher.Path = SourceDirPath;
            }
            
        }
        public void MonitorDestDir()
        {
            if(Directory.Exists(JobFolderPath))
            {
                if (destDirWatcher == null)
                {
                    destDirWatcher = new FileSystemWatcher();
                    destDirWatcher.Path = JobFolderPath;
                    destDirWatcher.IncludeSubdirectories = true;
                    destDirWatcher.NotifyFilter = NotifyFilters.FileName;
                    destDirWatcher.Deleted += OnDeleted;
                    destDirWatcher.EnableRaisingEvents = true;
                }
                destDirWatcher.Path = JobFolderPath;
            }
            
        }
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            //string value = $"Created: {e.FullPath}";
            //Console.WriteLine("new File: " + value);
            //MessageBox.Show("New File: " + value);
            //File.Move(e.FullPath, PhotoFolderPath + "\\" + e.Name);

            //int index = 0;
            //while(index < 5)
            //{
            //    if (!IsFileLocked(e.FullPath) && !File.Exists(PhotoFolderPath + "\\" + e.Name))
            //    {
            //        System.Threading.Thread.Sleep(1000);
            //        File.Move(e.FullPath, PhotoFolderPath + "\\" + e.Name);
            //        return;
            //    }
            //    index++;
            //    System.Threading.Thread.Sleep(200);
            //}
            //System.Threading.Thread.Sleep(1000);
            //File.Move(e.FullPath, PhotoFolderPath + "\\" + e.Name);
            Task.Run(() => MovePhoto(e.FullPath, PhotoFolderPath + "\\" + e.Name));
            //MessageBox.Show("hey, new pic");
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Task.Run(() => GetPhotoPathsAsync(JobFolderPath));
        }
        private static bool IsFileLocked(string filePath)
        {
            try
            {
                //using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;

        }
        public void MovePhoto(string sourceFilePath, string destFilePath)
        {
            sourceDirWatcher.EnableRaisingEvents = false;
            int index = 0;
            while (index < 5)
            {
                if (!IsFileLocked(sourceFilePath) && !File.Exists(destFilePath))
                {
                    System.Threading.Thread.Sleep(1000);
                    File.Move(sourceFilePath, destFilePath);
                    Task.Run(() => GetPhotoPathsAsync(JobFolderPath));
                    //OnPropertyChanged(nameof(PhotoPaths));
                    sourceDirWatcher.EnableRaisingEvents = true;
                    return;
                }
                index++;
                System.Threading.Thread.Sleep(200);
            }
            sourceDirWatcher.EnableRaisingEvents = true;
        }
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Properties.Settings1.Default.JobNumber = JobNumber;
            Properties.Settings1.Default.SourceDirPath = SourceDirPath;
            Properties.Settings1.Default.JobFolderParentDirPath = JobFolderParentDirPath;
            Properties.Settings1.Default.JobNumberPrefix = JobNumberPrefix;
            Properties.Settings1.Default.JobNumberSuffix = JobNumberSuffix;
            Properties.Settings1.Default.MoveToSubfolder = MoveToSubfolder;
            Properties.Settings1.Default.StartAtRight = StartAtRight;
            Properties.Settings1.Default.Save();
        }
        public void OpenSelectedPhoto()
        {
            if(SelectedPhoto != null)
            {
                Process openPhoto = new Process();
                openPhoto.StartInfo = new ProcessStartInfo(SelectedPhoto)
                {
                    UseShellExecute = true
                };
                openPhoto.Start();
            }
        }
        public void OpenJobFolder()
        {
            if (Directory.Exists(JobFolderPath))
            {
                Process openJobFolder = new Process();
                openJobFolder.StartInfo = new ProcessStartInfo(JobFolderPath)
                {
                    UseShellExecute = true
                };
                openJobFolder.Start();
            }
        }
        public void JobNumberAddChar(string key)
        {
            JobNumber += key;
            OnPropertyChanged(nameof(JobNumber));
        }
        public void JobNumberDelChar()
        {
            JobNumber = JobNumber.Substring(0, JobNumber.Length - 1);
            OnPropertyChanged(nameof(JobNumber));
        }
        public void BrowseSourceDir()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SourceDirPath = dialog.FileName;
                Properties.Settings1.Default.SourceDirPath = dialog.FileName;
                OnPropertyChanged(nameof(SourceDirPath));
            }
            
        }
        public void BrowseJobFolderParentDir()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                JobFolderParentDirPath = dialog.FileName;
                Properties.Settings1.Default.JobFolderParentDirPath = dialog.FileName;
                OnPropertyChanged(nameof(JobFolderParentDirPath));
            }

        }
        public void Exit()
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Commands
        public ICommand OpenSelectedPhotoCommand { get; set; }
        public ICommand OpenJobFolderCommand { get; set; }
        public ICommand BrowseSourceDirCommand { get; set; }
        public ICommand BrowseJobFolderParentDirCommand { get; set; }
        public ICommand JobNumberAddCharCommand { get; set; }
        public ICommand JobNumberDelCharCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
