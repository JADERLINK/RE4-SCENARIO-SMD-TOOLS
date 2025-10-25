using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_TOOLS.ALL;


namespace SHARED_GCWII_BIN.ALL
{
    public static class IdxMtlParser
    {
        public static IdxMtl Parser(IdxMaterial idxMaterial, string baseFileName)
        {
            IdxMtl idx = new IdxMtl();
            idx.MtlDic = new Dictionary<string, MtlObj>();

            foreach (var mat in idxMaterial.MaterialDic)
            {
                MtlObj mtl = new MtlObj();

                mtl.map_Kd = GetTexPathRef(mat.Value.diffuse_map, baseFileName);

                mtl.Ks = new KsClass(mat.Value.intensity_specular_r, mat.Value.intensity_specular_g, mat.Value.intensity_specular_b);

                mtl.specular_scale = mat.Value.specular_scale;

                if ((mat.Value.material_flag & 0x01) == 0x01) //bump_map
                {
                    mtl.map_Bump = GetTexPathRef(mat.Value.bump_map, baseFileName);
                }

                if ((mat.Value.material_flag & 0x04) == 0x04) //opacity_map
                {
                    mtl.map_d = GetTexPathRef(mat.Value.opacity_map, baseFileName);
                }

                if ((mat.Value.material_flag & 0x02) == 0x02) //generic_specular_map
                {
                    mtl.ref_specular_map = GetTexPathRef(mat.Value.generic_specular_map, "generic_specular");
                }

                if ((mat.Value.material_flag & 0x10) == 0x10) //custom_specular_map
                {
                    mtl.ref_specular_map = GetTexPathRef(mat.Value.custom_specular_map, baseFileName);
                }

                idx.MtlDic.Add(mat.Key, mtl);
            }

            return idx;
        }


        private static TexPathRef GetTexPathRef(byte Index, string baseFileName)
        {
            return new TexPathRef(baseFileName.Replace(" ", "_"), Index, "png");
        }

    }
}
