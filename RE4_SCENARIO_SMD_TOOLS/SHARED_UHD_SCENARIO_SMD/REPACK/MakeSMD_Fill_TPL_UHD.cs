using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ;
using SHARED_UHD_BIN_TPL.EXTRACT;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_UHD_SCENARIO_SMD.REPACK
{
    public class MakeSMD_Fill_TPL_UHD : MakeSMD_Fill_TPL
    {
        private UhdTPL uhdTPL;

        private bool CreateTplFilesInFolder;
        private string TplfolderPath;

        private bool isPS4NS;
        private Endianness endianness;

        public MakeSMD_Fill_TPL_UHD(UhdTPL uhdTPL, bool createTplFilesInFolder, string tplfolderPath, bool isPS4NS, Endianness endianness)
        {
            this.uhdTPL = uhdTPL;
            CreateTplFilesInFolder = createTplFilesInFolder;
            TplfolderPath = tplfolderPath;
            this.isPS4NS = isPS4NS;
            this.endianness = endianness;
        }

        protected override void PutTpl(EndianBinaryWriter bw, int tplId, long startTplOffset, out long endTplOffset)
        {
            if (tplId == 0)
            {
                //tpl file
                SHARED_UHD_BIN_TPL.REPACK.TPLmakeFile.MakeFile(uhdTPL, bw.BaseStream, startTplOffset, out endTplOffset, isPS4NS, endianness);
            }
            else 
            {
                PutEmptyTpl.PutTpl(bw.BaseStream, startTplOffset, out endTplOffset, false, isPS4NS, endianness);
            }
           
            if (CreateTplFilesInFolder)
            {
                string filePath = Path.Combine(TplfolderPath, "TPL.TPL");
                if (tplId > 0)
                {
                    filePath = Path.Combine(TplfolderPath, $"TPL.{tplId}.TPL");
                }

                try
                {
                    Directory.CreateDirectory(TplfolderPath);

                    //salva o tpl em arquivo
                    Stream stream = bw.BaseStream;
                    stream.Position = startTplOffset;
                    int tplLenght = (int)(endTplOffset - startTplOffset);
                    byte[] tpl = new byte[tplLenght];
                    stream.Read(tpl, 0, tplLenght);

                    File.WriteAllBytes(filePath, tpl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on write in file: " + Path.GetFileName(filePath) + Environment.NewLine + ex.ToString());
                }
            }
        }

        protected override int TplAlignment()
        {
            return 16;
        }
    }
}
