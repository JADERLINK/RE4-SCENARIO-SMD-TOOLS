using SHARED_TOOLS.REPACK.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHARED_UHD_BIN_TPL.REPACK.Structures
{
    public class FinalStructure
    {
        public (float vx, float vy, float vz)[] Vertex_Position_Array;
        public (float nx, float ny, float nz)[] Vertex_Normal_Array;
        public (float tu, float tv)[] Vertex_UV_Array;
        public (byte a, byte r, byte g, byte b)[] Vertex_Color_Array;

        public FinalWeightMap[] WeightMaps;
        public ushort[] WeightIndex;

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
        public ushort Type;
        public ushort Count;
    }

}
