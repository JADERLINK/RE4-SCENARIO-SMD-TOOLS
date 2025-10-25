using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SHARED_UHD_BIN_TPL.EXTRACT;
using SimpleEndianBinaryIO;

namespace SHARED_UHD_BIN_TPL.REPACK
{
   public static class TPLmakeFile
   {
        public static void MakeFile(UhdTPL uhdTPL, Stream stream, long startOffset, out long endOffset, bool IsPS4NS, Endianness endianness) 
        {
            EndianBinaryWriter bw = new EndianBinaryWriter(stream, endianness);
            bw.BaseStream.Position = startOffset;

            uint TplCount = (uint)uhdTPL.TplArray.Length;
            uint paletteCount = (uint)uhdTPL.TplArray.Where(x => x.HasPalette).Count();

            uint Magic = 0x78563412;
            if (IsPS4NS)
            {
                Magic = 0x12345678;
            }

            bw.Write((uint)Magic); // magic
            bw.Write((uint)TplCount); // quantidade de tpl

            uint FirstOffset = 0x0C;
            if (IsPS4NS) 
            {
                FirstOffset = 0x10;
            }

            bw.Write((uint)FirstOffset); // primeiro offset

            if (IsPS4NS)
            {
                bw.Write((uint)0); // FirstOffset part2
            }

            uint paletteArea = 0;
            if (paletteCount > 0)
            {
                paletteArea = paletteCount * 0xC;
                if (IsPS4NS)
                {
                    paletteArea = paletteCount * 0x10;
                }
            }

            uint paletteTempOffset = 0xC + (8 * TplCount);
            uint tempOffset = 0xC + (8 * TplCount) + paletteArea;

            if (IsPS4NS)
            {
                paletteTempOffset = 0x10 + (16 * TplCount);
                tempOffset = 0x10 + (16 * TplCount) + paletteArea;
            }

            for (int i = 0; i < TplCount; i++)
            {
                bw.Write((uint)tempOffset);
                if (IsPS4NS)
                {
                    bw.Write((uint)0x0); // tempOffset part2
                }

                if (paletteCount > 0 && uhdTPL.TplArray[i].HasPalette)
                {
                    bw.Write((uint)paletteTempOffset);
                    paletteTempOffset += 0xC;
                    if (IsPS4NS)
                    {
                        paletteTempOffset += 4;
                    }
                }
                else 
                {
                    bw.Write((uint)0x0);
                }
              
                if (IsPS4NS)
                {
                    bw.Write((uint)0x0); // part2
                }

                tempOffset += 36;
                if (IsPS4NS)
                {
                    tempOffset += 4;
                }
            }

            for (int i = 0; i < TplCount; i++)
            {
                TplInfo tplInfo = uhdTPL.TplArray[i];

                if (tplInfo.HasPalette)
                {
                    bw.Write((ushort)tplInfo.ColorsCount);
                    bw.Write((byte)tplInfo.Unpacked);
                    bw.Write((byte)tplInfo.Pad);
                    bw.Write((uint)tplInfo.PaletteFormatType);
                    bw.Write((uint)0x0); // unused offset
                    if (IsPS4NS)
                    {
                        bw.Write((uint)0x0); //unused offset part2
                    }
                }
            }

            for (int i = 0; i < TplCount; i++)
            {
                TplInfo tplInfo = uhdTPL.TplArray[i];

                bw.Write((ushort)tplInfo.Height); // Primeiro Altura
                bw.Write((ushort)tplInfo.Width);  // Segundo Largura
                bw.Write((uint)tplInfo.PixelFormatType);
                bw.Write((uint)tempOffset);
                if (IsPS4NS)
                {
                    bw.Write((uint)0x0); //tempOffset part2
                }
                bw.Write((uint)tplInfo.Wrap_S);
                bw.Write((uint)tplInfo.Wrap_T);
                bw.Write((uint)tplInfo.Min_Filter);
                bw.Write((uint)tplInfo.Mag_Filter);
                bw.Write((float)tplInfo.Lod_Bias);
                bw.Write((byte)tplInfo.Enable_Lod);
                bw.Write((byte)tplInfo.Min_Lod);
                bw.Write((byte)tplInfo.Max_Lod);
                bw.Write((byte)tplInfo.Is_Compressed);

                tempOffset += 8;
            }

            for (int i = 0; i < TplCount; i++)
            {
                TplInfo tplInfo = uhdTPL.TplArray[i];
                bw.Write((uint)tplInfo.PackID);
                bw.Write((uint)tplInfo.TextureID);
            }

            //padding
            int padding = (16 - ((int)bw.BaseStream.Position % 16)) % 16;
            if (padding > 0) { bw.Write(new byte[padding]); }
            
            endOffset = bw.BaseStream.Position;
        }
        
   }
}
