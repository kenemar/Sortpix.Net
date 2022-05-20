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
        public SortPixMainWindowsVM()   //ViewModelConstructor
        {
            //Load user preferences from Saved Settings
            SourceDirPath = Properties.Settings1.Default.SourceDirPath;
            JobFolderParentDirPath = Properties.Settings1.Default.JobFolderParentDirPath;
            JobNumberPrefix = Properties.Settings1.Default.JobNumberPrefix;
            JobNumber = Properties.Settings1.Default.JobNumber;
            JobNumberSuffix = Properties.Settings1.Default.JobNumberSuffix;
            MoveToSubfolder = Properties.Settings1.Default.MoveToSubfolder;
            StartAtRight = Properties.Settings1.Default.StartAtRight;

            //Initialize ICommands
            OpenSelectedPhotoCommand = new OpenSelectedPhotoCommand(this);
            OpenJobFolderCommand = new OpenJobFolderCommand(this);
            BrowseSourceDirCommand = new BrowseSourceDirCommand(this);
            BrowseJobFolderParentDirCommand = new BrowseJobFolderParentDirCommand(this);
            JobNumberAddCharCommand = new JobNumberAddCharCommand(this);
            JobNumberDelCharCommand = new JobNumberDelCharCommand(this);
            ExitCommand = new ExitCommand(this);

            //Start file watchers on Source and Destination folders
            MonitorSourceDir();
            MonitorDestDir();

        }
        #region Private Fields
        //private backing fields for public properties
        private ObservableCollection<string> _photoPaths;
        private string _sourceDirPath;
        private string _jobNumber;
        private bool _pathsValid;
        //Potential names for Pictures subfolder in job folder
        private readonly string[] JobPhotoLocations = { "\\pictures", "\\photos", "\\pics" };
        #endregion

        #region Public Properties
        public string JobNumber         //Job Number property
        {
            get => _jobNumber;
            set
            {
                _jobNumber = value;
                OnPropertyChanged(nameof(PathsValid));
                OnPropertyChanged(nameof(PathsValidColor));
                //if Paths and Job Number is valid, start a thread to retreive recent photos from job folder
                if (Directory.Exists(JobFolderPath))
                {
                    Task.Run(() => GetPhotoPathsAsync(JobFolderPath));

                    //Start monitoring in case it's not running
                    MonitorDestDir();                   
                }
                //If Job Number not valid, return empty ObservableCollection
                else PhotoPaths = new ObservableCollection<string>();           
            }
        }
        public string PathsValid        //Message to show whether Paths and Job Number are valid (ready to copy)
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
        public Brush PathsValidColor    //Brush property to set color of PathsValid Message  
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
        public string SourceDirPath     //Path to source directory (Camera Roll)
        {
            get => _sourceDirPath;
            set
            {
                _sourceDirPath = value;
                //If FileWatcher is not null, set the Path field to the selected Path; otherwise start monitoring
                if(sourceDirWatcher != null)
                {
                    sourceDirWatcher.Path = value;
                } else
                {
                    MonitorSourceDir();
                }
            }
        }
        public string JobFolderParentDirPath { get; set; }  //Path to folder that contains the Job folders
        public string JobNumberPrefix { get; set; }         //Anything coming before the numerical part of the job number in the file path
        public string JobNumberSuffix { get; set; }         //Anything coming after the numerical part of the job number in the file path
        public string JobFolderPath         //Build the Full Job Folder Path by combining Parent directory path, preffix and suffix
        {
            get
            {
                return JobFolderParentDirPath + "\\" + JobNumberPrefix + JobNumber + JobNumberSuffix;
            }

        }
        public string PhotoFolderPath       //Location to actually copy the pictures to (Job folder or Pictures subfolder)
        {
            get
            {
                if (!MoveToSubfolder) { return JobFolderPath; }     //If MovetoSubfolder is not selected, just return JobFolderPath
                else
                {
                    //Otherwise loop through possible names for Pictures subfolder
                    foreach(string location in JobPhotoLocations)
                    {
                        //if there are any matches, return Job folder + subfolder name
                        if (Directory.Exists(JobFolderPath + location))
                        {
                            return JobFolderPath + location;
                        }
                    }
                    //If no matches, created a subfolder called Pictures and return JobFolder path + Pictures
                    _ = Directory.CreateDirectory(JobFolderPath + "\\Pictures");
                    return JobFolderPath + "\\Pictures";
                }
            }
        }
        public ObservableCollection<string> PhotoPaths  //List of paths to most recent pictures in current Job folder, to display along bottom of app
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
        public string SelectedPhoto { get; set; }   //Which photo is currently selected
        public bool MoveToSubfolder { get; set; }   //Option to move pix to a subfolder in the job folder (created one if it doesn't exist)
        public bool StartAtRight { get; set; }      //Option to startup app at the right of the screen, makes it easier to use with touch screen
        #endregion

        #region Methods
        private void GetPhotoPathsAsync(string path)    //set the PhotoPaths property to an ObservableCollection containing paths to the 4 most recent jpg or jpeg files in job folder, sorted from newest to oldest
        {
            
            try
            {
                //first run the query and store results in a list, then create new ObservableCollection and set PhotoPaths to it
                //This is important because if you do it all in one step, it freezes the UI while the query runs, even if it's running in a different thread
                var photoPathsList = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".jpeg")).OrderByDescending(d => new FileInfo(d).CreationTime).Take(4);
                PhotoPaths = new ObservableCollection<string>(photoPathsList);
            }
            catch   //if there are any errors here, just return an empty ObservableCollection
            {
                PhotoPaths = new ObservableCollection<string>();
            }
            
        }
        //Declare file system watchers for source and destination directories
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
            Task.Run(() => MovePhoto(e.FullPath, PhotoFolderPath + "\\" + e.Name));
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Task.Run(() => GetPhotoPathsAsync(JobFolderPath));
        }
        private static bool IsFileLocked(string filePath)
        {
            try
            {
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
