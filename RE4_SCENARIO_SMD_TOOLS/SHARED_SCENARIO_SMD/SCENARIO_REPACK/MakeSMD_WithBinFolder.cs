using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;

namespace SHARED_SCENARIO_SMD.SCENARIO_REPACK
{
    public static class MakeSMD_WithBinFolder
    {
        public static void CreateSMD(string baseDirectory, IdxScenario idxScenario, Endianness endianness, bool isGcWii, bool isPS4NS)
        {
            // validações
            string binFolderPath = Path.Combine(baseDirectory, idxScenario.BinFolder);
            string smdFilePath = Path.GetFullPath(Path.Combine(baseDirectory, idxScenario.SmdFileName));
            string tplFilePath = Path.GetFullPath(Path.Combine(baseDirectory, idxScenario.TplFileName));
            if (isGcWii == false) // UHD
            {
                tplFilePath = Path.GetFullPath(Path.Combine(binFolderPath, "TPL.TPL"));
            }

            if (Directory.Exists(binFolderPath) == false && idxScenario.SmdLinesDic.Any())
            {
                throw new ApplicationException("The content of the 'BinFolder' property is invalid.");
            }

            if (File.Exists(tplFilePath) == false && (idxScenario.IgnoreFirstTplFile == false || isGcWii == false))
            {
                throw new ApplicationException("The TPL file does not exist: " + Path.GetFileName(tplFilePath));
            }

            ValidateMagic.Validate(idxScenario.Magic);

            // pre processamento

            int smdLinesCount = idxScenario.SmdLinesDic.Any() ? idxScenario.SmdLinesDic.Max(a => a.Key) + 1 : 0;
            int binFilesCount = 0;
            int tplFilesCount = 0;
            int sharedBinFilesCount = 0;

            SMDLine[] SmdLines = SmdLineParcer.ParserWithPart2(smdLinesCount, idxScenario.SmdLinesDic, idxScenario.SmdLinesPart2Dic, out binFilesCount, out tplFilesCount, ref sharedBinFilesCount);

            Console.WriteLine("SMD Entry Count: " + smdLinesCount);
            Console.WriteLine("BIN Files Count: " + binFilesCount);
            if (sharedBinFilesCount != 0)
            {
                Console.WriteLine("Has Shared BIN Files!");
            }
            Console.WriteLine("TPL Files Count: " + tplFilesCount);
            Console.WriteLine("Magic: " + idxScenario.Magic.ToString("X4"));
   
            SmdMagic smdMagic = new SmdMagic();
            smdMagic.magic = idxScenario.Magic;

            if (idxScenario.Magic == 0x0140)
            {
                int extraCount = idxScenario.ExtraParametersDic.Any() ? idxScenario.ExtraParametersDic.Max(a => a.Key) + 1 : 0;
                Console.WriteLine("ExtraParametersCount:" + extraCount);
                smdMagic.extraParameters = new uint[extraCount];

                for (int i = 0; i < extraCount; i++)
                {
                    if (idxScenario.ExtraParametersDic.ContainsKey(i))
                    {
                        smdMagic.extraParameters[i] = idxScenario.ExtraParametersDic[i];
                    }
                    else
                    {
                        smdMagic.extraParameters[i] = 0;
                    }
                }
            }
           
            EndianBinaryWriter bw = new EndianBinaryWriter(new FileInfo(smdFilePath).Create(), endianness);
            MakeSMD_Top.FillTopSmd(bw, endianness, smdMagic, SmdLines, 0, out _);

            //---------------------------
            // PARTE DOS ARQUIVOS BINs

            uint BinAreaOffset = (uint)bw.Position;

            int BinOffsetBlockCount = (((binFilesCount * 4) + 15) / 16) * 16;

            bw.Write(new byte[BinOffsetBlockCount]);

            long OffsetToOffsetBin = BinAreaOffset;
            long RealOffsetBin = BinOffsetBlockCount;

            for (int i = 0; i < binFilesCount; i++)
            {
                bw.Position = OffsetToOffsetBin;
                bw.Write((uint)RealOffsetBin);
                bw.Position = BinAreaOffset + RealOffsetBin;

                string binFilePath = Path.Combine(binFolderPath, i.ToString("D4") + ".BIN");

                long tempStart = bw.Position;
                long tempEnd = tempStart;

                try
                {
                    MemoryStream ms = new MemoryStream();

                    FileInfo info = new FileInfo(binFilePath);
                    var read = info.OpenRead();
                    read.CopyTo(ms);
                    read.Close();

                    // alinhamento do bin
                    int _padding = (int)((16 - (ms.Position % 16)) % 16);
                    ms.Write(new byte[_padding], 0, _padding);

                    //verifica o magic
                    ms.Position = 0;
                    EndianBinaryReader br = new EndianBinaryReader(ms, endianness);
                    uint magic = br.ReadUInt32();

                    var ex = new ApplicationException("The BIN file is from a different version of the SMD that is being repacked.");
                    if (isGcWii)
                    {
                        if (magic != 0x40)
                        {
                            throw ex;
                        }
                    }
                    else if (isPS4NS)
                    {
                        if (magic != 0x98)
                        {
                            throw ex;
                        }
                    }
                    else
                    {
                        if (magic != 0x60)
                        {
                            throw ex;
                        }
                    }

                    // copia
                    ms.Position = 0;
                    ms.CopyTo(bw.BaseStream);
                    ms.Close();
                    tempEnd = bw.Position;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in " + i.ToString("D4") + ".BIN: " + Environment.NewLine + ex.ToString());
                    PutEmptyBin.PutBin(bw.BaseStream, tempStart, out tempEnd, isGcWii, isPS4NS, endianness);
                }
          
                OffsetToOffsetBin += 4;
                RealOffsetBin = (bw.Position - BinAreaOffset);
            }

            //---------------------------
            // PARTE DOS ARQUIVOS TPLs

            uint TplAreaOffset = (uint)bw.Position;
            int TplAlignment = isGcWii ? 32 : 16;

            int TplOffsetBlockCount = (int)((((TplAreaOffset + (tplFilesCount * 4) + (TplAlignment -1 )) / TplAlignment) * TplAlignment) - TplAreaOffset);

            bw.Write(new byte[TplOffsetBlockCount]);

            long OffsetToOffsetTpl = TplAreaOffset;
            long RealOffsetTpl = TplOffsetBlockCount;

            for (int i = 0; i < tplFilesCount; i++)
            {
                bw.Position = OffsetToOffsetTpl;
                bw.Write((uint)RealOffsetTpl);
                bw.Position = TplAreaOffset + RealOffsetTpl;

                string tempTplFilePath = tplFilePath;
                if (i > 0)
                {
                    tempTplFilePath = Path.ChangeExtension(tempTplFilePath, $"{i}.TPL");
                }

                long tempStart = bw.Position;
                long tempEnd = tempStart;

                if (idxScenario.IgnoreFirstTplFile && i == 0 && isGcWii)
                {
                    // se for para ignorar, coloca o TPL em branco, porem somente se for GCWII
                    PutEmptyTpl.PutTpl(bw.BaseStream, tempStart, out tempEnd, isGcWii, isPS4NS, endianness);
                }
                else
                {
                    try
                    {
                        MemoryStream ms = new MemoryStream();
                        ms.Position = 0;

                        FileInfo info = new FileInfo(tempTplFilePath);
                        var read = info.OpenRead();
                        read.CopyTo(ms);
                        read.Close();

                        // alinhamento do tpl
                        int _padding = (int)((TplAlignment - (ms.Position % TplAlignment)) % TplAlignment);
                        ms.Write(new byte[_padding], 0, _padding);
                        long tplLength = ms.Position;

                        //verifica o magic
                        ms.Position = 0;
                        EndianBinaryReader br = new EndianBinaryReader(ms, endianness);
                        uint __magic = br.ReadUInt32();
                        uint __tplAmount = br.ReadUInt32();
                        uint __offsetToOffsetArea = br.ReadUInt32();

                        var ex = new ApplicationException("The TPL file is from a different version of the SMD that is being repacked.");
                        if (isGcWii)
                        {
                            if (__magic != 0x0020AF30)
                            {
                                throw ex;
                            }
                        }
                        else if (isPS4NS)
                        {
                            if (!(__magic == 0x78563412 || __magic == 0x12345678) || __tplAmount > 0x00_01_00_00 || __offsetToOffsetArea < 0x10)
                            {
                                throw ex;
                            }
                        }
                        else
                        {
                            if (!(__magic == 0x78563412 || __magic == 0x12345678) || __tplAmount > 0x00_01_00_00 || __offsetToOffsetArea >= 0x10)
                            {
                                throw ex;
                            }
                        }

                        // copia
                        ms.Position = 0;
                        ms.CopyTo(bw.BaseStream);
                        ms.Close();
                        tempEnd = bw.Position;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in {Path.GetFileName(tempTplFilePath)}: " + Environment.NewLine + ex.ToString());
                        PutEmptyTpl.PutTpl(bw.BaseStream, tempStart, out tempEnd, isGcWii, isPS4NS, endianness);
                    }
                }

                OffsetToOffsetTpl += 4;
                RealOffsetTpl = (bw.Position - TplAreaOffset);
            }

            uint endFileOffset = (uint)bw.Position;

            //coloca os offsets no topo
            bw.Position = 4;
            bw.Write((uint)BinAreaOffset);
            bw.Write((uint)TplAreaOffset);
            bw.Write((uint)endFileOffset);

            bw.Close();
        }

    }
}
