using SHARED_TOOLS.ALL;
using SHARED_TOOLS.SCENARIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.OutputFiles
{
    public static class ScenarioOBJ
    {
        public static void CreateOBJ(SMDLine[] smdLines, Dictionary<int, GenericModelBIN> BinDic, Dictionary<(MaterialPart mat, ushort MagicID), string> materialDic,
           string baseDirectory, string baseFileName, string startGroupName, bool UseColorsInObjFile = true)
        {
            StreamWriter obj = new StreamWriter(Path.Combine(baseDirectory, baseFileName + ".obj"), false);
            obj.WriteLine(SHARED_TOOLS.Shared.HeaderText());
            obj.WriteLine("");

            obj.WriteLine("mtllib " + baseFileName + ".mtl");

            uint vertexIndexCount = 0;
            uint normalIndexCount = 0;
            uint uvIndexCount = 0;

            for (int smdID = 0; smdID < smdLines.Length; smdID++)
            {
                int key = smdLines[smdID].BinFileID;
                bool IsSharedBin = smdLines[smdID].IsSharedBIN();
                ushort UseMagicId = IsSharedBin ? (byte)0u : smdLines[smdID].TplFileID;
                if (BinDic.ContainsKey(key))
                {
                    SMDLine smdLine = smdLines[smdID];

                    StringBuilder group = new StringBuilder();
                    group.Append("g ").Append(startGroupName)
                        .Append("#SMD_").Append(smdID.ToString("D3"))
                        .Append("#SMX_").Append(smdLine.SmxID.ToString("D3"))
                        .Append("#TYPE_").Append(smdLine.ObjectStatus.ToString("X2"))
                        .Append("#BIN_").Append(smdLine.BinFileID.ToString("D3"))
                        .Append("#");

                    obj.WriteLine(group.ToString());

                    ObjCreatePart(obj, BinDic[key], smdLine, materialDic, UseMagicId, UseColorsInObjFile, ref vertexIndexCount, ref normalIndexCount, ref uvIndexCount);
                }

            }

            obj.Close();
        }

        private static void ObjCreatePart(StreamWriter obj, GenericModelBIN bin, SMDLine smdLine, Dictionary<(MaterialPart mat, ushort MagicID), string> materialDic,
           ushort UseMagicID, bool UseColorsInObjFile, ref uint vertexIndexCount, ref uint normalIndexCount, ref uint uvIndexCount)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bin.Vertex_Position_Array.Length; i++)
            {
                float[] pos = new float[3];// 0 = x, 1 = y, 2 = z
                pos[0] = bin.Vertex_Position_Array[i].vx;
                pos[1] = bin.Vertex_Position_Array[i].vy;
                pos[2] = bin.Vertex_Position_Array[i].vz;

                pos = RotationUtils.RotationInX(pos, smdLine.AngleX);
                pos = RotationUtils.RotationInY(pos, smdLine.AngleY);
                pos = RotationUtils.RotationInZ(pos, smdLine.AngleZ);

                pos[0] = ((pos[0] * smdLine.ScaleX) + (smdLine.PositionX / CONSTs.GLOBAL_POSITION_SCALE));
                pos[1] = ((pos[1] * smdLine.ScaleY) + (smdLine.PositionY / CONSTs.GLOBAL_POSITION_SCALE));
                pos[2] = ((pos[2] * smdLine.ScaleZ) + (smdLine.PositionZ / CONSTs.GLOBAL_POSITION_SCALE));

                sb.Append("v ").Append(pos[0].ToFloatString())
                   .Append(" ").Append(pos[1].ToFloatString())
                   .Append(" ").Append(pos[2].ToFloatString());

                if (UseColorsInObjFile && bin.Vertex_Color_Array.Length > i)
                {
                    float r = bin.Vertex_Color_Array[i].r;
                    float g = bin.Vertex_Color_Array[i].g;
                    float b = bin.Vertex_Color_Array[i].b;
                    float a = bin.Vertex_Color_Array[i].a;

                    sb.Append(" ").Append(r.ToFloatString())
                      .Append(" ").Append(g.ToFloatString())
                      .Append(" ").Append(b.ToFloatString())
                      .Append(" ").Append(a.ToFloatString());

                }
                sb.AppendLine();
            }

            for (int i = 0; i < bin.Vertex_Normal_Array.Length; i++)
            {
                float[] normal = new float[3];// 0 = x, 1 = y, 2 = z
                normal[0] = bin.Vertex_Normal_Array[i].nx;
                normal[1] = bin.Vertex_Normal_Array[i].ny;
                normal[2] = bin.Vertex_Normal_Array[i].nz;

                normal = RotationUtils.RotationInX(normal, smdLine.AngleX);
                normal = RotationUtils.RotationInY(normal, smdLine.AngleY);
                normal = RotationUtils.RotationInZ(normal, smdLine.AngleZ);

                sb.Append("vn ").Append(normal[0].ToFloatString())
                    .Append(" ").Append(normal[1].ToFloatString())
                    .Append(" ").Append(normal[2].ToFloatString())
                    .AppendLine();
            }

            for (int i = 0; i < bin.Vertex_UV_Array.Length; i++)
            {
                float tu = bin.Vertex_UV_Array[i].tu;
                float tv = bin.Vertex_UV_Array[i].tv;
                sb.Append("vt ").Append(tu.ToFloatString()).Append(" ").Append(tv.ToFloatString()).AppendLine();
            }


            for (int g = 0; g < bin.Materials.Length; g++)
            {
                var matKey = (bin.Materials[g].material, UseMagicID);
                string MaterialName = materialDic.ContainsKey(matKey) ? materialDic[matKey] : "UNKNOWN_MATERIAL";

                sb.Append("usemtl ").Append(MaterialName).AppendLine();

                for (int i = 0; i < bin.Materials[g].face_index_array.Length; i++)
                {
                    long av = (bin.Materials[g].face_index_array[i].i1.indexVertex + vertexIndexCount + 1);
                    long bv = (bin.Materials[g].face_index_array[i].i2.indexVertex + vertexIndexCount + 1);
                    long cv = (bin.Materials[g].face_index_array[i].i3.indexVertex + vertexIndexCount + 1);

                    long an = (bin.Materials[g].face_index_array[i].i1.indexNormal + normalIndexCount + 1);
                    long bn = (bin.Materials[g].face_index_array[i].i2.indexNormal + normalIndexCount + 1);
                    long cn = (bin.Materials[g].face_index_array[i].i3.indexNormal + normalIndexCount + 1);

                    long at = (bin.Materials[g].face_index_array[i].i1.indexUV + uvIndexCount + 1);
                    long bt = (bin.Materials[g].face_index_array[i].i2.indexUV + uvIndexCount + 1);
                    long ct = (bin.Materials[g].face_index_array[i].i3.indexUV + uvIndexCount + 1);

                    sb.Append("f ").Append(av).Append('/').Append(at).Append('/').Append(an)
                       .Append(" ").Append(bv).Append('/').Append(bt).Append('/').Append(bn)
                       .Append(" ").Append(cv).Append('/').Append(ct).Append('/').Append(cn)
                       .AppendLine();
                }
            }

            obj.Write(sb.ToString());

            vertexIndexCount += (uint)bin.Vertex_Position_Array.Length;
            normalIndexCount += (uint)bin.Vertex_Normal_Array.Length;
            uvIndexCount += (uint)bin.Vertex_UV_Array.Length;
        }

        public static void R100CreateOBJ(SMDLine[][] smdLinesList, Dictionary<int, GenericModelBIN>[] modelList, 
            Dictionary<(MaterialPart mat, ushort MagicID), string> materialDic,
            string baseDirectory, string baseFileName, int sharedFileId, int mainFileID, bool UseColorsInObjFile = true)
        {
            StreamWriter obj = new StreamWriter(Path.Combine(baseDirectory, baseFileName + ".obj"), false);
            obj.WriteLine(SHARED_TOOLS.Shared.HeaderText());
            obj.WriteLine("");

            obj.WriteLine("mtllib " + baseFileName + ".mtl");

            uint vertexIndexCount = 0;
            uint normalIndexCount = 0;
            uint uvIndexCount = 0;

            int[] order = new int[smdLinesList.Length - 1]; // aqui não tem o shared
            order[0] = mainFileID; // primeiro o main file
            for (int i = 0; i < smdLinesList.Length - 2; i++) // depois os outros
            {
                order[i + 1] = i;
            }

            for (int o = 0; o < order.Length; o++)
            {
                int fileID = order[o];

                string startGroup = "FILE_" + fileID.ToString("D2");
                if (fileID == mainFileID)
                {
                    startGroup = "MAINSCENARIO";
                }

                for (int smdID = 0; smdID < smdLinesList[fileID].Length; smdID++)
                {
                    SMDLine smdLine = smdLinesList[fileID][smdID];

                    int key = smdLine.BinFileID;
                    bool IsSharedBin = smdLine.IsSharedBIN();
                    ushort UseMagicId = IsSharedBin ? (byte)0u : smdLine.TplFileID;

                    if (IsSharedBin == false && fileID != mainFileID) // o shared não é verificado pois não esta nesse for
                    {
                        UseMagicId = (ushort)(((fileID + 1) * 0x01_00) + smdLine.TplFileID);
                        // ushort dividido em duas pares 0xAA_BB
                        // AA : id do arquivo (smdFileID), sendo o id 0 o main/shared, e os proximo é o id do file +1
                        // BB : id do arquivo tpl (tplFileID), para os shared é sempre zero.
                    }

                    StringBuilder group = new StringBuilder();
                    group.Append("g ").Append(startGroup)
                        .Append("#SMD_").Append(smdID.ToString("D3"))
                        .Append("#SMX_").Append(smdLine.SmxID.ToString("D3"))
                        .Append("#TYPE_").Append(smdLine.ObjectStatus.ToString("X2"))
                        .Append("#BIN_").Append(smdLine.BinFileID.ToString("D3"))
                        .Append("#");
                        
                    if (IsSharedBin)
                    {
                        group.Append("CommonBIN#");
                    }

                    obj.WriteLine(group.ToString());

                    if (IsSharedBin)
                    {
                        if (modelList[sharedFileId].ContainsKey(key))
                        {
                            ObjCreatePart(obj, modelList[sharedFileId][key], smdLine, materialDic, UseMagicId, UseColorsInObjFile, ref vertexIndexCount, ref normalIndexCount, ref uvIndexCount);
                        }
                    }
                    else
                    {
                        if (modelList[fileID].ContainsKey(key))
                        {
                            ObjCreatePart(obj, modelList[fileID][key], smdLine, materialDic, UseMagicId, UseColorsInObjFile, ref vertexIndexCount, ref normalIndexCount, ref uvIndexCount);
                        }

                    }
                }
            }

            obj.Close();
        }

    }
}
