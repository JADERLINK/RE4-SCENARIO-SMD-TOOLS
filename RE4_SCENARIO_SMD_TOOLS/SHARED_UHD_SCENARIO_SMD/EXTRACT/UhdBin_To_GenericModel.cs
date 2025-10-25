using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_TOOLS.ALL;
using SHARED_UHD_BIN_TPL.ALL;
using SHARED_UHD_BIN_TPL.EXTRACT;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_UHD_SCENARIO_SMD.EXTRACT
{
    public static class UhdBin_To_GenericModel
    {
        public static GenericModelBIN Converter(UhdBIN uhdbin) 
        {
            GenericModelBIN gmb = new GenericModelBIN();

            gmb.Vertex_Position_Array = new (float vx, float vy, float vz)[uhdbin.Vertex_Position_Array.Length];

            if (uhdbin.Header.ReturnsHasEnableVertexColorsTag())
            {
                gmb.Vertex_Color_Array = new (float a, float r, float g, float b)[uhdbin.Vertex_Position_Array.Length];
            }
            else
            {
                gmb.Vertex_Color_Array = new (float a, float r, float g, float b)[0];
            }

            for (int i = 0; i < uhdbin.Vertex_Position_Array.Length; i++)
            {
                float vx = uhdbin.Vertex_Position_Array[i].vx / CONSTs.GLOBAL_POSITION_SCALE;
                float vy = uhdbin.Vertex_Position_Array[i].vy / CONSTs.GLOBAL_POSITION_SCALE;
                float vz = uhdbin.Vertex_Position_Array[i].vz / CONSTs.GLOBAL_POSITION_SCALE;
                gmb.Vertex_Position_Array[i] = (vx, vy, vz);

                if (gmb.Vertex_Color_Array.Length != 0 && uhdbin.Vertex_Color_Array.Length > i)
                {
                    float r = uhdbin.Vertex_Color_Array[i].r / 255f;
                    float g = uhdbin.Vertex_Color_Array[i].g / 255f;
                    float b = uhdbin.Vertex_Color_Array[i].b / 255f;
                    float a = uhdbin.Vertex_Color_Array[i].a / 255f;
                    gmb.Vertex_Color_Array[i] = (a, r, g, b);
                }
            }

            gmb.Vertex_Normal_Array = new (float nx, float ny, float nz)[uhdbin.Vertex_Normal_Array.Length];

            for (int i = 0; i < uhdbin.Vertex_Normal_Array.Length; i++)
            {
                float nx = uhdbin.Vertex_Normal_Array[i].nx;
                float ny = uhdbin.Vertex_Normal_Array[i].ny;
                float nz = uhdbin.Vertex_Normal_Array[i].nz;

                float NORMAL_FIX = (float)Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
                NORMAL_FIX = (NORMAL_FIX == 0) ? 1 : NORMAL_FIX;
                nx /= NORMAL_FIX;
                ny /= NORMAL_FIX;
                nz /= NORMAL_FIX;

                gmb.Vertex_Normal_Array[i] = (nx, ny, nz);
            }

            gmb.Vertex_UV_Array = new (float tu, float tv)[uhdbin.Vertex_UV_Array.Length];

            for (int i = 0; i < uhdbin.Vertex_UV_Array.Length; i++)
            {
                float tu = uhdbin.Vertex_UV_Array[i].tu;
                float tv = (uhdbin.Vertex_UV_Array[i].tv - 1) * -1;
                gmb.Vertex_UV_Array[i] = (tu, tv);
            }

            gmb.Materials = new GenericMaterialBIN[uhdbin.Materials.Length];

            for (int g = 0; g < uhdbin.Materials.Length; g++)
            {
                gmb.Materials[g] = new GenericMaterialBIN();
                gmb.Materials[g].material = uhdbin.Materials[g].material;

                List<(GenericFaceIndex i1, GenericFaceIndex i2, GenericFaceIndex i3)> Faces = new List<(GenericFaceIndex i1, GenericFaceIndex i2, GenericFaceIndex i3)>();

                for (int i = 0; i < uhdbin.Materials[g].face_index_array.Length; i++)
                {
                    int a = uhdbin.Materials[g].face_index_array[i].i1;
                    int b = uhdbin.Materials[g].face_index_array[i].i2;
                    int c = uhdbin.Materials[g].face_index_array[i].i3;

                    GenericFaceIndex i1 = new GenericFaceIndex();
                    i1.indexVertex = a;
                    i1.indexNormal = a;
                    i1.indexUV = a;

                    GenericFaceIndex i2 = new GenericFaceIndex();
                    i2.indexVertex = b;
                    i2.indexNormal = b;
                    i2.indexUV = b;

                    GenericFaceIndex i3 = new GenericFaceIndex();
                    i3.indexVertex = c;
                    i3.indexNormal = c;
                    i3.indexUV = c;

                    Faces.Add((i1, i2, i3));
                }

                gmb.Materials[g].face_index_array = Faces.ToArray();
            }

            return gmb;
        }


    }
}
