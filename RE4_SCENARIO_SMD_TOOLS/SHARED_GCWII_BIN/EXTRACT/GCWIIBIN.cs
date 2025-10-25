using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_TOOLS.ALL;

namespace SHARED_GCWII_BIN.EXTRACT
{
    public class GCWIIBIN
    {
        public GcWiiBinHeader Header;

        public (short vx, short vy, short vz, ushort weightmap_index)[] Vertex_Position_Array;
        public (short nx, short ny, short nz, ushort weightmap_index)[] Vertex_Normal_Array; // short or sbyte, ushort or byte
        public (short tu, short tv)[] Vertex_UV_Array;
        public (byte a, byte r, byte g, byte b)[] Vertex_Color_Array;

        public Bone[] Bones;
        public WeightMap[] WeightMaps;
        public MaterialBin[] Materials;
        public BonePair[] BonePairs;
    }

    public struct BonePair
    {
        public ushort Bone1;
        public ushort Bone2;
        public ushort Bone3;
        public ushort Bone4;
    }


    public class MaterialBin
    {
        public MaterialPart material;

        public (FaceIndex i1, FaceIndex i2, FaceIndex i3)[] face_index_array;
    }

    public struct FaceIndex
    {
        public ushort indexVertex;
        public ushort indexNormal;
        public ushort indexColor;
        public ushort indexUV;
    }

    public struct WeightMap
    {
        public byte boneId1;
        public byte boneId2;
        public byte boneId3;
        public byte count;
        public byte weight1;
        public byte weight2;
        public byte weight3;
        public byte unused;
    }

    public struct Bone
    {
        public byte BoneID;
        public byte BoneParent;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
    }


    public class GcWiiBinHeader
    {
        public uint bone_offset;
        public uint unknown_x04;
        public uint unknown_x08;
        public uint vertex_colour_offset; //colors

        public uint vertex_texcoord_offset; // UV
        public uint weightmap_offset;
        public byte weightmap_count;
        public byte bone_count;
        public ushort material_count;
        public uint material_offset;

        public uint Bin_flags;
        public uint Tex_count;
        public byte vertex_scale;
        public byte unknown_x29;
        public ushort weightmap2_count;
        public uint morph_offset;

        public uint vertex_position_offset;
        public uint vertex_normal_offset;
        public ushort vertex_position_count;
        public ushort vertex_normal_count;
        public uint version_flags;

        public uint bonepair_offset;
        public uint adjacent_offset;
        public uint unused_x48;
        public uint unused_x4C;
    }

    [Flags]
    public enum BinFlags : uint
    {
        Empty = 0x00_00_00_00,
        EnableModernStyle = 0x80_00_00_00,
        EnableVertexColors = 0x40_00_00_00,
        EnableAltNormals = 0x20_00_00_00,
        EnableAdjacentBoneTag = 0x00_00_02_00,
        EnableBonepairTag = 0x00_00_01_00,
        EnableRe1Flag = 0x00_00_00_01,
    }
}
