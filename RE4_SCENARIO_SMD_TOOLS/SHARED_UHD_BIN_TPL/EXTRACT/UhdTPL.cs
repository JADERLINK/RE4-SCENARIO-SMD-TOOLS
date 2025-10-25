using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHARED_UHD_BIN_TPL.EXTRACT
{
    public class UhdTPL
    {
        public TplInfo[] TplArray;
    }

    public class TplInfo
    {
        // Palette Area
        public bool HasPalette; // se é usado essa parte
        public ushort ColorsCount;
        public byte Unpacked; //default: 0
        public byte Pad; //default: 0
        public uint PaletteFormatType;
        // uint Offset here

        // Texture Area
        public ushort Height;
        public ushort Width;
        public uint PixelFormatType;
        // uint Offset here
        public uint Wrap_S; //default: 1
        public uint Wrap_T; //default: 1
        public uint Min_Filter; //default: 1
        public uint Mag_Filter; //default: 1
        public float Lod_Bias; //default: 0
        public byte Enable_Lod; //default: 0
        public byte Min_Lod; //default: 0
        public byte Max_Lod; //default: 0
        public byte Is_Compressed; //default: 0

        public uint PackID;
        public uint TextureID;

        public override string ToString() // apenas informacional
        {
            return Width.ToString("X4") + "  "
                 + Height.ToString("X4") + "  "
                 + PixelFormatType.ToString("X8") + "  "
                 + Wrap_S.ToString("X8") + "  "
                 + Wrap_T.ToString("X8") + "  "
                 + Min_Filter.ToString("X8") + "  "
                 + Mag_Filter.ToString("X8") + "  "
                 + Lod_Bias.ToString("F9") + "  "
                 + Enable_Lod.ToString("X2") + "  "
                 + Min_Lod.ToString("X2") + "  "
                 + Max_Lod.ToString("X2") + "  "
                 + Is_Compressed.ToString("X2") + "  "
                 + PackID.ToString("X8") + "  "
                 + TextureID.ToString("X8") + "  "
                 + HasPalette + "  "
                 + ColorsCount.ToString("X4") + "  "
                 + Unpacked.ToString("X2") + "  "
                 + Pad.ToString("X2") + "  "
                 + PaletteFormatType.ToString("X8")
               ;
        }

        public override bool Equals(object obj)
        {
            return obj is TplInfo info && info.TextureID == TextureID && info.PackID == PackID;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (int)(PackID + TextureID);
            }          
        }
    }
}
