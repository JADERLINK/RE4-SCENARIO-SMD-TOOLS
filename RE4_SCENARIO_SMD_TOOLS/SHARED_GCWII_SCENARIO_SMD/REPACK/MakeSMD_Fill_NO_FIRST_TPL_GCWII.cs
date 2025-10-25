using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_GCWII_SCENARIO_SMD.REPACK
{
    public class MakeSMD_Fill_NO_FIRST_TPL_GCWII : MakeSMD_Fill_TPL
    {
        private string TplfolderPath;
        private string TplFileName;

        public MakeSMD_Fill_NO_FIRST_TPL_GCWII(string tplfolderPath, string tplFileName)
        {
            TplfolderPath = tplfolderPath;
            TplFileName = tplFileName;
        }

        protected override void PutTpl(EndianBinaryWriter bw, int tplId, long startTplOffset, out long endTplOffset)
        {
            if (tplId == 0)
            {
                PutEmptyTpl.PutTpl(bw.BaseStream, startTplOffset, out endTplOffset, true, false, Endianness.BigEndian);
            }
            else
            {
                string tempTplFilePath = Path.Combine(TplfolderPath, TplFileName);
                tempTplFilePath = Path.ChangeExtension(tempTplFilePath, $"{tplId}.tpl");
                if (File.Exists(tempTplFilePath))
                {
                    try
                    {
                        MemoryStream ms = new MemoryStream();
                        ms.Position = 0;

                        FileInfo info = new FileInfo(tempTplFilePath);
                        var read = info.OpenRead();
                        read.CopyTo(ms);
                        read.Close();

                        // alinhamento do bin
                        int _padding = (int)((TplAlignment() - (ms.Position % TplAlignment())) % TplAlignment());
                        ms.Write(new byte[_padding], 0, _padding);
                        long tplLength = ms.Position;

                        //verifica o magic
                        ms.Position = 0;
                        EndianBinaryReader br = new EndianBinaryReader(ms, Endianness.BigEndian);
                        uint __magic = br.ReadUInt32();
                        uint __tplAmount = br.ReadUInt32();

                        if (__magic != 0x0020AF30 || __tplAmount > 0x00_01_00_00)
                        {
                            throw new ApplicationException("The TPL file is from a different version of the SMD that is being repacked.");
                        }

                        // copia
                        bw.Position = startTplOffset;
                        ms.Position = 0;
                        ms.CopyTo(bw.BaseStream);
                        ms.Close();
                        endTplOffset = bw.Position;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in {Path.GetFileName(tempTplFilePath)}: " + Environment.NewLine + ex.Message);
                        PutEmptyTpl.PutTpl(bw.BaseStream, startTplOffset, out endTplOffset, true, false, Endianness.BigEndian);
                    }
                }
                else
                {
                    Console.WriteLine($"Error in {Path.GetFileName(tempTplFilePath)}: " + Environment.NewLine + "File not Exist!");
                    PutEmptyTpl.PutTpl(bw.BaseStream, startTplOffset, out endTplOffset, true, false, Endianness.BigEndian);
                }
            }

        }

        protected override int TplAlignment()
        {
            return 32;
        }
    }
}
