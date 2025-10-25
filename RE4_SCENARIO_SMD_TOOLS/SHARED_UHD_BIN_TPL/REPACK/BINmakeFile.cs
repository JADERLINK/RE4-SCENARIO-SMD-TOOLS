using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SHARED_UHD_BIN_TPL.REPACK.Structures;
using SHARED_UHD_BIN_TPL.EXTRACT;
using SHARED_TOOLS.ALL;
using SHARED_TOOLS.REPACK;
using SimpleEndianBinaryIO;
using SHARED_TOOLS.REPACK.Structures;

namespace SHARED_UHD_BIN_TPL.REPACK
{
    public static class BINmakeFile
    {
        public static void MakeFile(Stream stream, long startOffset, out long endOffset, 
            FinalStructure finalStructure, FinalBoneLine[] boneLines, IdxMaterial material, 
            (ushort b1, ushort b2, ushort b3, ushort b4)[] BonePairLines, bool UseExtendedNormals, 
            bool UseWeightMap, bool EnableBonepairTag, bool EnableAdjacentBoneTag, bool UseColors, 
            bool IsPS4NS, Endianness endianness)
        {
            //header 0x60 bytes

            //bone lines
            //weightMap
            //bonepair
            //vertex position
            //vertex weight
            //vertex normal
            //vertex color
            //vertex uv
            //material

            var bin = new EndianBinaryWriter(stream, endianness);
            bin.BaseStream.Position = startOffset;

            UhdBinHeader header = GetHeader(finalStructure, boneLines, BonePairLines.Length, UseExtendedNormals, UseWeightMap, EnableBonepairTag, EnableAdjacentBoneTag, UseColors, IsPS4NS);
            byte[] byteHeader = IsPS4NS ? MakeHeaderPS4NS(header) : MakeHeader(header, endianness);
            bin.Write(byteHeader, 0, byteHeader.Length);

            bin.BaseStream.Position = header.bone_offset + startOffset;
            byte[] bones = MakeBone(boneLines);
            bin.Write(bones, 0, bones.Length);

            if (header.weightmap_count != 0 && UseWeightMap)
            {
                bin.BaseStream.Position = header.weightmap_offset + startOffset;
                byte[] weightMap = MakeWeightMap(finalStructure.WeightMaps);
                bin.Write(weightMap, 0, weightMap.Length);
            }

            if (BonePairLines.Length != 0 && EnableBonepairTag)
            {
                bin.BaseStream.Position = header.bonepair_offset + startOffset;
                byte[] BonePair = MakeBonepair(BonePairLines, endianness);
                bin.Write(BonePair, 0, BonePair.Length);
            }

            //vertex position
            bin.BaseStream.Position = header.vertex_position_offset + startOffset;
            byte[] pos = MakeVertexPositionNormal(finalStructure.Vertex_Position_Array, endianness);
            bin.Write(pos, 0, pos.Length);

            if (header.vertex_weight_index_offset != 0 && UseWeightMap)
            {
                bin.BaseStream.Position = header.vertex_weight_index_offset + startOffset;
                byte[] weight_index = MakeVertexWeightIndex(finalStructure.WeightIndex, endianness);
                bin.Write(weight_index, 0, weight_index.Length);
            }

            //vertex normal
            bin.BaseStream.Position = header.vertex_normal_offset + startOffset;
            byte[] normal = MakeVertexPositionNormal(finalStructure.Vertex_Normal_Array, endianness);
            bin.Write(normal, 0, normal.Length);

            if (UseColors)
            {
                bin.BaseStream.Position = header.vertex_colour_offset + startOffset;
                byte[] colors = MakeVertexColors(finalStructure.Vertex_Color_Array, endianness);
                bin.Write(colors, 0, colors.Length);
            }

            //TexcoordUV
            bin.BaseStream.Position = header.vertex_texcoord_offset  + startOffset;
            byte[] texcoord = MakeVertexTexcoordUV(finalStructure.Vertex_UV_Array, endianness);
            bin.Write(texcoord, 0, texcoord.Length);

            //material
            bin.BaseStream.Position = header.material_offset + startOffset;
            byte[] materialGroup = MakeMaterial(finalStructure.Groups, material, endianness);
            bin.Write(materialGroup, 0, materialGroup.Length);

            //padding
            int padding = (16 - ((int)bin.BaseStream.Position % 16)) % 16;
            if (padding > 0) { bin.Write(new byte[padding]); }

            endOffset = bin.BaseStream.Position;
        }


