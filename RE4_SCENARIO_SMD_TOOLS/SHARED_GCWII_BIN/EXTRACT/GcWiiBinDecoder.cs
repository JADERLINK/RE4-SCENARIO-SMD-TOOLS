using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SHARED_GCWII_BIN.ALL;
using SHARED_TOOLS.ALL;
using SimpleEndianBinaryIO;

namespace SHARED_GCWII_BIN.EXTRACT
{
    public static class GcWiiBinDecoder
    {
        public static GCWIIBIN Decoder(Stream stream, long startOffset, out long endOffset) 
        {
            GCWIIBIN BIN = new GCWIIBIN();

            EndianBinaryReader br = new EndianBinaryReader(stream, Endianness.BigEndian);
            br.BaseStream.Position = startOffset;
            BIN.Header = GetHeader(br);

            //Quantidade real de vertices/normals/UV, calculado pela montagem de faces
            (int Vertex_Count, int Normal_Count, int Color_Count, int UV_Count) True_Count;
            //Quantidade real de WeightMaps
            int WeightMap_Count = 0;

            //material
            BIN.Materials = Materials(br, BIN.Header.material_offset + startOffset, BIN.Header.material_count, out True_Count, BIN.Header.ReturnHasEnableModernStyle());
            endOffset = br.BaseStream.Position;

            //--------

            BIN.Vertex_Position_Array = Get_Vertex_Position_Array(br, BIN.Header.vertex_position_offset + startOffset, True_Count.Vertex_Count, ref WeightMap_Count);
            BIN.Vertex_UV_Array = Get_Vertex_UV_Array(br, BIN.Header.vertex_texcoord_offset + startOffset, True_Count.UV_Count);

            if (BIN.Header.ReturnsHasNormalsAlternativeTag())
            {
                BIN.Vertex_Normal_Array = Get_Vertex_Normal_Array_Alt(br, BIN.Header.vertex_normal_offset + startOffset, True_Count.Normal_Count);
            }
            else 
            {
                BIN.Vertex_Normal_Array = Get_Vertex_Normal_Array(br, BIN.Header.vertex_normal_offset + startOffset, True_Count.Normal_Count);
            }
            
            if (BIN.Header.vertex_colour_offset != 0 && BIN.Header.ReturnsHasEnableVertexColorsTag())
            {
                BIN.Vertex_Color_Array = Get_Vertex_Color_Array(br, BIN.Header.vertex_colour_offset + startOffset, True_Count.Color_Count);
            }
            else
            {
                BIN.Vertex_Color_Array = new (byte a, byte r, byte g, byte b)[0];
            }

            BIN.Bones = Get_Bones(br, BIN.Header.bone_offset + startOffset, BIN.Header.bone_count);

            if (BIN.Header.weightmap_offset != 0 && (BIN.Header.weightmap_count > 0 || BIN.Header.weightmap2_count > 0))
            {
                if (BIN.Header.weightmap2_count > 255)
                {
                    BIN.WeightMaps = Get_WeightMaps(br, BIN.Header.weightmap_offset + startOffset, BIN.Header.weightmap2_count);
                }
                else
                {
                    BIN.WeightMaps = Get_WeightMaps(br, BIN.Header.weightmap_offset + startOffset, BIN.Header.weightmap_count);
                }
            }
            else 
            {
                BIN.WeightMaps = new WeightMap[0];
            }

            if (BIN.Header.bonepair_offset != 0 && BIN.Header.ReturnsHasEnableBonepairTag())
            {
                BIN.BonePairs = Get_Bonepairs(br, BIN.Header.bonepair_offset + startOffset);
            }
            else 
            {
                BIN.BonePairs = new BonePair[0];
            }

            br.BaseStream.Position = endOffset;
            return BIN;
        }


        private static MaterialBin[] Materials(BinaryReader br, long offset, ushort MatCount, out (int Vertex_Count, int Normal_Count, int Color_Count, int UV_Count) True_Count, bool IsModernStyle) 
        {
            MaterialBin[] materials = new MaterialBin[MatCount];
            br.BaseStream.Position = offset;

            int index_Vertex = 0;
            int index_Normal = 0;
            int index_Color = 0;
            int index_UV = 0;

            for (int i = 0; i < MatCount; i++)
            {
                materials[i] = Get_Material(br, ref index_Vertex, ref index_Normal, ref index_UV, ref index_Color, IsModernStyle);
            }

            True_Count = (index_Vertex, index_Normal, index_Color, index_UV);
            return materials;
        }

