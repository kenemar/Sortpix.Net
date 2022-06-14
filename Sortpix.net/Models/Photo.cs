using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SortPix.ViewModels;
using SortPix.Commands;

namespace SortPix.Models
{
    public class Photo
    {
        public string Path { get; set; }
        public ICommand OpenSelectedPhotoCommand { get; set; }

    }
}
