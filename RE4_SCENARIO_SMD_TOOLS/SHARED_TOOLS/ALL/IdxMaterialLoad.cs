using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SHARED_TOOLS.ALL
{
    public static class IdxMaterialLoad
    {
        public static IdxMaterial Load(Stream stream)
        {
            StreamReader reader = new StreamReader(stream, Encoding.ASCII);

            IdxMaterial idx = new IdxMaterial();
            idx.MaterialDic = new Dictionary<string, MaterialPart>();

            MaterialPart temp = new MaterialPart();

            while (!reader.EndOfStream)
            {
                string line = reader?.ReadLine()?.Trim()?.ToUpperInvariant();

                if (line == null || line.Length == 0 || line.StartsWith("\\") || line.StartsWith("/") || line.StartsWith("#") || line.StartsWith(":"))
                {
                    continue;
                }
                else if (line.StartsWith("USEMATERIAL"))
                {
                    temp = new MaterialPart();

                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        string name = split[1].Trim();

                        if (!idx.MaterialDic.ContainsKey(name))
                        {
                            idx.MaterialDic.Add(name, temp);
                        }
                    }
                }
                else 
                {
                   _ = Utils.SetByteHex(ref line, "MATERIAL_FLAG", ref temp.material_flag) //HEX
                    || Utils.SetByteDec(ref line, "DIFFUSE_MAP", ref temp.diffuse_map)
                    || Utils.SetByteDec(ref line, "BUMP_MAP", ref temp.bump_map)
                    || Utils.SetByteDec(ref line, "OPACITY_MAP", ref temp.opacity_map)
                    || Utils.SetByteDec(ref line, "GENERIC_SPECULAR_MAP", ref temp.generic_specular_map)
                    || Utils.SetByteDec(ref line, "INTENSITY_SPECULAR_R", ref temp.intensity_specular_r)
                    || Utils.SetByteDec(ref line, "INTENSITY_SPECULAR_G", ref temp.intensity_specular_g)
                    || Utils.SetByteDec(ref line, "INTENSITY_SPECULAR_B", ref temp.intensity_specular_b)
                    || Utils.SetByteDec(ref line, "UNK_08", ref temp.unk_08)
                    || Utils.SetByteDec(ref line, "UNK_09", ref temp.unk_09)
                    || Utils.SetByteHex(ref line, "SPECULAR_SCALE", ref temp.specular_scale) //HEX
                    || Utils.SetByteDec(ref line, "UNK_11", ref temp.unk_11)
                    || Utils.SetByteDec(ref line, "CUSTOM_SPECULAR_MAP", ref temp.custom_specular_map)
                    //------
                    || Utils.SetByteDec(ref line, "UNK_MIN_01", ref temp.unk_min_01)
                    || Utils.SetByteDec(ref line, "UNK_MIN_02", ref temp.unk_min_02)
                    || Utils.SetByteDec(ref line, "UNK_MIN_03", ref temp.unk_min_03)
                    || Utils.SetByteDec(ref line, "UNK_MIN_04", ref temp.unk_min_04)
                    || Utils.SetByteDec(ref line, "UNK_MIN_05", ref temp.unk_min_05)
                    || Utils.SetByteDec(ref line, "UNK_MIN_06", ref temp.unk_min_06)
                    || Utils.SetByteDec(ref line, "UNK_MIN_07", ref temp.unk_min_07)
                    || Utils.SetByteDec(ref line, "UNK_MIN_08", ref temp.unk_min_08)
                    || Utils.SetByteDec(ref line, "UNK_MIN_09", ref temp.unk_min_09)
                    || Utils.SetByteDec(ref line, "UNK_MIN_10", ref temp.unk_min_10)
                    || Utils.SetByteDec(ref line, "UNK_MIN_11", ref temp.unk_min_11)
                    ;
                }

            }

            return idx;
        }

    }
}
