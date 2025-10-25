using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT
{
    public class ToFileMethod_TPL
    {
        private string DirectoryToSaveTPL = "";
        private string TPLBaseName0 = "TPL";
        private string TPLBaseNameOthers = "TPL";
        private bool EnableExtract = false;

        public ToFileMethod_TPL(bool EnableExtract, string DirectoryToSaveTPL, string TPLBaseName0 = "TPL", string TPLBaseNameOthers = "TPL", bool UseAltBaseName = false)
        {
            this.DirectoryToSaveTPL = DirectoryToSaveTPL;
            this.TPLBaseName0 = TPLBaseName0;
            this.TPLBaseNameOthers = TPLBaseName0;
            if (UseAltBaseName)
            {
                this.TPLBaseNameOthers = TPLBaseNameOthers;
            }
            this.EnableExtract = EnableExtract;
        }

        public void ToFileTpl(Stream fileStream, long tplOffset, long endOffset, int tplID)
        {
            if (EnableExtract && tplOffset > 0)
            {
                string FileName = TPLBaseName0 + ".TPL";
                if (tplID > 0)
                {
                    FileName = TPLBaseNameOthers + "." + tplID + ".TPL";
                }
                try
                {
                    //le os bytes do tpl e grava em um arquivo
                    fileStream.Position = tplOffset;
                    long tplLenght = endOffset - tplOffset;

                    byte[] tplArray = new byte[tplLenght];
                    fileStream.Read(tplArray, 0, (int)tplLenght);

                    string tplPath = Path.Combine(DirectoryToSaveTPL, FileName);

                    Directory.CreateDirectory(DirectoryToSaveTPL);
                    File.WriteAllBytes(tplPath, tplArray);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on write in file: " + FileName + Environment.NewLine + ex.ToString());
                }
            }
        }

    }

}
