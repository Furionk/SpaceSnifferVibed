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

            RootFolderData = new FolderData
            {
                Name = SelectedFolder,
                Size = FolderSizeCalculator.GetFolderSize(SelectedFolder),
                SubFolders = GetSubFolders(SelectedFolder)
            };

            SubfolderSizes.Clear();
            foreach (var subFolder in RootFolderData.SubFolders)
            {
                SubfolderSizes.Add(subFolder);
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