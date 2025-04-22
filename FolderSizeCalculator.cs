using System.IO;

public class FolderSizeCalculator
{
    public static long GetFolderSize(string folderPath)
    {
        try
        {
            return Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                            .Sum(file => new FileInfo(file).Length);
        }
        catch
        {
            return 0; // Handle access exceptions
        }
    }
}
