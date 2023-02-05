namespace XAMLEditor
{
    public interface IFileService
    {
        string FilePath { get; set; }
        bool OpenFileDialog();
        bool SaveFileDialog();
    }
}