public class FolderData
{
    public string Name { get; set; }
    public long Size { get; set; }
    public List<FolderData> SubFolders { get; set; } = new();
}
