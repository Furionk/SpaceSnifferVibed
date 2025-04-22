using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpaceSnifferX
{
    public class MainWindowViewModel : BaseViewModel
    {
        private string _selectedFolder;
        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;
                    OnPropertyChanged();
                }
            }
        }

        public FolderData RootFolderData { get; set; }

        public ObservableCollection<FolderData> SubfolderSizes { get; } = new();

        private IEnumerable<FolderData> FlattenFolderHierarchy(FolderData folder)
        {
            // Add the current folder
            yield return folder;

            // Recursively add all subfolders
            foreach (var subFolder in folder.SubFolders)
            {
                foreach (var child in FlattenFolderHierarchy(subFolder))
                {
                    yield return child;
                }
            }
        }


        public ICommand SelectFolderCommand { get; }
        public ICommand ScanFolderCommand { get; }

        public MainWindowViewModel()
        {
            SelectFolderCommand = new RelayCommand(SelectFolder);
            ScanFolderCommand = new RelayCommand(ScanFolder, CanScanFolder);
        }

        private void SelectFolder()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedFolder = dialog.SelectedPath;
                SubfolderSizes.Clear();
            }
        }


        private void ScanFolder()
        {
            if (string.IsNullOrEmpty(SelectedFolder)) return;

            // Scan the root folder and its subfolders
            RootFolderData = new FolderData
            {
                Name = SelectedFolder,
                Size = FolderSizeCalculator.GetFolderSize(SelectedFolder),
                SubFolders = GetSubFolders(SelectedFolder)
            };

            // Clear and populate the SubfolderSizes collection with all folders
            SubfolderSizes.Clear();
            foreach (var folder in FlattenFolderHierarchy(RootFolderData))
            {
                SubfolderSizes.Add(folder);
            }

            // Notify the view to update the heat map
            OnHeatMapDataUpdated?.Invoke(RootFolderData);
        }

        private List<FolderData> GetSubFolders(string folderPath)
        {
            var subFolders = new List<FolderData>();
            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                var subFolder = new FolderData
                {
                    Name = dir,
                    Size = FolderSizeCalculator.GetFolderSize(dir),
                    SubFolders = GetSubFolders(dir) // Recursive call to get subfolders
                };
                subFolders.Add(subFolder);
            }
            return subFolders;
        }

        public event Action<FolderData> OnHeatMapDataUpdated;

        public bool CanScanFolder()
        {
            return true;
        }

        private long GetFolderSize(string folderPath)
        {
            try
            {
                return Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                                .Sum(file => new FileInfo(file).Length);
            }
            catch
            {
                return 0; // Handle inaccessible folders gracefully
            }
        }
    }

}