using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_TOOLS.REPACK.Structures;
using SHARED_UHD_BIN_TPL.REPACK.Structures;

namespace SHARED_UHD_BIN_TPL.REPACK
{
    public static partial class BinRepack
    {
        public static FinalStructure MakeFinalStructure(IntermediaryLevel2 intermediaryStructure) 
        {
            FinalStructure final = new FinalStructure();

            List<(float vx, float vy, float vz)> Vertex_Position_Array = new List<(float vx, float vy, float vz)>();
            List<(float nx, float ny, float nz)> Vertex_Normal_Array = new List<(float nx, float ny, float nz)>();
            List<(float tu, float tv)> Vertex_UV_Array = new List<(float tu, float tv)>();
            List<(byte a, byte r, byte g, byte b)> Vertex_Color_Array = new List<(byte a, byte r, byte g, byte b)>();

            List<FinalWeightMap> WeightMaps = new List<FinalWeightMap>();
            List<ushort> WeightIndex = new List<ushort>();

            List<FinalMaterialGroup> Groups = new List<FinalMaterialGroup>();

            foreach (var item in intermediaryStructure.Groups.OrderBy(a => a.Key).ToArray())
            {
                FinalMaterialGroup group = new FinalMaterialGroup();
                group.materialName = item.Key;

                group.Mesh = new FinalFace[item.Value.Faces.Count];

                for (int i = 0; i < item.Value.Faces.Count; i++)
                {
                    FinalFace face = new FinalFace();
                    face.Count = item.Value.Faces[i].Count;
                    face.Type = item.Value.Faces[i].Type;
                    group.Mesh[i] = face;

                    for (int iv = 0; iv < item.Value.Faces[i].Vertexs.Count; iv++)
                    {
                        var vertex = item.Value.Faces[i].Vertexs[iv];

                        Vertex_Position_Array.Add((vertex.PosX, vertex.PosY, vertex.PosZ));
                        Vertex_Normal_Array.Add((vertex.NormalX, vertex.NormalY, vertex.NormalZ));
                        Vertex_UV_Array.Add((vertex.TextureU, vertex.TextureV));
                        Vertex_Color_Array.Add((vertex.ColorA,vertex.ColorR, vertex.ColorG, vertex.ColorB));

                        FinalWeightMap weightMap = vertex.WeightMap;

                        if (!WeightMaps.Contains(weightMap))
                        {
                            WeightMaps.Add(weightMap);
                        }

                        ushort weightMapIndex = (ushort)WeightMaps.IndexOf(weightMap);
                        WeightIndex.Add(weightMapIndex);
                    }

                }

                Groups.Add(group);
            }
            
            final.Groups = Groups.ToArray();
            final.WeightIndex = WeightIndex.ToArray();
            final.WeightMaps = WeightMaps.ToArray();
            final.Vertex_Color_Array = Vertex_Color_Array.ToArray();
            final.Vertex_Normal_Array = Vertex_Normal_Array.ToArray();
            final.Vertex_Position_Array = Vertex_Position_Array.ToArray();
            final.Vertex_UV_Array = Vertex_UV_Array.ToArray();
            return final;
        }

    }
}
