using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_REPACK
{
    public static class MakeSMD_Top
    {
        public static void FillTopSmd(EndianBinaryWriter bw, Endianness endianness, SmdMagic smdMagic, SMDLine[] SmdLines, long startOffset, out long EndOffset)
        {
            bw.Position = startOffset;

            bw.Write((ushort)smdMagic.magic, Endianness.LittleEndian);
            bw.Write((ushort)SmdLines.Length);

            bw.Position = startOffset + 0x10;

            if (smdMagic.magic == 0x0140) // dados adicionais
            {
                bw.Write((uint)smdMagic.extraParameters.Length);

                for (int i = 0; i < smdMagic.extraParameters.Length; i++)
                {
                    bw.Write((uint)smdMagic.extraParameters[i]);
                }
            }

            //-------------------------
            //SmdLines

            for (int i = 0; i < SmdLines.Length; i++)
            {
                float positionX = SmdLines[i].PositionX;
                float positionY = SmdLines[i].PositionY;
                float positionZ = SmdLines[i].PositionZ;

                float angleX = SmdLines[i].AngleX;
                float angleY = SmdLines[i].AngleY;
                float angleZ = SmdLines[i].AngleZ;

                float scaleX = SmdLines[i].ScaleX;
                float scaleY = SmdLines[i].ScaleY;
                float scaleZ = SmdLines[i].ScaleZ;

                byte BinFileID = SmdLines[i].BinFileID;
                byte TplFileID = SmdLines[i].TplFileID;
                byte FixedFF = SmdLines[i].FixedFF;
                byte SmxID = SmdLines[i].SmxID;
                uint unused1 = SmdLines[i].Unused1;
                uint unused2 = SmdLines[i].Unused2;
                uint unused3 = SmdLines[i].Unused3;
                uint unused4 = SmdLines[i].Unused4;
                uint unused5 = SmdLines[i].Unused5;
                uint unused6 = SmdLines[i].Unused6;
                uint unused7 = SmdLines[i].Unused7;
                uint objectStatus = SmdLines[i].ObjectStatus;

                byte[] SMDLine = new byte[72];

                EndianBitConverter.GetBytes(positionX, endianness).CopyTo(SMDLine, 0);
                EndianBitConverter.GetBytes(positionY, endianness).CopyTo(SMDLine, 4);
                EndianBitConverter.GetBytes(positionZ, endianness).CopyTo(SMDLine, 8);
                EndianBitConverter.GetBytes(angleX, endianness).CopyTo(SMDLine, 12);
                EndianBitConverter.GetBytes(angleY, endianness).CopyTo(SMDLine, 16);
                EndianBitConverter.GetBytes(angleZ, endianness).CopyTo(SMDLine, 20);
                EndianBitConverter.GetBytes(scaleX, endianness).CopyTo(SMDLine, 24);
                EndianBitConverter.GetBytes(scaleY, endianness).CopyTo(SMDLine, 28);
                EndianBitConverter.GetBytes(scaleZ, endianness).CopyTo(SMDLine, 32);
                SMDLine[36] = BinFileID;
                SMDLine[37] = TplFileID;
                SMDLine[38] = FixedFF;
                SMDLine[39] = SmxID;
                EndianBitConverter.GetBytes(unused1, endianness).CopyTo(SMDLine, 40);
                EndianBitConverter.GetBytes(unused2, endianness).CopyTo(SMDLine, 44);
                EndianBitConverter.GetBytes(unused3, endianness).CopyTo(SMDLine, 48);
                EndianBitConverter.GetBytes(unused4, endianness).CopyTo(SMDLine, 52);
                EndianBitConverter.GetBytes(unused5, endianness).CopyTo(SMDLine, 56);
                EndianBitConverter.GetBytes(unused6, endianness).CopyTo(SMDLine, 60);
                EndianBitConverter.GetBytes(unused7, endianness).CopyTo(SMDLine, 64);
                EndianBitConverter.GetBytes(objectStatus, endianness).CopyTo(SMDLine, 68);

                bw.Write(SMDLine, 0, 72);
            }

            //Alinhamento
            int _padding = (int)((16 - (bw.Position % 16)) % 16);
            bw.Write(new byte[_padding]);

            EndOffset = bw.Position;
        }

    }
}
