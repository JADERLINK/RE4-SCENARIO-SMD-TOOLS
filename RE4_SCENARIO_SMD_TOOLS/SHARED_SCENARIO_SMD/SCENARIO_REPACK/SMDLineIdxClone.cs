using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_REPACK
{
    public static class SMDLineIdxClone
    {
        public static SMDLineIdx Clone(this SMDLineIdx line)
        {
            SMDLineIdx newLine = new SMDLineIdx();
            newLine.PositionX = line.PositionX;
            newLine.PositionY = line.PositionY;
            newLine.PositionZ = line.PositionZ;
            newLine.AngleX = line.AngleX;
            newLine.AngleY = line.AngleY;
            newLine.AngleZ = line.AngleZ;
            newLine.ScaleX = line.ScaleX;
            newLine.ScaleY = line.ScaleY;
            newLine.ScaleZ = line.ScaleZ;
            return newLine;
        }
    }
}
