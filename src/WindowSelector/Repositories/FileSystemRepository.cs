using System.IO;
using WindowSelector.Properties;

namespace WindowSelector.Repositories
{
    public class FileSystemRepository
    {
        public StreamWriter CreateText([NotNull]string path)
        {
            return File.CreateText(path);
        }

        public StreamReader OpenText([NotNull]string path)
        {
            return File.OpenText(path);
        }

        public void Delete()
        {
            //return File.Delete()
        }
    }
}
