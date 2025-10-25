using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using SHARED_TOOLS.ALL;

namespace SHARED_UHD_BIN_TPL.ALL
{
    /// <summary>
    /// representa o arquivo MTL
    /// </summary>
    public class IdxMtl
    {
        /// <summary>
        /// material name, MtlObj
        /// </summary>
        public Dictionary<string, MtlObj> MtlDic;
    }


    /// <summary>
    /// representa um material do MTL
    /// </summary>
    public class MtlObj 
    {        
        /// <summary>
        /// diffuse_texture
        /// </summary>
        public TexPathRef map_Kd = null;

        /// <summary>
        /// bump_texture (bump)
        /// </summary>
        public TexPathRef map_Bump = null;

        /// <summary>
        /// opacity_map alpha_texture
        /// </summary>
        public TexPathRef map_d = null;

        /// <summary>
        /// (map_Ks) or (map_Ns) // generic_specular_map or custom_specular_map
        /// </summary>
        public TexPathRef ref_specular_map = null;

        /// <summary>
        /// specular_scale
        /// </summary>
        public byte specular_scale = 0;

        /// <summary>
        /// intensity_specular_r, intensity_specular_g, intensity_specular_b
        /// </summary>
        public KsClass Ks = null;

    }


    /// <summary>
    /// é usado para definir o caminho das texturas no MTL
    /// </summary>
    public class TexPathRef 
    {
        public uint PackID { get; private set; }
        public uint TextureID { get; private set; }
        public string Format { get; private set; }

        public TexPathRef(uint PackID, uint TextureID, uint FormatType, bool IsPS4NS) 
        {
            this.PackID = PackID;
            this.TextureID = TextureID;
            Format = FormatType == 0xE ? "dds" : "tga";
            Format = IsPS4NS ? "dds" : Format;
        }

        public TexPathRef(uint PackID, uint TextureID, string ImageFormat)
        {
            this.PackID = PackID;
            this.TextureID = TextureID;
            Format = ImageFormat?.ToLowerInvariant() ?? "null";
        }

        public TexPathRef(string texturePath) 
        {
            Format = "null";
            if (texturePath == null)
            {
                texturePath = "";
            }

            texturePath = texturePath.Replace("\\\\", "/").Replace("\\", "/"); // Coloca o tipo de barra no padrão linux
            var split = texturePath.Split('/').Where(s => s.Length != 0).ToArray(); // Divide o path em pasta e nome do arquivo

            try
            {
                var last = split.Last().Split('.').Where(s => s.Length != 0).ToArray(); // Pega o conteúdo com o nome do arquivo e divide por ponto, para separar o formato.
                TextureID = uint.Parse(Utils.ReturnValidDecValue(last[0]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                Format = last.Last().ToLowerInvariant();
            }
            catch (Exception ex)
            {
                // Se aconteceu um erro, é porque não conseguiu pegar um ID válido, então o valor de TextureID vai ficar 0
                Console.WriteLine("Error getting TextureID from texture path: " + texturePath);
                Console.WriteLine(ex.Message);
            }

            if (split.Length - 1 > 0)
            {
                try
                {
                    // Aqui é splitado o ' ' char de espaço, pois pode ter as informações do specular na string
                    var resplit = split[split.Length - 2].Split(' ').Where(s => s.Length != 0).ToArray();
                    PackID = uint.Parse(Utils.ReturnValidHexValue(resplit.Last()), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    // Se aconteceu um erro, é porque não conseguiu pegar um ID válido, então o valor de PackID vai ficar 0
                    Console.WriteLine("Error getting PackID from texture path: " + texturePath);
                    Console.WriteLine(ex.Message);
                }
            }
            else 
            {
                Console.WriteLine("Error getting PackID from texture path: " + texturePath);
            }

        }

        public override string ToString()
        {
            return GetPath();
        }
        
        public string GetPath() 
        {
            return PackID.ToString("x8") + "/" + TextureID.ToString("D4") + "." + Format;
        }

        public override bool Equals(object obj)
        {
            return obj is TexPathRef tpr && tpr.PackID == PackID && tpr.TextureID == TextureID && tpr.Format == Format;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + PackID.GetHashCode();
                hash = hash * 23 + TextureID.GetHashCode();
                hash = hash * 23 + Format.GetHashCode();
                return hash;
            }
        }
    }

    public class KsClass 
    {
        private byte r, g, b;

        public KsClass(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public KsClass(float r, float g, float b)
        {
            this.r = (byte)(r * 255f);
            this.g = (byte)(g * 255f);
            this.b = (byte)(b * 255f);
        }

        public override string ToString()
        {
            return GetKs();
        }

        public string GetKs()
        {
           return (r / 255f).ToFloatString()
          + " " + (g / 255f).ToFloatString()
          + " " + (b / 255f).ToFloatString();
        }

        public byte GetR() 
        {
            return r;
        }

        public byte GetG()
        {
            return g;
        }

        public byte GetB()
        {
            return b;
        }

        public override bool Equals(object obj)
        {
            return obj is KsClass ks && ks.r == r && ks.g == g && ks.b == b;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + r.GetHashCode();
                hash = hash * 23 + g.GetHashCode();
                hash = hash * 23 + b.GetHashCode();
                return hash;
            }
        }
    }

}
