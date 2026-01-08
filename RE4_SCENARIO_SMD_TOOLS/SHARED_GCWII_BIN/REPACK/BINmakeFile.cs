using System;
using System.Collections.Generic;
using System.IO;
using SHARED_GCWII_BIN.REPACK.Structures;
using SHARED_GCWII_BIN.EXTRACT;
using SHARED_TOOLS.ALL;
using SHARED_TOOLS.REPACK;
using SimpleEndianBinaryIO;
using SHARED_TOOLS.REPACK.Structures;

namespace SHARED_GCWII_BIN.REPACK
{
    public static class BINmakeFile
    {
        public static void MakeFile(Stream stream, long startOffset, out long endOffset,
            FinalStructure finalStructure, FinalBoneLine[] boneLines, IdxMaterial material,
            (ushort b1, ushort b2, ushort b3, ushort b4)[] BonePairLines, bool UseAltNormal,
            bool UseWeightMap, bool EnableBonepairTag, bool EnableAdjacentBoneTag, bool UseColors,
            bool IsRe1Style, byte vertex_scale)
        {
            //header 0x60 bytes or 0x40 bytes

            //bone lines
            //weightMap
            //bonepair
            //vertex position
            //vertex normal
            //vertex color
            //vertex uv
            //material

            const Endianness endianness = Endianness.BigEndian;

            var bin = new EndianBinaryWriter(stream, endianness);
            bin.BaseStream.Position = startOffset;

            var (header, IsTinyHeader) = GetHeader(finalStructure, boneLines, BonePairLines.Length, UseAltNormal, UseWeightMap, EnableBonepairTag, EnableAdjacentBoneTag, UseColors, IsRe1Style, vertex_scale);
            byte[] byteHeader = MakeHeader(header, endianness, IsTinyHeader);
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
            byte[] pos = MakeVertexPositionNormal(finalStructure.Vertex_Position_Array, endianness, false);
            bin.Write(pos, 0, pos.Length);

            //vertex normal
            bin.BaseStream.Position = header.vertex_normal_offset + startOffset;
            byte[] normal = MakeVertexPositionNormal(finalStructure.Vertex_Normal_Array, endianness, UseAltNormal && IsRe1Style == false);
            bin.Write(normal, 0, normal.Length);

            if (UseColors)
            {
                bin.BaseStream.Position = header.vertex_colour_offset + startOffset;
                byte[] colors = MakeVertexColors(finalStructure.Vertex_Color_Array);
                bin.Write(colors, 0, colors.Length);
            }

            //TexcoordUV
            bin.BaseStream.Position = header.vertex_texcoord_offset  + startOffset;
            byte[] texcoord = MakeVertexTexcoordUV(finalStructure.Vertex_UV_Array, endianness);
            bin.Write(texcoord, 0, texcoord.Length);

            //material
            bin.BaseStream.Position = header.material_offset + startOffset;
            byte[] materialGroup = MakeMaterial(finalStructure.Groups, material, endianness, IsRe1Style == false);
            bin.Write(materialGroup, 0, materialGroup.Length);

            //padding
            int padding = (32 - ((int)bin.BaseStream.Position % 32)) % 32;
            if (padding > 0) { bin.Write(new byte[padding]); }

            endOffset = bin.BaseStream.Position;
        }


        private static byte[] MakeMaterial(FinalMaterialGroup[] Groups, IdxMaterial material, Endianness endianness, bool IsMordernStyle) 
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

                List<byte> bb = new List<byte>();
                for (int im = 0; im < Groups[i].Mesh.Length; im++)
                {
                    bb.Add(Groups[i].Mesh[im].Type);
                    bb.AddRange(EndianBitConverter.GetBytes((ushort)Groups[i].Mesh[im].indexs.Length, endianness));

                    for (int ii = 0; ii < Groups[i].Mesh[im].indexs.Length; ii++)
                    {
                        bb.AddRange(EndianBitConverter.GetBytes((ushort)Groups[i].Mesh[im].indexs[ii].indexVertex, endianness));
                        bb.AddRange(EndianBitConverter.GetBytes((ushort)Groups[i].Mesh[im].indexs[ii].indexNormal, endianness));
                        if (IsMordernStyle)
                        {
                            bb.AddRange(EndianBitConverter.GetBytes((ushort)Groups[i].Mesh[im].indexs[ii].indexColor, endianness));
                        }
                        bb.AddRange(EndianBitConverter.GetBytes((ushort)Groups[i].Mesh[im].indexs[ii].indexUV, endianness));
                    }
                }
                uint buffer = (uint)bb.Count;
                buffer = ((buffer + 31) / 32) * 32;
                bb.AddRange(new byte[buffer - bb.Count]);

