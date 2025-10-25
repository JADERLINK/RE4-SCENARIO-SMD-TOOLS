using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SimpleEndianBinaryIO;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT
{
    public static class SmdExtract
    {
        public static SMDLine[] Extract(Stream fileStream, out SmdMagic smdMagic, out uint offsetBin, out uint offsetTpl, Endianness endianness)
        {
            EndianBinaryReader br = new EndianBinaryReader(fileStream, endianness);
            br.Position = 0;

            ushort Magic = br.ReadUInt16(Endianness.LittleEndian);
            
            // se não for um desses não é um .SMD valido;
            // nota: tem o magic 0x00000, porem esse crasha o jogo
            if (!(Magic == 0x0040 || Magic == 0x0140 || Magic == 0x0031 || Magic == 0x0020 || Magic == 0x0010))
            {
                throw new ApplicationException("Invalid SMD file!!!");
            }

            ushort SmdLinesAmount = br.ReadUInt16();

             offsetBin = br.ReadUInt32(); // para offset dos bins (são relativos)
             offsetTpl = br.ReadUInt32(); // tpl offset relativo (aponta para um offset (relativo), dele que vai pro tpl)
             _= br.ReadUInt32(); // no gc é o fim do arquivo, no de pc fica sendo o primeiro bin.

            smdMagic = new SmdMagic();
            smdMagic.magic = Magic;

            // no magic 0x0140, tem parametros adicionais
            if (Magic == 0x0140)
            {
                uint extraAmount = br.ReadUInt32();
                smdMagic.extraParameters = new uint[extraAmount];
                for (int i = 0; i < extraAmount; i++)
                {
                    uint extraValue = br.ReadUInt32();
                    smdMagic.extraParameters[i] = extraValue;
                }
            }
            else 
            {
                smdMagic.extraParameters = new uint[0];
            }

            // 72 bytes por linha smd
            SMDLine[] SMDLines = new SMDLine[SmdLinesAmount];

            for (int i = 0; i < SmdLinesAmount; i++)
            {
                SMDLine smdLine = new SMDLine();

                smdLine.PositionX = br.ReadSingle();
                smdLine.PositionY = br.ReadSingle();
                smdLine.PositionZ = br.ReadSingle();
                smdLine.AngleX = br.ReadSingle();
                smdLine.AngleY = br.ReadSingle();
                smdLine.AngleZ = br.ReadSingle();
                smdLine.ScaleX = br.ReadSingle();
                smdLine.ScaleY = br.ReadSingle();
                smdLine.ScaleZ = br.ReadSingle();
                smdLine.BinFileID = br.ReadByte();
                smdLine.TplFileID = br.ReadByte();
                smdLine.FixedFF = br.ReadByte();
                smdLine.SmxID = br.ReadByte();
                smdLine.Unused1 = br.ReadUInt32();
                smdLine.Unused2 = br.ReadUInt32();
                smdLine.Unused3 = br.ReadUInt32();
                smdLine.Unused4 = br.ReadUInt32();
                smdLine.Unused5 = br.ReadUInt32();
                smdLine.Unused6 = br.ReadUInt32();
                smdLine.Unused7 = br.ReadUInt32();
                smdLine.ObjectStatus = br.ReadUInt32();
               
                SMDLines[i] = smdLine;
            }
            return SMDLines;
        }
    }

    public class SmdMagic
    {
        // "magic" é o inicio do arquivo .smd, os dois primeiro bytes
        // na verdade, o magic é um byte, e o segundo byte é uma flag
        // mais estou colocando os dois junto para simplificar.
        public ushort magic = 0x0040;
        // tem somente no magic 0x0140 do r100.udas/r100_004.SMD
        // esses valores são a quantidade de "SMDLine", dos Smd que estão dentro dos arquivos .dat
        public uint[] extraParameters = new uint[0];
    }

    public class SMDLine // SmdEntry
    {
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float AngleX;
        public float AngleY;
        public float AngleZ;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;

        public byte BinFileID;
        public byte TplFileID;
        public byte FixedFF;
        public byte SmxID;
        public uint Unused1;
        public uint Unused2;
        public uint Unused3;
        public uint Unused4;
        public uint Unused5;
        public uint Unused6;
        public uint Unused7;
        public uint ObjectStatus;
    }

}