using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP.DoxygenEditor.Extensions
{
    static class DirectoryInfoExtensions
    {
        public static DirectoryInfo GetDirectory(this DirectoryInfo root, string name)
        {
            DirectoryInfo[] dirs = root.GetDirectories(name, SearchOption.TopDirectoryOnly);
            if (dirs.Length == 1)
                return (dirs[0]);
            return (null);
        }
    }
}
