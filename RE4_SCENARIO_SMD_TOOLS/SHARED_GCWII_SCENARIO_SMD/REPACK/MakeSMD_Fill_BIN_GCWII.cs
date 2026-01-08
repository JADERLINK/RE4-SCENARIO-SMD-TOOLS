using SHARED_GCWII_BIN.REPACK.Structures;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ;
using SHARED_TOOLS.ALL;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_GCWII_SCENARIO_SMD.REPACK
{
    public class MakeSMD_Fill_BIN_GCWII : MakeSMD_Fill_BIN
    {
        // binID, FinalStructure
        private Dictionary<int, FinalStructure> FinalBinDic;
        private Dictionary<int, byte> Vertex_scale_Dic;
        private IdxMaterial material;

        private bool EnableVertexColors;
        private bool EnableDinamicVertexColor;

        private bool CreateBinFilesInFolder;
        private string BinfolderPath;

        public MakeSMD_Fill_BIN_GCWII(Dictionary<int, FinalStructure> finalBinDic, Dictionary<int, byte> vertex_scale_Dic, IdxMaterial material, bool enableVertexColors, bool enableDinamicVertexColor, bool createBinFilesInFolder, string binfolderPath)
        {
            FinalBinDic = finalBinDic;
            Vertex_scale_Dic = vertex_scale_Dic;
            this.material = material;
            EnableVertexColors = enableVertexColors;
            EnableDinamicVertexColor = enableDinamicVertexColor;
            CreateBinFilesInFolder = createBinFilesInFolder;
            BinfolderPath = binfolderPath;
        }

        protected override void PutBin(EndianBinaryWriter bw, int binId, long startBinOffset, out long endBinOffset)
        {
            long outOffset = startBinOffset;

            if (FinalBinDic.ContainsKey(binId))
            {
                bool EnableColor = EnableVertexColors || CheckDinamicVertexColor.Check(FinalBinDic[binId], EnableDinamicVertexColor);

                //boneLine
                SHARED_TOOLS.REPACK.FinalBoneLine[] boneLineArray = new SHARED_TOOLS.REPACK.FinalBoneLine[1];
                boneLineArray[0] = new SHARED_TOOLS.REPACK.FinalBoneLine(0, 0xFF, 0, 0, 0, Endianness.BigEndian);

                Console.WriteLine("BIN ID: " + binId.ToString("D3"));
                SHARED_GCWII_BIN.REPACK.BINmakeFile.MakeFile(bw.BaseStream, startBinOffset, out outOffset, FinalBinDic[binId],
                    boneLineArray, material, new (ushort b1, ushort b2, ushort b3, ushort b4)[0], true, false, false, false, EnableColor, false, Vertex_scale_Dic[binId]);
            }
            else
            {
                PutEmptyBin.PutBin(bw.BaseStream, startBinOffset, out outOffset, true, false, Endianness.BigEndian);
            }

            if (CreateBinFilesInFolder)
            {
                try
                {
                    Directory.CreateDirectory(BinfolderPath);

                    //--salva em um arquivo
                    Stream stream = bw.BaseStream;
                    stream.Position = startBinOffset;
                    int lenght = (int)(outOffset - startBinOffset);
                    byte[] bin = new byte[lenght];
                    stream.Read(bin, 0, lenght);
                    File.WriteAllBytes(Path.Combine(BinfolderPath, binId.ToString("D4") + ".BIN"), bin);
                    stream.Position = outOffset;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on write in file: " + binId.ToString("D3") + ".BIN" + Environment.NewLine + ex.ToString());
                }

            }

            endBinOffset = outOffset;
        }

        protected override int BinAlignment()
        {
            return 32;
        }

    }
}
