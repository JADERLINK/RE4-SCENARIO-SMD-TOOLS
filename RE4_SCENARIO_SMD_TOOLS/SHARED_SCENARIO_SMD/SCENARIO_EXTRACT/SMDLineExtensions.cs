using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT
{
    public static class SMDLineExtensions
    {
        public static SMDLine Clone(this SMDLine line)
        {
            SMDLine newLine = new SMDLine();
            newLine.PositionX = line.PositionX;
            newLine.PositionY = line.PositionY;
            newLine.PositionZ = line.PositionZ;
            newLine.AngleX = line.AngleX;
            newLine.AngleY = line.AngleY;
            newLine.AngleZ = line.AngleZ;
            newLine.ScaleX = line.ScaleX;
            newLine.ScaleY = line.ScaleY;
            newLine.ScaleZ = line.ScaleZ;

            newLine.BinFileID = line.BinFileID;
            newLine.TplFileID = line.TplFileID;
            newLine.FixedFF = line.FixedFF;
            newLine.SmxID = line.SmxID;
            newLine.Unused1 = line.Unused1;
            newLine.Unused2 = line.Unused2;
            newLine.Unused3 = line.Unused3;
            newLine.Unused4 = line.Unused4;
            newLine.Unused5 = line.Unused5;
            newLine.Unused6 = line.Unused6;
            newLine.Unused7 = line.Unused7;
            newLine.ObjectStatus = line.ObjectStatus;

            return newLine;
        }

        public static bool IsSharedBIN(this SMDLine line) 
        {
            return (line.ObjectStatus & 0x10) == 0x10;
        }

        public static bool IsNotSharedBIN(this SMDLine line)
        {
            return (line.ObjectStatus & 0x10) != 0x10;
        }
    }
}
