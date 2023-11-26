using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursova
{
    public interface IFileHandler
    {
        void WriteToFile(string filePath, string data);
        void AppendToFile(string filePath, string data);
        string ReadFromFile(string filePath);
        string LoadFromFile(string filePath);
    }
}