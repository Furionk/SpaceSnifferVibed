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

namespace SpaceSnifferX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            var rootFolder = new FolderData
            {
                Name = "Root",
                Size = 1000,
                SubFolders = new List<FolderData>
        {
            new FolderData { Name = "Folder1", Size = 400 },
            new FolderData { Name = "Folder2", Size = 600 }
        }
            };
            var viewModel = (MainWindowViewModel)DataContext;
            HeatMapGenerator.GenerateHeatMap(HeatMapCanvas, rootFolder, 0, 0, HeatMapCanvas.Width, HeatMapCanvas.Height);
            viewModel.OnHeatMapDataUpdated += rootFolder =>
            {
                HeatMapCanvas.Children.Clear();
                HeatMapGenerator.GenerateHeatMap(HeatMapCanvas, rootFolder, 0, 0, HeatMapCanvas.Width, HeatMapCanvas.Height);
            };

        }

        private void HeatMapCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Clear the canvas
            HeatMapCanvas.Children.Clear();

            // Get the root folder data (you may need to store this in your ViewModel or pass it here)
            var rootFolder = ((MainWindowViewModel)DataContext).RootFolderData;

            if (rootFolder != null)
            {
                // Redraw the heat map with the updated canvas size
                HeatMapGenerator.GenerateHeatMap(HeatMapCanvas, rootFolder, 0, 0, HeatMapCanvas.ActualWidth, HeatMapCanvas.ActualHeight);
            }
        }
    }
}