using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SHARED_SCENARIO_SMD
{
    public class StartMain
    {
        //Fileinfo arquivo, string formato, bool se_o_formato_é_usado_pelo_metodo
        public List<Func<FileInfo, string, bool>> FileFormatList { get; private set; }

        public StartMain()
        {
            FileFormatList = new List<Func<FileInfo, string, bool>>();
        }

        public void Continue(string[] args) 
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Console.WriteLine(SHARED_TOOLS.Shared.HeaderText());

            bool usingBatFile = false;
            int start = 0;
            if (args.Length > 0 && args[0].ToLowerInvariant() == "-bat")
            {
                usingBatFile = true;
                start = 1;
            }

            for (int i = start; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    try
                    {
                        FileInfo fileInfo1 = new FileInfo(args[i]);
                        string file1Extension = fileInfo1.Extension.ToUpperInvariant();
                        Console.WriteLine("File: " + fileInfo1.Name);
                        ProcessFiles(fileInfo1, file1Extension);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + Environment.NewLine + ex);
                    }
                }
                else
                {
                    Console.WriteLine("File specified does not exist: " + args[i]);
                }

            }

            if (args.Length == 0)
            {
                Console.WriteLine("How to use: drag the file to the executable.");
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4-SCENARIO-SMD-TOOLS");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Finished!!!");
                if (!usingBatFile)
                {
                    Console.WriteLine("Press any key to close the console.");
                    Console.ReadKey();
                }
            }
        }

        private void ProcessFiles(FileInfo fileInfo1, string file1Extension)
        {
            bool validated = false;
            foreach (var item in FileFormatList)
            {
                if (item?.Invoke(fileInfo1, file1Extension) ?? false)
                {
                    validated = true;
                    break;
                }
            }

            if (validated == false)
            {
                Console.WriteLine("Invalid file format: " + fileInfo1.Name);
            }
        }
    }
}
