using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace Cursova.FileHandling
{
    public class FileHandler : IFileHandler
    {
        public void WriteToFile(string filePath, string content)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to file: {ex.Message}");
            }
        }

        public void AppendToFile(string filePath, string data)
        {
            File.AppendAllText(filePath, $"{Environment.NewLine}{data}");
        }

        public string ReadFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public string LoadFromFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}