        private static MaterialBin Get_Material(BinaryReader br, ref int index_Vertex, ref int index_Normal, ref int index_UV, ref int index_Color, bool IsModernStyle) 
        {
            MaterialBin group = new MaterialBin();
            MaterialPart mat = new MaterialPart();
            mat.unk_min_11 = br.ReadByte();
            mat.unk_min_10 = br.ReadByte();
            mat.unk_min_09 = br.ReadByte();
            mat.unk_min_08 = br.ReadByte();
            mat.unk_min_07 = br.ReadByte();
            mat.unk_min_06 = br.ReadByte();
            mat.unk_min_05 = br.ReadByte();
            mat.unk_min_04 = br.ReadByte();
            mat.unk_min_03 = br.ReadByte();
            mat.unk_min_02 = br.ReadByte();
            mat.unk_min_01 = br.ReadByte();
            mat.material_flag = br.ReadByte();
            mat.diffuse_map = br.ReadByte();
            mat.bump_map = br.ReadByte();
            mat.opacity_map = br.ReadByte();
            mat.generic_specular_map = br.ReadByte();
            mat.intensity_specular_r = br.ReadByte();
            mat.intensity_specular_g = br.ReadByte();
            mat.intensity_specular_b = br.ReadByte();
            mat.unk_08 = br.ReadByte();
            mat.unk_09 = br.ReadByte();
            mat.specular_scale = br.ReadByte();
            mat.unk_11 = br.ReadByte();
            mat.custom_specular_map = br.ReadByte();
            group.material = mat;

            group.face_index_array = Get_face_index(br, ref index_Vertex, ref index_Normal, ref index_UV, ref index_Color, IsModernStyle);

            return group;
        }

        private static (FaceIndex i1, FaceIndex i2, FaceIndex i3)[] Get_face_index(BinaryReader br, 
            ref int index_Vertex, ref int index_Normal, ref int index_UV, ref int index_Color, bool IsModernStyle) 
        {
            List<(FaceIndex i1, FaceIndex i2, FaceIndex i3)> face_index_list = new List<(FaceIndex i1, FaceIndex i2, FaceIndex i3)>();

            uint buffer_size = br.ReadUInt32();
            _ = br.ReadUInt32(); //unused

            long tempOffset = br.BaseStream.Position;

            while (true)
            {
                if (br.BaseStream.Position >= tempOffset + buffer_size) // evita loop infinito, caso houver um erro
                {
                    break;
                }

                byte ftype = br.ReadByte();

                if (ftype == 0x98 || ftype == 0x90 || ftype == 0x80 || ftype == 0xA0)
                {
                    ushort fcount = br.ReadUInt16();

                    Get_face_index_parts(br, ref face_index_list, fcount, ftype, ref index_Vertex, ref index_Normal, ref index_UV, ref index_Color, IsModernStyle);
                }
                else 
                {
                    break;
                }
            }

            br.BaseStream.Position = tempOffset + buffer_size;
            return face_index_list.ToArray();
        }