        private static byte[] MakeMaterial(FinalMaterialGroup[] Groups, IdxMaterial material, Endianness endianness) 
        {
            List<byte> b = new List<byte>();

            for (int i = 0; i < Groups.Length; i++)
            {
                if (material.MaterialDic.ContainsKey(Groups[i].materialName))
                {
                    b.AddRange(material.MaterialDic[Groups[i].materialName].GetArray());
                    Console.WriteLine($"[{i}] Used material: {Groups[i].materialName}");
                }
                else
                {
                    //usado quando não encontrado um material valido
                    b.AddRange(EmpatyMaterialArray());
                    Console.WriteLine($"[{i}] Not found material: {Groups[i].materialName}");
                }

                uint buffer = (uint)(4 + (Groups[i].Mesh.Length * 4));
                buffer = ((buffer + 15) / 16) * 16;

                uint count = 0;
                for (int im = 0; im < Groups[i].Mesh.Length; im++)
                {
                    count += Groups[i].Mesh[im].Count;
                }

                b.AddRange(EndianBitConverter.GetBytes(buffer, endianness));
                b.AddRange(EndianBitConverter.GetBytes(count, endianness));

                byte[] bb = new byte[buffer];
                EndianBitConverter.GetBytes((uint)Groups[i].Mesh.Length, endianness).CopyTo(bb, 0);
                int tempOffset = 4;
                for (int im = 0; im < Groups[i].Mesh.Length; im++)
                {
                    EndianBitConverter.GetBytes(Groups[i].Mesh[im].Type, endianness).CopyTo(bb, tempOffset);
                    EndianBitConverter.GetBytes(Groups[i].Mesh[im].Count, endianness).CopyTo(bb, tempOffset +2);
                    tempOffset += 4;
                }
                b.AddRange(bb);

            }

            return b.ToArray();
        }

        private static byte[] EmpatyMaterialArray() 
        {
            byte[] b = new byte[24];
            b[13] = 0xFF;
            b[14] = 0xFF;
            b[15] = 0xFF;
            b[23] = 0xFF;
            return b;
        }


        private static byte[] MakeVertexColors((byte a, byte r, byte g, byte b)[] Vertex_Color_Array, Endianness endianness) 
        {
            
            byte[] b = new byte[CalculateBytesVertexColor((uint)Vertex_Color_Array.Length)];

            int tempOffset = 0;
            for (int i = 0; i < Vertex_Color_Array.Length; i++)
            {
                if (endianness == Endianness.BigEndian)
                {
                    b[tempOffset] = Vertex_Color_Array[i].b;
                    b[tempOffset + 1] = Vertex_Color_Array[i].g;
                    b[tempOffset + 2] = Vertex_Color_Array[i].r;
                    b[tempOffset + 3] = Vertex_Color_Array[i].a;
                }
                else
                {
                    b[tempOffset] = Vertex_Color_Array[i].a;
                    b[tempOffset + 1] = Vertex_Color_Array[i].r;
                    b[tempOffset + 2] = Vertex_Color_Array[i].g;
                    b[tempOffset + 3] = Vertex_Color_Array[i].b;
                }

                tempOffset += 4;
            }

            return b;
        }


        private static byte[] MakeVertexTexcoordUV((float tu, float tv)[] Vertex_UV_Array, Endianness endianness)
        {
            byte[] b = new byte[CalculateBytesVertexTexcoordUV((uint)Vertex_UV_Array.Length)];

            int tempOffset = 0;
            for (int i = 0; i < Vertex_UV_Array.Length; i++)
            {
                EndianBitConverter.GetBytes(Vertex_UV_Array[i].tu, endianness).CopyTo(b, tempOffset);
                EndianBitConverter.GetBytes(Vertex_UV_Array[i].tv, endianness).CopyTo(b, tempOffset + 4);
                tempOffset += 8;
            }

            return b;
        }


