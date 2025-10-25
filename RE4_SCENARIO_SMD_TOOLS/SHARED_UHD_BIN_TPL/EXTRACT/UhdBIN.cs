using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_TOOLS.ALL;

namespace SHARED_UHD_BIN_TPL.EXTRACT
{
    public class UhdBIN
    {
        public UhdBinHeader Header;

        public (float vx, float vy, float vz)[] Vertex_Position_Array;
        public (float nx, float ny, float nz)[] Vertex_Normal_Array;
        public (float tu, float tv)[] Vertex_UV_Array;
        public (byte a, byte r, byte g, byte b)[] Vertex_Color_Array;

        public Bone[] Bones;
        public WeightMap[] WeightMaps;
        public ushort[] WeightIndex;
        public BonePair[] BonePairs;
        public MaterialBin[] Materials;
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

        public (int i1, int i2, int i3)[] face_index_array;
    }

    public class WeightMap
    {
        // correção: é sempre byte
        // o jogo base so aceita so 255 combinações desse objeto(WeightMap)
        // para ter mais que 255 tem que usar a DLL do qingsheng
        // X3DAudio1_7.dll
        // no "X3DAudio1_7.ini" mudar "Allocate more memory for bones" de 0 para 1
        // também pode ser usada a Companion DLL
        // atenção: nos arquivos r10c_27.BIN r22a_27.BIN, os bones são ushort, mas essa logica não funciona para os outros arquivos.
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


    public class UhdBinHeader
    {
        public uint bone_offset; // magic: 60 00 00 00
        public uint unknown_x04; //--zeros
        public uint unknown_x08; // offset: 50 00 00 00 // address is present when adjacent_meshinfo_addr is present {but address goes to blank area}
        public uint vertex_colour_offset; //colors


        public uint vertex_texcoord_offset; // UV
        public uint weightmap_offset;
        public byte weightmap_count;
        public byte bone_count;
        public ushort material_count;
        public uint material_offset;

        public uint Bin_flags;
        public uint Tex_count; // quantidade de id de tpl usados
        public byte vertex_scale;
        public byte unknown_x29;
        public ushort weightmap2_count; //--same as weightcount
        public uint morph_offset;

        public uint vertex_position_offset;
        public uint vertex_normal_offset;
        public ushort vertex_position_count;
        public ushort vertex_normal_count;
        public uint version_flags;

        public uint bonepair_offset;
        public uint adjacent_offset;
        public uint vertex_weight_index_offset;
        public uint vertex_weight2_index_offset;
    }

    [Flags]
    public enum BinFlags : uint
    {
        Empty = 0x00_00_00_00,
        AlwaysActivated = 0x80_00_00_00,
        EnableVertexColors = 0x40_00_00_00,
        EnableAltNormals = 0x20_00_00_00,
        EnableAdjacentBoneTag = 0x00_00_02_00,
        EnableBonepairTag = 0x00_00_01_00,
    }
}