        private static void Get_face_index_parts(BinaryReader br, ref List<(FaceIndex i1, FaceIndex i2, FaceIndex i3)> face_index_list,
            ushort fcount, byte ftype, ref int index_Vertex, ref int index_Normal, ref int index_UV, ref int index_Color, bool IsModernStyle)
        {
            FaceIndex[] indexArr = new FaceIndex[fcount];
            for (int i = 0; i < indexArr.Length; i++)
            {
                FaceIndex FIndex = new FaceIndex();
                FIndex.indexVertex = br.ReadUInt16();
                FIndex.indexNormal = br.ReadUInt16();
                if (IsModernStyle)
                {
                    FIndex.indexColor = br.ReadUInt16();
                }
                FIndex.indexUV = br.ReadUInt16();
                indexArr[i] = FIndex;

                if (FIndex.indexVertex >= index_Vertex)
                {
                    index_Vertex = FIndex.indexVertex + 1;
                }

                if (FIndex.indexNormal >= index_Normal)
                {
                    index_Normal = FIndex.indexNormal + 1;
                }

                if (FIndex.indexUV >= index_UV)
                {
                    index_UV = FIndex.indexUV + 1;
                }

                if (FIndex.indexColor >= index_Color)
                {
                    index_Color = FIndex.indexColor + 1;
                }
            }



            if (ftype == 0x98) // -- Trianglestrip
            {
                if (indexArr.Length >=3)
                {
                    int bkface = -1;
                    int a = 0;

                    FaceIndex p1 = indexArr[a];
                    FaceIndex p2 = indexArr[a + 1];
                    FaceIndex p3 = indexArr[a + 2];

                    a = a + 3;
                    bkface = bkface * -1; // --reboot face strip for each strips

                    face_index_list.Add((p1, p2, p3));

                    for (int i = 1; i < fcount - 2; i++)
                    {
                        bkface = bkface * -1;
                        p1 = p2;
                        p2 = p3;
                        p3 = indexArr[a];
                        a = a + 1;

                        if ((p1.indexVertex != p2.indexVertex) && (p1.indexVertex != p3.indexVertex) && (p2.indexVertex != p3.indexVertex))
                        {
                            if (bkface == 1)
                            {
                                face_index_list.Add((p1, p2, p3));
                            }
                            if (bkface == -1)
                            {
                                face_index_list.Add((p3, p2, p1));
                            }
                        }

                    }

                }

            }


            else if (ftype == 0x90) // -- Triangles (List)
            {
                if (indexArr.Length >= 3)
                {
                    int a = 0;
                    for (int i = 0; i < (fcount / 3); i++)
                    {
                        FaceIndex p1 = indexArr[a];
                        FaceIndex p2 = indexArr[a + 1];
                        FaceIndex p3 = indexArr[a + 2];
                        a = a + 3;

                        if ((p1.indexVertex != p2.indexVertex) && (p1.indexVertex != p3.indexVertex) && (p2.indexVertex != p3.indexVertex))
                        {
                            face_index_list.Add((p1, p2, p3));
                        }

                    }
                }
            }


            else if (ftype == 0x80) // -- Quads
            {
                if (indexArr.Length >= 4)
                {
                    int a = 0;

                    for (int i = 0; i < (fcount / 4); i++)
                    {

                        FaceIndex p1 = indexArr[a];
                        FaceIndex p2 = indexArr[a + 1];
                        FaceIndex p3 = indexArr[a + 2];
                        FaceIndex p4 = indexArr[a + 3];
                        a = a + 4;

                        if ((p1.indexVertex != p2.indexVertex) && (p1.indexVertex != p3.indexVertex) && (p2.indexVertex != p3.indexVertex))
                        {
                            face_index_list.Add((p1, p2, p3));
                            face_index_list.Add((p1, p3, p4));
                        }


                    }
                }
            }


            else if (ftype == 0xA0) // - Trianglefan
            {
                int a = 0;

                for (int i = 2; i < fcount; i++)
                {
                    FaceIndex p1 = indexArr[a];
                    FaceIndex p2 = indexArr[a + i -1];
                    FaceIndex p3 = indexArr[a + i];

                    if ((p1.indexVertex != p2.indexVertex) && (p1.indexVertex != p3.indexVertex) && (p2.indexVertex != p3.indexVertex))
                    {
                        face_index_list.Add((p1, p2, p3));
                    }
                }
            }


        }

        private static Bone[] Get_Bones(BinaryReader br, long offset, ushort count) 
        {
            Bone[] bones = new Bone[count];
           
            br.BaseStream.Position = offset;

            for (int i = 0; i < count; i++)
            {
                Bone bone = new Bone();
                bone.BoneID = br.ReadByte();
                bone.BoneParent = br.ReadByte();
                _ = br.ReadUInt16(); // padding
                bone.PositionX = br.ReadSingle();
                bone.PositionY = br.ReadSingle();
                bone.PositionZ = br.ReadSingle();
                bones[i] = bone;
            }

            return bones;
        }

        private static BonePair[] Get_Bonepairs(BinaryReader br, long offset)
        {
            br.BaseStream.Position = offset;

            uint count = br.ReadUInt32();

            BonePair[] Bonepairs = new BonePair[count];

            for (int i = 0; i < count; i++)
            {
                BonePair bonePair = new BonePair();
                bonePair.Bone1 = br.ReadUInt16();
                bonePair.Bone2 = br.ReadUInt16();
                bonePair.Bone3 = br.ReadUInt16();
                bonePair.Bone4 = br.ReadUInt16();
                Bonepairs[i] = bonePair;
            }
            return Bonepairs;
        }

        private static WeightMap[] Get_WeightMaps(BinaryReader br, long offset, ushort count) 
        {
            br.BaseStream.Position = offset;

            WeightMap[] weightMaps = new WeightMap[count];
            for (int i = 0; i < count; i++)
            {
                WeightMap weightMap = new WeightMap();
                weightMap.boneId1 = br.ReadByte();
                weightMap.boneId2 = br.ReadByte();
                weightMap.boneId3 = br.ReadByte();
                weightMap.count = br.ReadByte();
                weightMap.weight1 = br.ReadByte();
                weightMap.weight2 = br.ReadByte();
                weightMap.weight3 = br.ReadByte();
                weightMap.unused = br.ReadByte();
                weightMaps[i] = weightMap;
            }
            return weightMaps;
        }


        private static (byte a, byte r, byte g, byte b)[] Get_Vertex_Color_Array(BinaryReader br, long offset, int count) 
        {
            (byte a, byte r, byte g, byte b)[] colors = new (byte a, byte r, byte g, byte b)[count];

            br.BaseStream.Position = offset;

            for (int i = 0; i < count; i++)
            {
                byte b = br.ReadByte();
                byte g = br.ReadByte();
                byte r = br.ReadByte();
                byte a = br.ReadByte();
                colors[i] = (a, r, g, b);
            }
            return colors;
        }

