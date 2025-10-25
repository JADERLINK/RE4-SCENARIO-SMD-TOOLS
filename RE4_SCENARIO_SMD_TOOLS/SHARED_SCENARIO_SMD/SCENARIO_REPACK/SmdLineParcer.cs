using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_TOOLS.ALL;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ;

namespace SHARED_SCENARIO_SMD.SCENARIO_REPACK
{
    public static class SmdLineParcer
    {
        public static SMDLine[] Parser(int SmdAmount, Dictionary<int, SMDLineIdx> SmdLines, Dictionary<int, SmdBaseLine> ObjGroupInfos, out int binFilesCount, ref int sharedBinFilesCount)
        {
            binFilesCount = 0;

            SMDLine[] smdLines = new SMDLine[SmdAmount];
            for (int i = 0; i < SmdAmount; i++)
            {
                SMDLine line = new SMDLine();

                line.ScaleX = 1f;
                line.ScaleY = 1f;
                line.ScaleZ = 1f;
                line.FixedFF = 0xFF;
                line.SmxID = 0xFE;

                if (SmdLines.ContainsKey(i))
                {
                    line.PositionX = SmdLines[i].PositionX * CONSTs.GLOBAL_POSITION_SCALE;
                    line.PositionY = SmdLines[i].PositionY * CONSTs.GLOBAL_POSITION_SCALE;
                    line.PositionZ = SmdLines[i].PositionZ * CONSTs.GLOBAL_POSITION_SCALE;
                    line.ScaleX = SmdLines[i].ScaleX;
                    line.ScaleY = SmdLines[i].ScaleY;
                    line.ScaleZ = SmdLines[i].ScaleZ;
                    line.AngleX = SmdLines[i].AngleX;
                    line.AngleY = SmdLines[i].AngleY;
                    line.AngleZ = SmdLines[i].AngleZ;
                }
        
                if (ObjGroupInfos.ContainsKey(i))
                {
                    line.BinFileID = (byte)ObjGroupInfos[i].BinId;
                    line.SmxID = (byte)ObjGroupInfos[i].SmxId;
                    line.FixedFF = 0xFF;
                    line.ObjectStatus = ObjGroupInfos[i].Type;
                }


                if (line.IsNotSharedBIN())
                {
                    if (line.BinFileID >= binFilesCount)
                    {
                        binFilesCount = line.BinFileID + 1;
                    }
                }
                else
                {
                    if (line.BinFileID >= sharedBinFilesCount)
                    {
                        sharedBinFilesCount = line.BinFileID + 1;
                    }
                }

                smdLines[i] = line;
            }

            return smdLines;
        }

        public static SMDLine[] ParserWithPart2(int smdLinesCount, Dictionary<int, SMDLineIdx> SmdLines, Dictionary<int, SMDLineIdxPart2> SmdLinesPart2, out int binFilesCount, out int tplFilesCount, ref int sharedBinFilesCount)
        {
            binFilesCount = 0;
            tplFilesCount = 1; // tem que ter no minimo 1;

            SMDLine[] smdLines = new SMDLine[smdLinesCount];
            for (int i = 0; i < smdLinesCount; i++)
            {
                SMDLine line = new SMDLine();

                line.ScaleX = 1f;
                line.ScaleY = 1f;
                line.ScaleZ = 1f;
                line.FixedFF = 0xFF;
                line.SmxID = 0xFE;

                if (SmdLines.ContainsKey(i))
                {
                    line.PositionX = SmdLines[i].PositionX * CONSTs.GLOBAL_POSITION_SCALE;
                    line.PositionY = SmdLines[i].PositionY * CONSTs.GLOBAL_POSITION_SCALE;
                    line.PositionZ = SmdLines[i].PositionZ * CONSTs.GLOBAL_POSITION_SCALE;
                    line.ScaleX = SmdLines[i].ScaleX;
                    line.ScaleY = SmdLines[i].ScaleY;
                    line.ScaleZ = SmdLines[i].ScaleZ;
                    line.AngleX = SmdLines[i].AngleX;
                    line.AngleY = SmdLines[i].AngleY;
                    line.AngleZ = SmdLines[i].AngleZ;
                }

                if (SmdLinesPart2.ContainsKey(i))
                {
                    line.BinFileID = SmdLinesPart2[i].BinFileID;
                    line.TplFileID = SmdLinesPart2[i].TplFileID;
                    line.FixedFF = SmdLinesPart2[i].FixedFF;
                    line.SmxID = SmdLinesPart2[i].SmxID;
                    line.Unused1 = SmdLinesPart2[i].Unused1;
                    line.Unused2 = SmdLinesPart2[i].Unused2;
                    line.Unused3 = SmdLinesPart2[i].Unused3;
                    line.Unused4 = SmdLinesPart2[i].Unused4;
                    line.Unused5 = SmdLinesPart2[i].Unused5;
                    line.Unused6 = SmdLinesPart2[i].Unused6;
                    line.Unused7 = SmdLinesPart2[i].Unused7;
                    line.ObjectStatus = SmdLinesPart2[i].ObjectStatus;
                }

                if (line.IsNotSharedBIN())
                {
                    if (line.BinFileID >= binFilesCount)
                    {
                        binFilesCount = line.BinFileID + 1;
                    }
                }
                else
                {
                    if (line.BinFileID >= sharedBinFilesCount)
                    {
                        sharedBinFilesCount = line.BinFileID + 1;
                    }
                }

                if (line.TplFileID >= tplFilesCount)
                {
                    tplFilesCount = line.TplFileID + 1;
                }

                smdLines[i] = line;
            }

            return smdLines;
        }

    }
}
