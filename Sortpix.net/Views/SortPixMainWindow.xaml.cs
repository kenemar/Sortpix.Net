using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SortPix.ViewModels;

namespace SortPix.Views
{
    /// <summary>
    /// Interaction logic for SortPixMainWindow.xaml
    /// </summary>
    public partial class SortPixMainWindow : Window
    {
        internal SortPixMainWindowsVM viewModel { get; set; }
        public SortPixMainWindow()
        {
            viewModel = new SortPixMainWindowsVM();
            DataContext = viewModel;
            Closing += viewModel.OnWindowClosing;
            InitializeComponent();
            JobNumber.CaretIndex = JobNumber.Text.Length;
            JobNumber.Focus();

            if (viewModel.StartAtRight)
            {
                Left = SystemParameters.WorkArea.Width - (Width + 10);
                Top = (SystemParameters.WorkArea.Height / 2) - (Height / 2);
            }
        }

        private void JobNumber_GotFocus(object sender, RoutedEventArgs e)
        {
            JobNumber.CaretIndex = JobNumber.Text.Length;
        }

        private void MainTabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            viewModel.MonitorDestDir();
        }
    }
}
