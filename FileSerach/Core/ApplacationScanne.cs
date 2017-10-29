using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSerach.Model;
using Microsoft.Win32;

namespace FileSerach.Core
{
    public class ApplacationScanne
    {
        public static IEnumerable<string> ShowAllSoftwaresName()
        {
            var startMenu = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonPrograms);
            var dires = Directory.EnumerateDirectories(startMenu);
            foreach (var dir in dires)
            {
                foreach (var file in Directory.EnumerateFiles(dir))
                {
                    yield return file;
                }
            }
            var files = Directory.EnumerateFiles(startMenu);
            foreach (var file in files)
            {
                yield return file;
            }
        }
    }
}
