using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PDFUtil.ViewModels;
using Microsoft.Win32;
using System.IO;
using WPFFolderBrowser;

namespace PDFUtil
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = new MainViewModel();
            this.DataContext = _vm;
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.Yes == MessageBox.Show("Do you want to clear all entries?", "Clear entries", MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                var temp = _vm;
                _vm = new MainViewModel();
                this.DataContext = _vm;
                temp.Dispose();
            }
        }

        private void runButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(_vm.OutputFolder))
            {
                var x = Directory.GetFileSystemEntries(_vm.OutputFolder);
                if (x.Count() > 0)
                {
                    MessageBox.Show("Destination Folder is NOT empty.\nPlease choose an empty or new folder", "Folder not empty", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
            }
            try
            {
                _vm.RunSplitMerge();
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void coverPathButton_Click(object sender, RoutedEventArgs e)
        {
            string initial = Properties.Settings.Default.LastCover;
            if (string.IsNullOrWhiteSpace(initial))
            {
                initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                if (!Directory.Exists(initial))
                {
                    initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            }
            OpenFileDialog dlg = new OpenFileDialog()
            {
                DefaultExt = ".pdf",
                DereferenceLinks = true,
                InitialDirectory = initial,
                Filter = "PDF Files|*.pdf",
                FilterIndex = 0,
                Multiselect = false,
                Title = "Select a file for cover letter"
            };
            while ((bool)dlg.ShowDialog(this))
            {
                if (File.Exists(dlg.FileName))
                {
                    _vm.CoverPath = dlg.FileName;
                    break;
                }
            }            
        }

        private void outputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string initial = Properties.Settings.Default.LastFolder;
            if (string.IsNullOrWhiteSpace(initial))
            {
                initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                if (!Directory.Exists(initial))
                {
                    initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            }
            var dlg = new WPFFolderBrowserDialog
            {
                DereferenceLinks = true,
                InitialDirectory = initial,
                Title = "Select the output folder",
                ShowPlacesList = true
            };
            if ((bool)dlg.ShowDialog(this))
            {
                _vm.OutputFolder = dlg.FileName;
                Properties.Settings.Default.LastFolder = _vm.OutputFolder;
            }
            //dlg.Dispose();
        }

        private void inputFileButton_Click(object sender, RoutedEventArgs e)
        {
            string initial = Properties.Settings.Default.LastFile;

            if (string.IsNullOrWhiteSpace(initial))
            {
                initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                initial = System.IO.Path.GetDirectoryName(initial);
                if (!Directory.Exists(initial))
                {
                    initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            }

            OpenFileDialog dlg = new OpenFileDialog()
            {
                DefaultExt = ".pdf",
                DereferenceLinks = true,
                InitialDirectory = initial,
                Filter = "PDF Files|*.pdf",
                FilterIndex = 0,
                Multiselect = false,
                Title = "Select a file to split"
            };
            while ((bool)dlg.ShowDialog(this))
            {
                if (File.Exists(dlg.FileName))
                {
                    _vm.InputFile = dlg.FileName;
                    _vm.InputFolder = null;
                    _vm.Check();
                    //splitButton.Content = inputFileButton.Content;
                    Properties.Settings.Default.LastFile = _vm.InputFile;
                    inputFile.Background = Brushes.LavenderBlush;
                    break;
                }
            }
        }

        private void inputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string initial = Properties.Settings.Default.LastFile;

            if (string.IsNullOrWhiteSpace(initial))
            {
                initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                initial = System.IO.Path.GetDirectoryName(initial);
                if (!Directory.Exists(initial))
                {
                    initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            }
            var dlg = new WPFFolderBrowserDialog
            {
                DereferenceLinks = true,
                InitialDirectory = initial,
                Title = "Select the Input folder",
                ShowPlacesList = true
            };
            if ((bool)dlg.ShowDialog(this))
            {                
                _vm.InputFile = dlg.FileName;
                _vm.InputFolder = dlg.FileName;
                //splitButton.Content = inputFolderButton.Content;    
                Properties.Settings.Default.LastFile = _vm.InputFile;
                inputFile.Background = Brushes.LightYellow;
            }
            //dlg.Dispose();
        }


        private void FooterPathButton_OnClick(object sender, RoutedEventArgs e)
        {
            string initial = Properties.Settings.Default.LastCover;
            if (string.IsNullOrWhiteSpace(initial))
            {
                initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            else
            {
                if (!Directory.Exists(initial))
                {
                    initial = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
            }
            OpenFileDialog dlg = new OpenFileDialog()
                {
                    DefaultExt = ".pdf",
                    DereferenceLinks = true,
                    InitialDirectory = initial,
                    Filter = "PDF Files|*.pdf",
                    FilterIndex = 0,
                    Multiselect = false,
                    Title = "Select a file for Footer"
                };
            while ((bool) dlg.ShowDialog(this))
            {
                if (File.Exists(dlg.FileName))
                {
                    _vm.FooterPath = dlg.FileName;
                    break;
                }
            }
        }
    }
}
