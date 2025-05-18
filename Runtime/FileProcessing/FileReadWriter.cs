using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace SaveLoadSystem.FileProcessing
{
    public class FileReadWriter
    {
        public void WriteFile(string filePath, byte[] data)
        {
            if (FileAlreadyExists(filePath))
                File.Delete(filePath);
            
            var file = File.Create(filePath);
            BinaryFormatter formatter = new();
            formatter.Serialize(file, data);
            file.Close();
        }
        
        public byte[] ReadFile(string filePath)
        {
            if (!FileAlreadyExists(filePath))
                return null;
            var file = File.Open(filePath, FileMode.Open);
            BinaryFormatter formatter = new();
            var dataBytes = formatter.Deserialize(file);
            file.Close();
            return (byte[])dataBytes;
        }

        public List<byte[]> ReadAllFilesOfExtension(string directoryPath, string extension)
        {
            if (!Directory.Exists(directoryPath))
                return new List<byte[]>();
            var filesNames = Directory.GetFiles(directoryPath, $"*{extension}");
            return filesNames.Select(ReadFile).ToList();
        }

        public void DeleteFile(string path)
        {
            if (!FileAlreadyExists(path)) return;
            File.Delete(path);
        }

        public bool FileAlreadyExists(string fullPath) => File.Exists(fullPath);

        public void CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }
    }
}