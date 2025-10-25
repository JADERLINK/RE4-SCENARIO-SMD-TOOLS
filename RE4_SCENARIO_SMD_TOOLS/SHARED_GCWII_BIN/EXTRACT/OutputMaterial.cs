using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SHARED_GCWII_BIN.ALL;
using SHARED_TOOLS.ALL;

namespace SHARED_GCWII_BIN.EXTRACT
{
    public static class OutputMaterial
    {
        public static void CreateIdxMaterial(IdxMaterial idxmaterial, string baseDirectory, string baseFileName)
        {
            TextWriter text = new FileInfo(Path.Combine(baseDirectory, baseFileName + ".idxmaterial")).CreateText();
            text.WriteLine(SHARED_TOOLS.Shared.HeaderText());
            text.WriteLine();
            text.WriteLine();

            foreach (var mat in idxmaterial.MaterialDic)
            {
                text.WriteLine("UseMaterial:" + mat.Key);

                if (mat.Value.unk_min_11 != 0)
                {
                    text.WriteLine("unk_min_11:" + mat.Value.unk_min_11);
                }
                if (mat.Value.unk_min_10 != 0)
                {
                    text.WriteLine("unk_min_10:" + mat.Value.unk_min_10);
                }
                if (mat.Value.unk_min_09 != 0)
                {
                    text.WriteLine("unk_min_09:" + mat.Value.unk_min_09);
                }
                if (mat.Value.unk_min_08 != 0)
                {
                    text.WriteLine("unk_min_08:" + mat.Value.unk_min_08);
                }
                if (mat.Value.unk_min_07 != 0)
                {
                    text.WriteLine("unk_min_07:" + mat.Value.unk_min_07);
                }
                if (mat.Value.unk_min_06 != 0)
                {
                    text.WriteLine("unk_min_06:" + mat.Value.unk_min_06);
                }
                if (mat.Value.unk_min_05 != 0)
                {
                    text.WriteLine("unk_min_05:" + mat.Value.unk_min_05);
                }
                if (mat.Value.unk_min_04 != 0)
                {
                    text.WriteLine("unk_min_04:" + mat.Value.unk_min_04);
                }
                if (mat.Value.unk_min_03 != 0)
                {
                    text.WriteLine("unk_min_03:" + mat.Value.unk_min_03);
                }
                if (mat.Value.unk_min_02 != 0)
                {
                    text.WriteLine("unk_min_02:" + mat.Value.unk_min_02);
                }
                if (mat.Value.unk_min_01 != 0)
                {
                    text.WriteLine("unk_min_01:" + mat.Value.unk_min_01);
                }

                text.WriteLine("material_flag:" + mat.Value.material_flag.ToString("X2"));
                text.WriteLine("diffuse_map:" + mat.Value.diffuse_map);
                text.WriteLine("bump_map:" + mat.Value.bump_map);
                text.WriteLine("opacity_map:" + mat.Value.opacity_map);
                text.WriteLine("generic_specular_map:" + mat.Value.generic_specular_map);
                text.WriteLine("intensity_specular_r:" + mat.Value.intensity_specular_r);
                text.WriteLine("intensity_specular_g:" + mat.Value.intensity_specular_g);
                text.WriteLine("intensity_specular_b:" + mat.Value.intensity_specular_b);
                text.WriteLine("unk_08:" + mat.Value.unk_08);
                text.WriteLine("unk_09:" + mat.Value.unk_09);
                text.WriteLine("specular_scale:" + mat.Value.specular_scale.ToString("X2"));
                text.WriteLine("unk_11:" + mat.Value.unk_11);
                text.WriteLine("custom_specular_map:" + mat.Value.custom_specular_map);

                text.WriteLine();
                text.WriteLine();
            }


            text.Close();
        }

        public static void CreateMTL(IdxMtl idxmtl, string baseDirectory, string baseFileName)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;

            TextWriter text = new FileInfo(Path.Combine(baseDirectory, baseFileName + ".mtl")).CreateText();
            text.WriteLine(SHARED_TOOLS.Shared.HeaderText());
            text.WriteLine();
            text.WriteLine();

            foreach (var item in idxmtl.MtlDic)
            {
                text.WriteLine("newmtl " + item.Key);
                text.WriteLine("Ka 1.0 1.0 1.0");
                text.WriteLine("Kd 1.0 1.0 1.0");
                text.WriteLine("Ks " + item.Value.Ks);
                text.WriteLine("Ns 0");
                text.WriteLine("d 1");
                text.WriteLine("map_Kd " + item.Value.map_Kd);

                if (item.Value.map_Bump != null)
                {
                    text.WriteLine("map_Bump " + item.Value.map_Bump);
                    text.WriteLine("Bump " + item.Value.map_Bump);
                }

                if (item.Value.map_d != null)
                {
                    text.WriteLine("map_d " + item.Value.map_d);
                }

                if (item.Value.ref_specular_map != null)
                {
                    byte x = (byte)((item.Value.specular_scale & 0xF0) >> 4);
                    byte y = (byte)(item.Value.specular_scale & 0x0F);
                    int fx = x + 1;
                    int fy = y + 1;

                    text.WriteLine("map_Ns -s " + fx.ToString("D", inv)
                        + " " + fy.ToString("D", inv) + " 1 " + item.Value.ref_specular_map); //map_ks
                }

                text.WriteLine();
                text.WriteLine();
            }



            text.Close();
        }

    }
}