                uint count = 0;
                for (int im = 0; im < Groups[i].Mesh.Length; im++)
                {
                    count += (uint)Groups[i].Mesh[im].indexs.Length;
                }

                b.AddRange(EndianBitConverter.GetBytes(buffer, endianness));
                b.AddRange(EndianBitConverter.GetBytes(count, endianness));

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


        private static byte[] MakeVertexColors((byte a, byte r, byte g, byte b)[] Vertex_Color_Array) 
        {
            
            byte[] b = new byte[CalculateBytesVertexColor((uint)Vertex_Color_Array.Length)];

            int tempOffset = 0;
            for (int i = 0; i < Vertex_Color_Array.Length; i++)
            {
                b[tempOffset] = Vertex_Color_Array[i].b;
                b[tempOffset+1] = Vertex_Color_Array[i].g;
                b[tempOffset+2] = Vertex_Color_Array[i].r;
                b[tempOffset+3] = Vertex_Color_Array[i].a;

                tempOffset += 4;
            }

            return b;
        }


        private static byte[] MakeVertexTexcoordUV((short tu, short tv)[] Vertex_UV_Array, Endianness endianness)
        {
            byte[] b = new byte[CalculateBytesVertexTexcoordUV((uint)Vertex_UV_Array.Length)];

            int tempOffset = 0;
            for (int i = 0; i < Vertex_UV_Array.Length; i++)
            {
                EndianBitConverter.GetBytes(Vertex_UV_Array[i].tu, endianness).CopyTo(b, tempOffset);
                EndianBitConverter.GetBytes(Vertex_UV_Array[i].tv, endianness).CopyTo(b, tempOffset + 2);
                tempOffset += 4;
            }

            return b;
        }

