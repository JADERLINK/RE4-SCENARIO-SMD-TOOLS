using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SHARED_UHD_BIN_TPL.ALL;
using SHARED_UHD_BIN_TPL.EXTRACT;
using SHARED_TOOLS.ALL;

namespace SHARED_UHD_BIN_TPL.REPACK
{
    public class MtlConverter
    {
        private string baseDirectory;

        private Dictionary<TexPathRef, (ushort width, ushort height)> TexturesDimension;

        public MtlConverter(string baseDirectory) 
        {
            this.baseDirectory = baseDirectory;
            TexturesDimension = new Dictionary<TexPathRef, (ushort width, ushort height)>();
        }

        public void Convert(IdxMtl idxmtl, ref UhdTPL uhdTpl, out IdxMaterial idxMaterial) 
        {
            idxMaterial = new IdxMaterial();
            idxMaterial.MaterialDic = new Dictionary<string, MaterialPart>();

            List<TplInfo> tplInfos = new List<TplInfo>();

            if (uhdTpl == null)
            {
                uhdTpl = new UhdTPL();
            }
            else
            {
                tplInfos.AddRange(uhdTpl.TplArray);
            }
            //-----

            foreach (var item in idxmtl.MtlDic.OrderBy(a => a.Key).OrderBy(a => a.Value.map_Kd.TextureID).ToArray())
            {
                MaterialPart mat = new MaterialPart();
                mat.material_flag = 0;
                mat.custom_specular_map = 255;
                mat.generic_specular_map = 255;
                mat.opacity_map = 255;
                mat.bump_map = 255;

                if (item.Value.map_Bump != null)
                {
                    mat.material_flag |= 0x01; // bump flag

                    mat.bump_map = TextureIndex(ref tplInfos, item.Value.map_Bump, TexType.bump);
                    mat.generic_specular_map = 0;
                    mat.intensity_specular_b = 255;
                    mat.intensity_specular_g = 255;
                    mat.intensity_specular_r = 255;
                    mat.specular_scale = 0x00;
                }

                if (item.Value.map_d != null)
                {
                    mat.material_flag |= 0x04; // opacity flag

                    mat.opacity_map = TextureIndex(ref tplInfos, item.Value.map_d, TexType.opacity);
                }

                if (item.Value.ref_specular_map != null)
                {
                    mat.material_flag |= 0x02; //generic specular flag

                    mat.intensity_specular_b = item.Value.Ks.GetB();
                    mat.intensity_specular_g = item.Value.Ks.GetG();
                    mat.intensity_specular_r = item.Value.Ks.GetR();
                    mat.specular_scale = item.Value.specular_scale;

                    if (item.Value.ref_specular_map.PackID == 0x07000000 || item.Value.ref_specular_map.PackID == 0x00000000)
                    {
                        mat.generic_specular_map = (byte)item.Value.ref_specular_map.TextureID;
                    }
                    else
                    {
                        mat.material_flag |= 0x10; // custom specular flag

                        mat.generic_specular_map = 0;
                        mat.custom_specular_map = TextureIndex(ref tplInfos, item.Value.ref_specular_map, TexType.custom_specular);
                    }
                }

                mat.diffuse_map = TextureIndex(ref tplInfos, item.Value.map_Kd, TexType.diffuse);

                idxMaterial.MaterialDic.Add(item.Key, mat);
            }

            //----
            uhdTpl.TplArray = tplInfos.ToArray();
            idxMaterial.MaterialDic = idxMaterial.MaterialDic.OrderBy(a => a.Key).ToDictionary(k => k.Key, v => v.Value);
        }

        private enum TexType : byte
        {
            diffuse = 0x0E,
            bump = 0x03,
            opacity = 0x00,
            custom_specular = 0x06,
        }

