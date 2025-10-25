using SHARED_TOOLS.ALL;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT
{
    public class GenericModelBIN
    {
        public (float vx, float vy, float vz)[] Vertex_Position_Array; // ja escalado
        public (float nx, float ny, float nz)[] Vertex_Normal_Array; // ja normalizado
        public (float tu, float tv)[] Vertex_UV_Array; // so colocar no obj
        public (float a, float r, float g, float b)[] Vertex_Color_Array; // vai ser tamanho 0 se não tiver.

        public GenericMaterialBIN[] Materials; // onde esta o material e as faces;
    }

    public class GenericMaterialBIN
    {
        public MaterialPart material;

        public (GenericFaceIndex i1, GenericFaceIndex i2, GenericFaceIndex i3)[] face_index_array;
    }

    public struct GenericFaceIndex
    {
        public int indexVertex;
        public int indexNormal;
        public int indexUV;
    }
}
