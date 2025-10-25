using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SimpleEndianBinaryIO;

namespace SHARED_UHD_BIN_TPL.EXTRACT
{
    public static class UhdTplDecoder
    {
        public static UhdTPL Decoder(Stream stream, long startOffset, out long endOffset, bool IsPs4NS, Endianness endianness)
        {
            UhdTPL tpl = new UhdTPL();

            EndianBinaryReader br = new EndianBinaryReader(stream, endianness);
            br.BaseStream.Position = startOffset;

            uint magic = br.ReadUInt32();
            if ( ! (magic == 0x78563412 || magic == 0x12345678))
            {
                throw new ApplicationException("Invalid TPL file!");
            }

            uint TplAmount = br.ReadUInt32();
            uint offsetToOffsetArea = br.ReadUInt32();

            if (TplAmount > 0x00_01_00_00 || offsetToOffsetArea > 0x00_01_00_00)
            {
                throw new ApplicationException("Invalid TPL file!");
            }

            br.BaseStream.Position = offsetToOffsetArea + startOffset;

            (uint image_data_offset, uint palette_offset)[] offsets = new (uint image_data_offset, uint palette_offset)[TplAmount];

            for (int i = 0; i < TplAmount; i++)
            {
                uint image_data_offset = br.ReadUInt32();
                if (IsPs4NS)
                {
                    _ = br.ReadUInt32(); // image_data_offset part2
                }

                uint palette_offset = br.ReadUInt32();
                if (IsPs4NS)
                {
                    _ = br.ReadUInt32(); // palette_offset part2
                }

                offsets[i] = (image_data_offset, palette_offset);
            }

            TplInfo[] tplArray = new TplInfo[TplAmount]; 

            for (int i = 0; i < TplAmount; i++)
            {
                TplInfo tplInfo = new TplInfo();

                if (offsets[i].palette_offset != 0)
                {
                    br.BaseStream.Position = offsets[i].palette_offset + startOffset;
                    tplInfo.HasPalette = true;
                    tplInfo.ColorsCount = br.ReadUInt16();
                    tplInfo.Unpacked = br.ReadByte();
                    tplInfo.Pad = br.ReadByte();
                    tplInfo.PaletteFormatType = br.ReadUInt32();
                }

                br.BaseStream.Position = offsets[i].image_data_offset + startOffset;

                tplInfo.Height = br.ReadUInt16(); // Primeiro Altura
                tplInfo.Width = br.ReadUInt16();  // Segundo Largura
                tplInfo.PixelFormatType = br.ReadUInt32();

                uint secundOffset = br.ReadUInt32();
                if (IsPs4NS)
                {
                    _ = br.ReadUInt32(); // secundOffset part2
                }

                tplInfo.Wrap_S = br.ReadUInt32();
                tplInfo.Wrap_T = br.ReadUInt32();
                tplInfo.Min_Filter = br.ReadUInt32();
                tplInfo.Mag_Filter = br.ReadUInt32();
                tplInfo.Lod_Bias = br.ReadSingle();

                tplInfo.Enable_Lod = br.ReadByte();
                tplInfo.Min_Lod = br.ReadByte();
                tplInfo.Max_Lod = br.ReadByte();
                tplInfo.Is_Compressed = br.ReadByte();

                br.BaseStream.Position = secundOffset + startOffset;

                tplInfo.PackID = br.ReadUInt32();
                tplInfo.TextureID = br.ReadUInt32();

                tplArray[i] = tplInfo;
            }

            tpl.TplArray = tplArray;

            endOffset = br.BaseStream.Position;
            return tpl;
        }


    }
}
