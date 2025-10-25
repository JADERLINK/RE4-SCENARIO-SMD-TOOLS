using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT
{
    public class ToFileMethod_BIN
    {
        private string DirectoryToSave = "";
        private bool EnableExtract = false;

        public ToFileMethod_BIN(string DirectoryToSave, bool EnableExtract)
        {
            this.DirectoryToSave = DirectoryToSave;
            this.EnableExtract = EnableExtract;
        }

        public void ToFileBin(Stream fileStream, long binOffset, long endOffset, int binID)
        {
            if (EnableExtract && binOffset > 0)
            {
                string FileName = binID.ToString("D4") + ".BIN";
                try
                {
                    //le os bytes do bin e grava em um arquivo
                    fileStream.Position = binOffset;
                    long lenght = endOffset - binOffset;

                    byte[] binArray = new byte[lenght];
                    fileStream.Read(binArray, 0, (int)lenght);

                    string binPath = Path.Combine(DirectoryToSave, FileName);

                    Directory.CreateDirectory(DirectoryToSave);
                    File.WriteAllBytes(binPath, binArray);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on write in file: " + FileName + Environment.NewLine + ex.ToString());
                }
            }
        }
    }

}
