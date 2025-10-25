using SHARED_TOOLS.REPACK.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHARED_GCWII_BIN.REPACK.Structures
{
    public class FinalStructure
    {
        public (short vx, short vy, short vz, ushort WeightIndex)[] Vertex_Position_Array;
        public (short nx, short ny, short nz, ushort WeightIndex)[] Vertex_Normal_Array;
        public (short tu, short tv)[] Vertex_UV_Array;
        public (byte a, byte r, byte g, byte b)[] Vertex_Color_Array;

        public FinalWeightMap[] WeightMaps;
        public FinalMaterialGroup[] Groups;
    }

    public class FinalMaterialGroup
    {
        // nome do material usado
        public string materialName;

        public FinalFace[] Mesh;
    }

    public class FinalFace
    {
        public byte Type;

        public FinalFaceVextexIndex[] indexs;
    }

    public struct FinalFaceVextexIndex
    {
        public ushort indexVertex;
        public ushort indexNormal;
        public ushort indexColor;
        public ushort indexUV;
    }

}
