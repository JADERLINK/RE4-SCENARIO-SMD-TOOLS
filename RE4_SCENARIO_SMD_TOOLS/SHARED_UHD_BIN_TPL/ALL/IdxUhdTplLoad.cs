using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using SHARED_UHD_BIN_TPL.EXTRACT;
using SHARED_TOOLS.ALL;

namespace SHARED_UHD_BIN_TPL.ALL
{
   public static class IdxUhdTplLoad
    {
        public static UhdTPL Load(Stream stream)
        {
            UhdTPL tpl = new UhdTPL();

            StreamReader reader = new StreamReader(stream, Encoding.ASCII);

            Dictionary<int, TplInfo> tplDic = new Dictionary<int, TplInfo>();

            TplInfo tpltemp = new TplInfo();

            int AmountTpl = 0;

            while (!reader.EndOfStream)
            {
                string line = reader?.ReadLine()?.Trim()?.ToUpperInvariant();

                if (line == null || line.Length == 0 || line.StartsWith("\\") || line.StartsWith("/") || line.StartsWith("#") || line.StartsWith(":"))
                {
                    continue;
                }

                else if (line.StartsWith("TPL")) 
                {
                    int tplID = -1;
                    try
                    {
                        tplID = int.Parse(Utils.ReturnValidDecValue(line), NumberStyles.Integer, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                    }

                    if (tplID >= AmountTpl)
                    {
                        AmountTpl = tplID+1;
                    }

                    if (tplID != -1)
                    {
                        tpltemp = new TplInfo();
                        tpltemp.Wrap_T = 1;
                        tpltemp.Wrap_S = 1;
                        tpltemp.Min_Filter = 1;
                        tpltemp.Mag_Filter = 1;

                        if (!tplDic.ContainsKey(tplID))
                        {
                            tplDic.Add(tplID, tpltemp);
                        }

                    }
                    else 
                    {
                        tpltemp = new TplInfo();
                    }
                }

                else 
                {
                    _ = Utils.SetUintHex(ref line, "PACKID", ref tpltemp.PackID)
                     || Utils.SetUintDec(ref line, "TEXTUREID", ref tpltemp.TextureID)
                     || Utils.SetUintHex(ref line, "PIXELFORMATTYPE", ref tpltemp.PixelFormatType)
                     || Utils.SetUshortDec(ref line, "WIDTH", ref tpltemp.Width)
                     || Utils.SetUshortDec(ref line, "HEIGHT", ref tpltemp.Height)
                     || Utils.SetUintDec(ref line, "WRAP_S", ref tpltemp.Wrap_S)
                     || Utils.SetUintDec(ref line, "WRAP_T", ref tpltemp.Wrap_T)
                     || Utils.SetUintDec(ref line, "MIN_FILTER", ref tpltemp.Min_Filter)
                     || Utils.SetUintDec(ref line, "MAG_FILTER", ref tpltemp.Mag_Filter)
                     || Utils.SetFloatDec(ref line, "LOD_BIAS", ref tpltemp.Lod_Bias)
                     || Utils.SetByteDec(ref line, "ENABLE_LOD", ref tpltemp.Enable_Lod)
                     || Utils.SetByteDec(ref line, "MIN_LOD", ref tpltemp.Min_Lod)
                     || Utils.SetByteDec(ref line, "MAX_LOD", ref tpltemp.Max_Lod)
                     || Utils.SetByteDec(ref line, "IS_COMPRESSED", ref tpltemp.Is_Compressed)
                     || Utils.SetBoolean(ref line, "HASPALETTE", ref tpltemp.HasPalette)
                     || Utils.SetUintHex(ref line, "PALETTEFORMATTYPE", ref tpltemp.PaletteFormatType)
                     || Utils.SetUshortDec(ref line, "COLORSCOUNT", ref tpltemp.ColorsCount)
                     || Utils.SetByteDec(ref line, "UNPACKED", ref tpltemp.Unpacked)
                     || Utils.SetByteDec(ref line, "PAD", ref tpltemp.Pad)
                     ;
                }

            }

            if (AmountTpl > 255)
            {
                AmountTpl = 255;
            }


            TplInfo[] tplArray = new TplInfo[AmountTpl];

            for (int i = 0; i < AmountTpl; i++)
            {
                if (tplDic.ContainsKey(i))
                {
                    tplArray[i] = tplDic[i];
                }
                else
                {
                    tpltemp = new TplInfo();
                    tpltemp.Wrap_T = 1;
                    tpltemp.Wrap_S = 1;
                    tpltemp.Min_Filter = 1;
                    tpltemp.Mag_Filter = 1;
                    tplArray[i] = tpltemp;
                }
            }

            tpl.TplArray = tplArray;
            return tpl;
        }
    }
}