        private static byte[] MakeVertexPositionNormal((short x, short y, short z, ushort WeightIndex)[] Vertex_Array, Endianness endianness, bool IsSmallForm) 
        {
            byte[] b = new byte[CalculateBytesVertexPositonNormal((uint)Vertex_Array.Length, IsSmallForm)];

            if (IsSmallForm)
            {
                int tempOffset = 0;
                for (int i = 0; i < Vertex_Array.Length; i++)
                {
                    b[tempOffset] = (byte)Vertex_Array[i].x;
                    b[tempOffset +1] = (byte)Vertex_Array[i].y;
                    b[tempOffset +2] = (byte)Vertex_Array[i].z;
                    b[tempOffset +3] = (byte)Vertex_Array[i].WeightIndex;
                    tempOffset += 4;
                }
            }
            else 
            {
                int tempOffset = 0;
                for (int i = 0; i < Vertex_Array.Length; i++)
                {
                    EndianBitConverter.GetBytes(Vertex_Array[i].x, endianness).CopyTo(b, tempOffset);
                    EndianBitConverter.GetBytes(Vertex_Array[i].y, endianness).CopyTo(b, tempOffset + 2);
                    EndianBitConverter.GetBytes(Vertex_Array[i].z, endianness).CopyTo(b, tempOffset + 4);
                    EndianBitConverter.GetBytes(Vertex_Array[i].WeightIndex, endianness).CopyTo(b, tempOffset + 6);
                    tempOffset += 8;
                }
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
            byte[] b = new byte[(((boneLines.Length * 16) + 31) / 32) * 32];

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



        private static (GcWiiBinHeader header, bool tinyheader) GetHeader(FinalStructure finalStructure, FinalBoneLine[] boneLines,
            int BonePairAmount, bool UseAltNormal, bool UseWeightMap, bool EnableBonepairTag, bool EnableAdjacentBoneTag, bool UseColors, bool IsRe1Style, byte vertex_scale) 
        {
            GcWiiBinHeader header = new GcWiiBinHeader();

            //calcula offsets;
            uint BoneOffset = 0x60;
            bool tinyheader = false;
            if (IsRe1Style || (EnableBonepairTag == false && EnableAdjacentBoneTag == false))
            {
                BoneOffset = 0x40;
                tinyheader = true;
            }
            uint WeightMapOffset = 0;
            uint BonePairOffset = 0;
            uint AdjacentBoneOffset = 0;
            uint VertexPositionOffset;
            uint VertexNormalOffset;
            uint VertexColorsOffset = 0;
            uint VertexTexcoordOffset;
            uint MaterialOffset;

            uint tempOffset = (uint)(BoneOffset + (((boneLines.Length * 16) + 31) / 32) * 32);

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
            tempOffset += (uint)CalculateBytesVertexPositonNormal((uint)finalStructure.Vertex_Position_Array.Length, false);


            //vertexNormal
            VertexNormalOffset = tempOffset;
            tempOffset += (uint)CalculateBytesVertexPositonNormal((uint)finalStructure.Vertex_Normal_Array.Length, UseAltNormal && IsRe1Style == false);

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


            uint binFlags;

            if (IsRe1Style == false)
            {
                binFlags = (uint)BinFlags.EnableModernStyle;
            }
            else 
            {
                binFlags = (uint)BinFlags.EnableRe1Flag;
            }

            ushort texture1_flags = 0x0000;

            if (EnableAdjacentBoneTag && IsRe1Style == false)
            {
                header.unknown_x08 = 0x50;
                texture1_flags = 0x0200;
                binFlags |= (uint)BinFlags.EnableAdjacentBoneTag;
            }

            if (EnableBonepairTag && IsRe1Style == false)
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

            if (UseAltNormal && IsRe1Style == false)
            {
                binFlags |= (uint)BinFlags.EnableAltNormals;
            }

            if (UseColors && IsRe1Style == false)
            {
                binFlags |= (uint)BinFlags.EnableVertexColors;
            }

            header.Bin_flags = binFlags;

            header.Tex_count = 0;
            header.vertex_scale = vertex_scale;
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

            return (header, tinyheader);
        }


        private static byte[] MakeHeader(GcWiiBinHeader header, Endianness endianness, bool IsTinyHeader) 
        {
            byte[] b = new byte[IsTinyHeader? 0x40 : 0x60];

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

            if (IsTinyHeader == false)
            {
                EndianBitConverter.GetBytes(header.bonepair_offset, endianness).CopyTo(b, 0x40);
                EndianBitConverter.GetBytes(header.adjacent_offset, endianness).CopyTo(b, 0x44);
            }
         
            return b;
        }

        // calcula quantidade de bytes usado por BonePair
        //BonePairAmount
        private static uint CalculateBytesBonePairAmount(uint count)
        {
            uint calc = 4 + (count * 8);

            uint response = ((calc + 31) / 32) * 32;
            return response;
        }


        // calcula a quantidade de bytes usado por weight map
        private static uint CalculateBytesVertexWeightMap(uint count)
        {
            uint response = 0;
            if (count != 0)
            {
                uint calc = count * 8;

                response = ((calc + 31) / 32) * 32;
            }

            return response;
        }

        // calcula a quantidade de bytes usado por vertex position/normal
        private static uint CalculateBytesVertexPositonNormal(uint count, bool IsSmallForm) 
        {
            uint calc = count * 4 * 2;
            if (IsSmallForm)
            {
                calc = count * 4;
            }

            uint response = ((calc + 31) / 32) * 32;
            return response;
        }

        //calcula a quantidade de bytes usado por texcoord UV
        private static uint CalculateBytesVertexTexcoordUV(uint count) 
        {
            uint calc = count * 2 * 2;

            uint response = ((calc + 31) / 32) * 32;
            return response;
        }

        //calcula a quantidade de bytes usado por color
        private static uint CalculateBytesVertexColor(uint count)
        {
            uint calc = count * 4; //color 1 uint

            uint response = ((calc + 31) / 32) * 32;
            return response;
        }
    }
}
