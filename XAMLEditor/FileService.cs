using Microsoft.Win32;

namespace XAMLEditor
{
    public class FileService : IFileService
    {
        public string FilePath { get; set; }
        public bool OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                return true;
            }

            return false;
        }

        public bool TryGetOpenFileName(out string name)
        {
            if (OpenFileDialog())
            {
                name = FilePath;
                return true;
            }

            name = null;
            return false;
        }

        public bool SaveFileDialog()
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            if (fileDialog.ShowDialog() == true)
            {
                FilePath = fileDialog.FileName;
                return true;
            }

            return false;
        }

        public bool TryGetSaveFileName(out string name)
        {
            if (SaveFileDialog())
            {
                name = FilePath;
                return true;
            }

            name = null;
            return false;
        }
    }
}