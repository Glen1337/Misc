using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Misc
{
    namespace Projects
    {
        class _Main
        {
            static void Main(string[] args)
            {
                // Looks for times.txtx file on Desktop and writes current time (run from Startup folder)
                using (StreamWriter writer = new StreamWriter(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"\\times.txt"),
                                                              true,
                                                              Encoding.Unicode))
                {
                    writer.WriteLine(System.DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"));
                }
            }
        }
    }
}
