using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace SortPix.ViewModels
//{
//    class ViewModel : INotifyPropertyChanged
//    {
//        private string _path;
//        public string Path
//        {
//            get => _path;
//            set
//            {
//                _path = value;
//                if (Directory.Exists(Path))
//                {
//                    OnPropertyChanged(nameof(PhotoPaths));
//                }

//            }
//        }

//        public ObservableCollection<string> PhotoPaths
//        {
//            get => GetPhotoPaths(Path);

//        }

//        private ObservableCollection<string> GetPhotoPaths(string path)
//        {
//            return new ObservableCollection<string>(Directory.GetFiles(path, "*.jpg"));
//        }
        
//        private void OnPropertyChanged(string v)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