        private static (short tu, short tv)[] Get_Vertex_UV_Array(BinaryReader br, long offset, int count) 
        {
            (short tu, short tv)[] uvs = new (short tu, short tv)[count];

            br.BaseStream.Position = offset;

            for (int i = 0; i < count; i++)
            {
                short tu = br.ReadInt16();
                short tv = br.ReadInt16();
                uvs[i] = (tu, tv);

            }
            return uvs;
        }

        private static (short nx, short ny, short nz, ushort weightmap_index)[] Get_Vertex_Normal_Array_Alt(BinaryReader br, long offset, int count) 
        {
            (short nx, short ny, short nz, ushort weightmap_index)[] normals = 
                new (short nx, short ny, short nz, ushort weightmap_index)[count];
            
            br.BaseStream.Position = offset;

            for (int i = 0; i < count; i++)
            {
                sbyte nx = br.ReadSByte();
                sbyte ny = br.ReadSByte();
                sbyte nz = br.ReadSByte();
                byte weightmap_index = br.ReadByte();
                normals[i] = (nx, ny, nz, weightmap_index);
            }
            return normals;
        }

        private static (short nx, short ny, short nz, ushort weightmap_index)[] Get_Vertex_Normal_Array(BinaryReader br, long offset, int count)
        {
            (short nx, short ny, short nz, ushort weightmap_index)[] normals =
                new (short nx, short ny, short nz, ushort weightmap_index)[count];

            br.BaseStream.Position = offset;

            for (int i = 0; i < count; i++)
            {
                short nx = br.ReadInt16();
                short ny = br.ReadInt16();
                short nz = br.ReadInt16();
                _ = br.ReadByte(); // padding, color_index or top of weightmap_index
                ushort weightmap_index = br.ReadByte();
                normals[i] = (nx, ny, nz, weightmap_index);
            }
            return normals;
        }

        private static (short vx, short vy, short vz, ushort weightmap_index)[] Get_Vertex_Position_Array(BinaryReader br, long offset, int count,
           ref int WeightMap_Count)
        {
            (short vx, short vy, short vz, ushort weightmap_index)[] positions = 
                new (short vx, short vy, short vz, ushort weightmap_index)[count];

            br.BaseStream.Position = offset;

            for (int i = 0; i < count; i++)
            {
                short vx = br.ReadInt16();
                short vy = br.ReadInt16();
                short vz = br.ReadInt16();
                _ = br.ReadByte(); // padding, color_index or top of weightmap_index
                ushort weightmap_index = br.ReadByte();
                positions[i] = (vx, vy, vz, weightmap_index);

                if (weightmap_index >= WeightMap_Count)
                {
                    WeightMap_Count = weightmap_index + 1;
                }
            }

            return positions;
        }

        private static GcWiiBinHeader GetHeader(BinaryReader br) 
        {
            GcWiiBinHeader header = new GcWiiBinHeader();

            header.bone_offset = br.ReadUInt32();

            if (! (header.bone_offset == 0x00000040 || header.bone_offset == 0x00000060))
            {
                throw new ApplicationException("Invalid BIN file!");
            }

            header.unknown_x04 = br.ReadUInt32();
            header.unknown_x08 = br.ReadUInt32();
            header.vertex_colour_offset = br.ReadUInt32();


            header.vertex_texcoord_offset = br.ReadUInt32();
            header.weightmap_offset = br.ReadUInt32();
            header.weightmap_count = br.ReadByte();
            header.bone_count = br.ReadByte();
            header.material_count = br.ReadUInt16();
            header.material_offset = br.ReadUInt32();


            header.Bin_flags = br.ReadUInt32();
            header.Tex_count = br.ReadUInt32();
            header.vertex_scale = br.ReadByte();
            header.unknown_x29 = br.ReadByte();
            header.weightmap2_count = br.ReadUInt16();
            header.morph_offset = br.ReadUInt32();


            header.vertex_position_offset = br.ReadUInt32();
            header.vertex_normal_offset = br.ReadUInt32();
            header.vertex_position_count = br.ReadUInt16();
            header.vertex_normal_count = br.ReadUInt16();
            header.version_flags = br.ReadUInt32();

            if (header.bone_offset >= 0x50)
            {
                header.bonepair_offset = br.ReadUInt32();
                header.adjacent_offset = br.ReadUInt32();
                header.unused_x48 = br.ReadUInt32();
                header.unused_x4C = br.ReadUInt32();
            }

            return header;
        }
    }
}