        private static byte[] MakeVertexWeightIndex(ushort[] WeightIndex, Endianness endianness)
        {
            byte[] b = new byte[CalculateBytesVertexWeightIndex((uint)WeightIndex.Length)];

            int tempOffset = 0;
            for (int i = 0; i < WeightIndex.Length; i++)
            {
                EndianBitConverter.GetBytes(WeightIndex[i], endianness).CopyTo(b, tempOffset);
                tempOffset += 2;
            }
            return b;
        }

        private static byte[] MakeVertexPositionNormal((float x, float y, float z)[] Vertex_Array, Endianness endianness) 
        {
            byte[] b = new byte[CalculateBytesVertexPositonNormal((uint)Vertex_Array.Length)];

            int tempOffset = 0;
            for (int i = 0; i < Vertex_Array.Length; i++)
            {
                EndianBitConverter.GetBytes(Vertex_Array[i].x, endianness).CopyTo(b, tempOffset);
                EndianBitConverter.GetBytes(Vertex_Array[i].y, endianness).CopyTo(b, tempOffset + 4);
                EndianBitConverter.GetBytes(Vertex_Array[i].z, endianness).CopyTo(b, tempOffset + 8);
                tempOffset += 12;
            }
            return b;
        }

        private static byte[] MakeWeightMap(FinalWeightMap[] WeightMaps) 
        {
            byte[] b = new byte[CalculateBytesVertexWeightMap((uint)WeightMaps.Length)];

            int tempOffset = 0;
            for (int i = 0; i < WeightMaps.Length; i++)
            {
                b[tempOffset + 0x0] = WeightMaps[i].BoneID1;
                b[tempOffset + 0x1] = WeightMaps[i].BoneID2;
                b[tempOffset + 0x2] = WeightMaps[i].BoneID3;
                b[tempOffset + 0x3] = WeightMaps[i].Links;
                b[tempOffset + 0x4] = WeightMaps[i].Weight1;
                b[tempOffset + 0x5] = WeightMaps[i].Weight2;
                b[tempOffset + 0x6] = WeightMaps[i].Weight3;
                b[tempOffset + 0x7] = 0;
                tempOffset += 8;
            }
            return b;
        }

        private static byte[] MakeBone(FinalBoneLine[] boneLines) 
        {
            byte[] b = new byte[(boneLines.Length * 16) + 16];

            int offset = 0;
            for (int i = 0; i < boneLines.Length; i++)
            {
                boneLines[i].Line.CopyTo(b, offset);
                offset += 16;
            }

            return b;
        }

        private static byte[] MakeBonepair((ushort b1, ushort b2, ushort b3, ushort b4)[] bonepairLines, Endianness endianness) 
        {
            byte[] b = new byte[CalculateBytesBonePairAmount((uint)bonepairLines.Length)];
            EndianBitConverter.GetBytes((uint)bonepairLines.Length, endianness).CopyTo(b, 0);

            int offset = 4;
            for (int i = 0; i < bonepairLines.Length; i++)
            {
                EndianBitConverter.GetBytes((ushort)bonepairLines[i].b1, endianness).CopyTo(b, offset);
                EndianBitConverter.GetBytes((ushort)bonepairLines[i].b2, endianness).CopyTo(b, offset + 2);
                EndianBitConverter.GetBytes((ushort)bonepairLines[i].b3, endianness).CopyTo(b, offset + 4);
                EndianBitConverter.GetBytes((ushort)bonepairLines[i].b4, endianness).CopyTo(b, offset + 6);
                offset += 8;
            }

            return b;
        }



