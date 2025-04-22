using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

public class HeatMapGenerator
{
    public static void GenerateHeatMap(Canvas canvas, FolderData folderData, double x, double y, double width, double height)
    {
        if (folderData == null || folderData.Size == 0) return;

        // Draw the rectangle for the current folder
        var rect = new System.Windows.Shapes.Rectangle
        {
            Width = width,
            Height = height,
            Fill = new SolidColorBrush(Colors.LightBlue),
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 1,
            Tag = folderData.Name // Store the folder path in the Tag property
        };
        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        canvas.Children.Add(rect);

        // Attach mouse events for hover effect
        rect.MouseEnter += Rectangle_MouseEnter;
        rect.MouseLeave += Rectangle_MouseLeave;
        rect.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;

        // Extract only the folder name
        string folderName = System.IO.Path.GetFileName(folderData.Name);

        // Add a label for the folder name
        var pathTextBlock = new TextBlock
        {
            Text = folderName, // Use only the folder name
            FontSize = 12,
            Foreground = new SolidColorBrush(Colors.Black),
            TextWrapping = TextWrapping.Wrap,
            Width = width,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Canvas.SetLeft(pathTextBlock, x);
        Canvas.SetTop(pathTextBlock, y);
        canvas.Children.Add(pathTextBlock);

        // Add a label for the folder size
        var sizeTextBlock = new TextBlock
        {
            Text = FormatSize(folderData.Size),
            FontSize = 10,
            Foreground = new SolidColorBrush(Colors.Gray),
            Width = width,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Canvas.SetLeft(sizeTextBlock, x);
        Canvas.SetTop(sizeTextBlock, y + 15);
        canvas.Children.Add(sizeTextBlock);

        // Calculate subfolder sizes
        double totalSize = folderData.SubFolders.Sum(f => f.Size);
        if (totalSize == 0) return;

        double currentX = x, currentY = y;
        double remainingWidth = width, remainingHeight = height;

        foreach (var subFolder in folderData.SubFolders)
        {
            double area = (subFolder.Size / totalSize) * (width * height);
            bool splitHorizontally = remainingWidth >= remainingHeight;

            double subWidth = splitHorizontally ? area / remainingHeight : remainingWidth;
            double subHeight = splitHorizontally ? remainingHeight : area / remainingWidth;

            if (subWidth > remainingWidth) subWidth = remainingWidth;
            if (subHeight > remainingHeight) subHeight = remainingHeight;

            GenerateHeatMap(canvas, subFolder, currentX, currentY, subWidth, subHeight);

            if (splitHorizontally)
            {
                currentX += subWidth;
                remainingWidth -= subWidth;
            }
            else
            {
                currentY += subHeight;
                remainingHeight -= subHeight;
            }
        }
    }


    private static void Rectangle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        var rect = sender as System.Windows.Shapes.Rectangle;
        if (rect != null)
        {
            // Change the rectangle's appearance on hover
            rect.Fill = new SolidColorBrush(Colors.LightGreen);
            rect.Stroke = new SolidColorBrush(Colors.DarkGreen);
        }
    }

    private static void Rectangle_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        var rect = sender as System.Windows.Shapes.Rectangle;
        if (rect != null)
        {
            // Revert the rectangle's appearance when the mouse leaves
            rect.Fill = new SolidColorBrush(Colors.LightBlue);
            rect.Stroke = new SolidColorBrush(Colors.Black);
        }
    }

    private static void Rectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var rect = sender as System.Windows.Shapes.Rectangle;
        if (rect?.Tag is string folderPath && Directory.Exists(folderPath))
        {
            // Open the folder in File Explorer
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
    }

    private static string FormatSize(long size)
    {
        if (size >= 1_073_741_824) // GB
            return $"{size / 1_073_741_824.0:F2} GB";
        if (size >= 1_048_576) // MB
            return $"{size / 1_048_576.0:F2} MB";
        if (size >= 1024) // KB
            return $"{size / 1024.0:F2} KB";
        return $"{size} Bytes";
    }
}