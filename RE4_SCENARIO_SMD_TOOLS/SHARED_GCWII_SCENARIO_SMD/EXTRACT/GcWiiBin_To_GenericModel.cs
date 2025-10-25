using SHARED_GCWII_BIN.ALL;
using SHARED_GCWII_BIN.EXTRACT;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_TOOLS.ALL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHARED_GCWII_SCENARIO_SMD.EXTRACT
{
    public static class GcWiiBin_To_GenericModel
    {
        private static float get_scale_from_vertex_scale(byte vertex_scale)
        {
            return (float)Math.Pow(2, vertex_scale);
        }

        public static GenericModelBIN Converter(GCWIIBIN bin) 
        {
            GenericModelBIN gmb = new GenericModelBIN();

            //---- correção para as cores ficar junto com os vertices
            //calculo para inserir as cores no arquivo sem erro. 
            // int vertex_id, HashSet int color_id
            Dictionary<int, HashSet<int>> DicVertexWithColorLists = new Dictionary<int, HashSet<int>>();
            // (int vertex_id, int color_id), int new_vertex_id
            Dictionary<(int vertex_id, int color_id), int> DicNewVextexId = new Dictionary<(int vertex_id, int color_id), int>();
            for (int g = 0; g < bin.Materials.Length; g++)
            {
                for (int i = 0; i < bin.Materials[g].face_index_array.Length; i++)
                {
                    int vextex_id1 = bin.Materials[g].face_index_array[i].i1.indexVertex;
                    int vextex_id2 = bin.Materials[g].face_index_array[i].i2.indexVertex;
                    int vextex_id3 = bin.Materials[g].face_index_array[i].i3.indexVertex;

                    int color_id1 = 0;
                    int color_id2 = 0;
                    int color_id3 = 0;

                    if (bin.Header.ReturnsHasEnableVertexColorsTag())
                    {
                        color_id1 = bin.Materials[g].face_index_array[i].i1.indexColor;
                        color_id2 = bin.Materials[g].face_index_array[i].i2.indexColor;
                        color_id3 = bin.Materials[g].face_index_array[i].i3.indexColor;
                    }

                    if (DicVertexWithColorLists.ContainsKey(vextex_id1))
                    {
                        DicVertexWithColorLists[vextex_id1].Add(color_id1);
                    }
                    else
                    {
                        DicVertexWithColorLists.Add(vextex_id1, new HashSet<int> { color_id1 });
                    }

                    if (DicVertexWithColorLists.ContainsKey(vextex_id2))
                    {
                        DicVertexWithColorLists[vextex_id2].Add(color_id2);
                    }
                    else
                    {
                        DicVertexWithColorLists.Add(vextex_id2, new HashSet<int> { color_id2 });
                    }

                    if (DicVertexWithColorLists.ContainsKey(vextex_id3))
                    {
                        DicVertexWithColorLists[vextex_id3].Add(color_id3);
                    }
                    else
                    {
                        DicVertexWithColorLists.Add(vextex_id3, new HashSet<int> { color_id3 });
                    }
                }
            }

            {
                int new_vertex_id_counter = 0;
                foreach (var item in DicVertexWithColorLists.OrderBy(a => a.Key).ToArray())
                {
                    foreach (var color in item.Value)
                    {
                        DicNewVextexId.Add((item.Key, color), new_vertex_id_counter);
                        new_vertex_id_counter++;
                    }
                }
            }

            //----

            gmb.Vertex_Position_Array = new (float vx, float vy, float vz)[DicNewVextexId.Count];

            if (bin.Header.ReturnsHasEnableVertexColorsTag())
            {
                gmb.Vertex_Color_Array = new (float a, float r, float g, float b)[DicNewVextexId.Count];
            }
            else
            {
                gmb.Vertex_Color_Array = new (float a, float r, float g, float b)[0];
            }

            float extraScale = CONSTs.GLOBAL_POSITION_SCALE * get_scale_from_vertex_scale(bin.Header.vertex_scale);

            foreach (var item in DicNewVextexId)
            {
                float vx = bin.Vertex_Position_Array[item.Key.vertex_id].vx / extraScale;
                float vy = bin.Vertex_Position_Array[item.Key.vertex_id].vy / extraScale;
                float vz = bin.Vertex_Position_Array[item.Key.vertex_id].vz / extraScale;

                gmb.Vertex_Position_Array[item.Value] = (vx, vy, vz);

                if (gmb.Vertex_Color_Array.Length != 0 && bin.Vertex_Color_Array.Length > item.Key.color_id)
                {
                    float r = bin.Vertex_Color_Array[item.Key.color_id].r / 255f;
                    float g = bin.Vertex_Color_Array[item.Key.color_id].g / 255f;
                    float b = bin.Vertex_Color_Array[item.Key.color_id].b / 255f;
                    float a = bin.Vertex_Color_Array[item.Key.color_id].a / 255f;

                    gmb.Vertex_Color_Array[item.Value] = (a, r, g, b);
                }

            }

            gmb.Vertex_Normal_Array = new (float nx, float ny, float nz)[bin.Vertex_Normal_Array.Length];

            for (int i = 0; i < bin.Vertex_Normal_Array.Length; i++)
            {
                float nx = bin.Vertex_Normal_Array[i].nx;
                float ny = bin.Vertex_Normal_Array[i].ny;
                float nz = bin.Vertex_Normal_Array[i].nz;

                float NORMAL_FIX = (float)Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
                NORMAL_FIX = (NORMAL_FIX == 0) ? 1 : NORMAL_FIX;
                nx /= NORMAL_FIX;
                ny /= NORMAL_FIX;
                nz /= NORMAL_FIX;

                gmb.Vertex_Normal_Array[i] = (nx, ny, nz);
            }

            gmb.Vertex_UV_Array = new (float tu, float tv)[bin.Vertex_UV_Array.Length];

            for (int i = 0; i < bin.Vertex_UV_Array.Length; i++)
            {
                float tu;
                float tv;

                if (bin.Header.ReturnHasEnableModernStyle())
                {
                    tu = bin.Vertex_UV_Array[i].tu / (float)byte.MaxValue;
                    tv = ((bin.Vertex_UV_Array[i].tv / (float)byte.MaxValue) - 1) * -1;
                }
                else
                {
                    tu = bin.Vertex_UV_Array[i].tu / (float)short.MaxValue;
                    tv = ((bin.Vertex_UV_Array[i].tv / (float)short.MaxValue) - 1) * -1;
                }

                gmb.Vertex_UV_Array[i] = (tu, tv);
            }

            gmb.Materials = new GenericMaterialBIN[bin.Materials.Length];

            for (int g = 0; g < bin.Materials.Length; g++)
            {
                gmb.Materials[g] = new GenericMaterialBIN();
                gmb.Materials[g].material = bin.Materials[g].material;

                List<(GenericFaceIndex i1, GenericFaceIndex i2, GenericFaceIndex i3)> Faces = new List<(GenericFaceIndex i1, GenericFaceIndex i2, GenericFaceIndex i3)>();

                for (int i = 0; i < bin.Materials[g].face_index_array.Length; i++)
                {
                    int color_id1 = 0;
                    int color_id2 = 0;
                    int color_id3 = 0;

                    if (bin.Header.ReturnsHasEnableVertexColorsTag())
                    {
                        color_id1 = bin.Materials[g].face_index_array[i].i1.indexColor;
                        color_id2 = bin.Materials[g].face_index_array[i].i2.indexColor;
                        color_id3 = bin.Materials[g].face_index_array[i].i3.indexColor;
                    }

                    int avid = DicNewVextexId[(bin.Materials[g].face_index_array[i].i1.indexVertex, color_id1)];
                    int bvid = DicNewVextexId[(bin.Materials[g].face_index_array[i].i2.indexVertex, color_id2)];
                    int cvid = DicNewVextexId[(bin.Materials[g].face_index_array[i].i3.indexVertex, color_id3)];

                    int an = bin.Materials[g].face_index_array[i].i1.indexNormal;
                    int bn = bin.Materials[g].face_index_array[i].i2.indexNormal;
                    int cn = bin.Materials[g].face_index_array[i].i3.indexNormal;

                    int at = bin.Materials[g].face_index_array[i].i1.indexUV;
                    int bt = bin.Materials[g].face_index_array[i].i2.indexUV;
                    int ct = bin.Materials[g].face_index_array[i].i3.indexUV;

                    GenericFaceIndex i1 = new GenericFaceIndex();
                    i1.indexVertex = avid;
                    i1.indexNormal = an;
                    i1.indexUV = at;

                    GenericFaceIndex i2 = new GenericFaceIndex();
                    i2.indexVertex = bvid;
                    i2.indexNormal = bn;
                    i2.indexUV = bt;

                    GenericFaceIndex i3 = new GenericFaceIndex();
                    i3.indexVertex = cvid;
                    i3.indexNormal = cn;
                    i3.indexUV = ct;

                    Faces.Add((i1, i2, i3));
                }

                gmb.Materials[g].face_index_array = Faces.ToArray();

            }

            return gmb;
        }


    }
}