        private static UhdBinHeader GetHeader(FinalStructure finalStructure, FinalBoneLine[] boneLines,
            int BonePairAmount, bool UseExtendedNormals, bool UseWeightMap, bool EnableBonepairTag, bool EnableAdjacentBoneTag, bool UseColors, bool IsPS4NS) 
        {
            UhdBinHeader header = new UhdBinHeader();

            //calcula offsets;
            uint BoneOffset = 0x60;
            if (IsPS4NS)
            {
                BoneOffset = 0x98;
            }
            uint WeightMapOffset = 0;
            uint BonePairOffset = 0;
            uint AdjacentBoneOffset = 0;
            uint VertexPositionOffset;
            uint VertexWeightIndexOffset = 0;
            uint VertexNormalOffset;
            uint VertexWeight2IndexOffset = 0;
            uint VertexColorsOffset = 0;
            uint VertexTexcoordOffset;
            uint MaterialOffset;

            uint tempOffset = (uint)(BoneOffset + (boneLines.Length * 16) + 16);

            if (finalStructure.WeightMaps != null && finalStructure.WeightMaps.Length != 0 && UseWeightMap)
            {
                WeightMapOffset = tempOffset;
                tempOffset += (uint)CalculateBytesVertexWeightMap((uint)finalStructure.WeightMaps.Length);
            }


            //BonePair
            if (BonePairAmount != 0)
            {
                BonePairOffset = tempOffset;
                tempOffset += (uint)CalculateBytesBonePairAmount((uint)BonePairAmount);
            }

            //vertexPosition
            VertexPositionOffset = tempOffset;
            tempOffset += (uint)CalculateBytesVertexPositonNormal((uint)finalStructure.Vertex_Position_Array.Length);


            if (finalStructure.WeightIndex != null && finalStructure.WeightIndex.Length != 0 && UseWeightMap)
            {
                VertexWeightIndexOffset = tempOffset;
                VertexWeight2IndexOffset = tempOffset;

                tempOffset += (uint)CalculateBytesVertexWeightIndex((uint)finalStructure.WeightIndex.Length);
            }

            //vertexNormal
            VertexNormalOffset = tempOffset;
            tempOffset += (uint)CalculateBytesVertexPositonNormal((uint)finalStructure.Vertex_Normal_Array.Length);

            if (UseColors)
            {
                VertexColorsOffset = tempOffset;
                tempOffset += (uint)CalculateBytesVertexColor((uint)finalStructure.Vertex_Color_Array.Length);
            }

            //VertexTexcoord
            VertexTexcoordOffset = tempOffset;
            tempOffset += (uint)CalculateBytesVertexTexcoordUV((uint)finalStructure.Vertex_UV_Array.Length);


            //material
            MaterialOffset = tempOffset;


            //preenche o header
            header.bone_offset = BoneOffset;
            header.unknown_x04 = 0;
            header.unknown_x08 = 0; // offset // 50 00 00 00

            
            header.vertex_colour_offset = VertexColorsOffset;

            header.vertex_texcoord_offset = VertexTexcoordOffset;
            header.weightmap_offset = WeightMapOffset;

            if (UseWeightMap)
            {
                header.weightmap_count = (byte)finalStructure.WeightMaps.Length;
                header.weightmap2_count = (ushort)finalStructure.WeightMaps.Length; //--same as weightcount

                if (finalStructure.WeightMaps.Length > ushort.MaxValue)
                {
                    header.weightmap_count = byte.MaxValue;
                    header.weightmap2_count = ushort.MaxValue;
                }
            }
            else 
            {
                header.weightmap_count = 0;
                header.weightmap2_count = 0;
            }


            header.bone_count =(byte)boneLines.Length;
            header.material_count = (ushort)finalStructure.Groups.Length;
            header.material_offset = MaterialOffset;

            uint binFlags = (uint)BinFlags.AlwaysActivated;
            ushort texture1_flags = 0x0000;

            if (EnableAdjacentBoneTag)
            {
                header.unknown_x08 = 0x50;
                texture1_flags = 0x0200;
                binFlags |= (uint)BinFlags.EnableAdjacentBoneTag;
            }

            if (EnableBonepairTag)
            {
                header.unknown_x08 = 0x50;
                texture1_flags = 0x0300;
                binFlags |= (uint)BinFlags.EnableAdjacentBoneTag;
                binFlags |= (uint)BinFlags.EnableBonepairTag;
            }

            if (texture1_flags != 0x0000)
            {
                header.version_flags = 0x20030818;
            }
            else 
            {
                header.version_flags = 0x20010801;
            }

            if (UseExtendedNormals)
            {
                binFlags |= (uint)BinFlags.EnableAltNormals;
            }

            if (UseColors)
            {
                binFlags |= (uint)BinFlags.EnableVertexColors;
            }
            //----------

            header.Bin_flags = binFlags;

            header.Tex_count = 0;
            header.vertex_scale = 0;
            header.unknown_x29 = 0;
          
            header.morph_offset = 0; // não suportado nessa versão do programa


            header.vertex_position_offset = VertexPositionOffset;
            header.vertex_normal_offset = VertexNormalOffset;

            //---
            uint Vertex_Position_Count = (uint)finalStructure.Vertex_Position_Array.Length;
            if (Vertex_Position_Count < ushort.MaxValue)
            {
                header.vertex_position_count = (ushort)Vertex_Position_Count;
            }
            else 
            {
                header.vertex_position_count = ushort.MaxValue;
            }
            
            uint Vertex_Normal_Count = (uint)finalStructure.Vertex_Normal_Array.Length;
            if (Vertex_Normal_Count < ushort.MaxValue)
            {
                header.vertex_normal_count = (ushort)Vertex_Normal_Count;
            }
            else
            {
                header.vertex_normal_count = ushort.MaxValue;
            }
            //----

            header.bonepair_offset = BonePairOffset;
            header.adjacent_offset = AdjacentBoneOffset;
            header.vertex_weight_index_offset = VertexWeightIndexOffset;  //--vertex weights id's array (2 words )* numvertex
            header.vertex_weight2_index_offset = VertexWeight2IndexOffset; //--vertex weights array (2 words )* numvertex

            return header;
        }