        private byte TextureIndex(ref List<TplInfo> tplInfos, TexPathRef texPathRef, TexType type) 
        {
            TplInfo info = null;

            int iindex = tplInfos.FindIndex(t => t.PackID == texPathRef.PackID && t.TextureID == texPathRef.TextureID);
            if (iindex > -1)
            {
                info = tplInfos[iindex];
            }
            else 
            {
                info = new TplInfo();
                info.PackID = texPathRef.PackID;
                info.TextureID = texPathRef.TextureID;
                info.Wrap_T = 1;
                info.Wrap_S = 1;
                info.Min_Filter = 1;
                info.Mag_Filter = 1;
                info.Width = 1;
                info.Height = 1;
                tplInfos.Add(info);
            }

            info.PixelFormatType = (uint)type;
            info.HasPalette = false;

            (ushort width, ushort height) dimension = (info.Width, info.Height);
            GetDimension(texPathRef, ref dimension);
            info.Width = dimension.width;
            info.Height = dimension.height;

            byte index = (byte)tplInfos.IndexOf(info);

            return index;
        }

        //TexturesDimension
        private void GetDimension(TexPathRef texPathRef, ref (ushort width, ushort height) dimension) 
        {
            if (TexturesDimension.ContainsKey(texPathRef))
            {
                dimension = TexturesDimension[texPathRef];
            }
            else 
            {
                GetImagemDimension(texPathRef, ref dimension);
                TexturesDimension.Add(texPathRef, dimension);
            }
        }

        private void GetImagemDimension(TexPathRef texPathRef, ref (ushort width, ushort height) dimension)
        {
            BinaryReader br = null;

            try
            {
                string file = Path.Combine(baseDirectory, texPathRef.GetPath());
                if (File.Exists(file))
                {
                    br = new BinaryReader(new FileInfo(file).OpenRead());

                    if (texPathRef.Format.ToUpperInvariant() == "DDS")
                    {
                        br.BaseStream.Position = 0xC;
                        dimension.height = (ushort)br.ReadUInt32();
                        dimension.width = (ushort)br.ReadUInt32();
                        Console.WriteLine("Image: " + texPathRef.ToString());
                        Console.WriteLine("Dimension: " + dimension.width + "x" + dimension.height);
                    }
                    else if (texPathRef.Format.ToUpperInvariant() == "TGA")
                    {
                        br.BaseStream.Position = 0xC;
                        dimension.width = br.ReadUInt16();
                        dimension.height = br.ReadUInt16();
                        Console.WriteLine("Image: " + texPathRef.ToString());
                        Console.WriteLine("Dimension: " + dimension.width + "x" + dimension.height);
                    }
                    else if (texPathRef.Format.ToUpperInvariant() == "GNF")
                    {
                        br.BaseStream.Position = 0x18;
                        uint val = br.ReadUInt32();
                        dimension.width = (ushort)(((val & 0x3FFF) >> 0) + 1);
                        dimension.height = (ushort)(((val & 0xFFFC000) >> 14) + 1);
                        Console.WriteLine("Image: " + texPathRef.ToString());
                        Console.WriteLine("Dimension: " + dimension.width + "x" + dimension.height);
                    }
                    else if (texPathRef.Format.ToUpperInvariant() == "PNG")
                    {
                        br.BaseStream.Position = 0x10;
                        dimension.width = BitConverter.ToUInt16(BitConverter.GetBytes(br.ReadUInt32()).Reverse().ToArray(), 0);
                        dimension.height = BitConverter.ToUInt16(BitConverter.GetBytes(br.ReadUInt32()).Reverse().ToArray(), 0);
                        Console.WriteLine("Image: " + texPathRef.ToString());
                        Console.WriteLine("Dimension: " + dimension.width + "x" + dimension.height);
                    }
                    else
                    {
                        Console.WriteLine("Image: " + texPathRef.ToString());
                        Console.WriteLine("Invalid image format.");
                    }
                }
                else
                {
                    Console.WriteLine("Error when getting image dimension: " + texPathRef + Environment.NewLine + "The file does not exist.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when getting image dimension: " + texPathRef + Environment.NewLine + ex.Message);
            }
            finally 
            {
                br?.Close();
            }
        
        }


    }
}