        private static byte[] MakeHeader(UhdBinHeader header, Endianness endianness) 
        {
            byte[] b = new byte[0x60];

            EndianBitConverter.GetBytes(header.bone_offset, endianness).CopyTo(b, 0x00);
            EndianBitConverter.GetBytes(header.unknown_x04, endianness).CopyTo(b, 0x04);
            EndianBitConverter.GetBytes(header.unknown_x08, endianness).CopyTo(b, 0x08);
            EndianBitConverter.GetBytes(header.vertex_colour_offset, endianness).CopyTo(b, 0x0C);


            EndianBitConverter.GetBytes(header.vertex_texcoord_offset, endianness).CopyTo(b, 0x10);
            EndianBitConverter.GetBytes(header.weightmap_offset, endianness).CopyTo(b, 0x14);
            b[0x18] = header.weightmap_count;
            b[0x19] = header.bone_count;
            EndianBitConverter.GetBytes(header.material_count, endianness).CopyTo(b, 0x1A);
            EndianBitConverter.GetBytes(header.material_offset, endianness).CopyTo(b, 0x1C);

            EndianBitConverter.GetBytes(header.Bin_flags, endianness).CopyTo(b, 0x20);
            EndianBitConverter.GetBytes(header.Tex_count, endianness).CopyTo(b, 0x24);
            b[0x28] = header.vertex_scale;
            b[0x29] = header.unknown_x29;
            EndianBitConverter.GetBytes(header.weightmap2_count, endianness).CopyTo(b, 0x2A);
            EndianBitConverter.GetBytes(header.morph_offset, endianness).CopyTo(b, 0x2C);


            EndianBitConverter.GetBytes(header.vertex_position_offset, endianness).CopyTo(b, 0x30);
            EndianBitConverter.GetBytes(header.vertex_normal_offset, endianness).CopyTo(b, 0x34);
            EndianBitConverter.GetBytes(header.vertex_position_count, endianness).CopyTo(b, 0x38);
            EndianBitConverter.GetBytes(header.vertex_normal_count, endianness).CopyTo(b, 0x3A);
            EndianBitConverter.GetBytes(header.version_flags, endianness).CopyTo(b, 0x3C);


            EndianBitConverter.GetBytes(header.bonepair_offset, endianness).CopyTo(b, 0x40);
            EndianBitConverter.GetBytes(header.adjacent_offset, endianness).CopyTo(b, 0x44);
            EndianBitConverter.GetBytes(header.vertex_weight_index_offset, endianness).CopyTo(b, 0x48);
            EndianBitConverter.GetBytes(header.vertex_weight2_index_offset, endianness).CopyTo(b, 0x4C);
 
            return b;
        }

        private static byte[] MakeHeaderPS4NS(UhdBinHeader header) 
        {
            byte[] b = new byte[0x98];

            BitConverter.GetBytes(header.bone_offset).CopyTo(b, 0x00);
            BitConverter.GetBytes(header.unknown_x04).CopyTo(b, 0x08);
            BitConverter.GetBytes(header.unknown_x08).CopyTo(b, 0x0C);
            BitConverter.GetBytes(header.vertex_colour_offset).CopyTo(b, 0x10);


            BitConverter.GetBytes(header.vertex_texcoord_offset).CopyTo(b, 0x18);
            BitConverter.GetBytes(header.weightmap_offset).CopyTo(b, 0x20);
            b[0x28] = header.weightmap_count;
            b[0x29] = header.bone_count;
            BitConverter.GetBytes(header.material_count).CopyTo(b, 0x2A);
            BitConverter.GetBytes(header.material_offset).CopyTo(b, 0x30);

            BitConverter.GetBytes(header.Bin_flags).CopyTo(b, 0x38);
            BitConverter.GetBytes(header.Tex_count).CopyTo(b, 0x3C);
            b[0x40] = header.vertex_scale;
            b[0x41] = header.unknown_x29;
            BitConverter.GetBytes(header.weightmap2_count).CopyTo(b, 0x42);
            BitConverter.GetBytes(header.morph_offset).CopyTo(b, 0x44);


            BitConverter.GetBytes(header.vertex_position_offset).CopyTo(b, 0x48);
            BitConverter.GetBytes(header.vertex_normal_offset).CopyTo(b, 0x50);
            BitConverter.GetBytes(header.vertex_position_count).CopyTo(b, 0x58);
            BitConverter.GetBytes(header.vertex_normal_count).CopyTo(b, 0x5A);
            BitConverter.GetBytes(header.version_flags).CopyTo(b, 0x5C);


            BitConverter.GetBytes(header.bonepair_offset).CopyTo(b, 0x60);
            BitConverter.GetBytes(header.adjacent_offset).CopyTo(b, 0x68);
            BitConverter.GetBytes(header.vertex_weight_index_offset).CopyTo(b, 0x70);
            BitConverter.GetBytes(header.vertex_weight2_index_offset).CopyTo(b, 0x78);

            return b;
        }



        // calcula quantidade de bytes usado por BonePair
        //BonePairAmount
        private static uint CalculateBytesBonePairAmount(uint count)
        {
            uint calc = 4 + (count * 8);

            uint response = ((calc + 15) / 16) * 16;
            return response;
        }


        // calcula a quantidade de bytes usado por weight map
        private static uint CalculateBytesVertexWeightMap(uint count)
        {
            uint response = 0;
            if (count != 0)
            {
                uint calc = count * 8;

                response = ((calc + 15) / 16) * 16;
            }

            return response;
        }

        // calcula a quantidade de bytes usado por weight index
        private static uint CalculateBytesVertexWeightIndex(uint count)
        {
            uint calc = count * 2;

            uint response = ((calc + 15) / 16) * 16;
            return response;
        }

        // calcula a quantidade de bytes usado por vertex position/normal
        private static uint CalculateBytesVertexPositonNormal(uint count) 
        {
            uint calc = count * 3 * 4;

            uint response = ((calc + 15) / 16) * 16;
            return response;
        }

        //calcula a quantidade de bytes usado por texcoord UV
        private static uint CalculateBytesVertexTexcoordUV(uint count) 
        {
            uint calc = count * 2 * 4;

            uint response = ((calc + 15) / 16) * 16;
            return response;
        }

        //calcula a quantidade de bytes usado por color
        private static uint CalculateBytesVertexColor(uint count)
        {
            uint calc = count * 4; //color 1 uint

            uint response = ((calc + 15) / 16) * 16;
            return response;
        }
    }
}
