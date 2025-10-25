using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using SHARED_TOOLS.REPACK.Structures;

namespace SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ
{
    public abstract class ScenarioRepack
    {
        public Dictionary<(int FileId, int SmdID), SmdBaseLine> ObjGroupInfosDic { get; protected set; }
        public Dictionary<(int FileId, int SmdID), SMDLineIdx> SmdLineIdxDic { get; protected set; }

        protected List<(int FileId, int BinID)> LoadedFiles { get; set; }

        public void RepackOBJ(Stream objFile,
            IdxScenario idxScenario,
            string[] ValidStartNameGroup,
            bool LoadColorsFromObjFile = true,
            bool LoadExtraFiles = false
            )
        {
            string GroupPattern = "";

            for (int i = 0; i < ValidStartNameGroup.Length; i++)
            {
                GroupPattern += ValidStartNameGroup[i];
                if (i < ValidStartNameGroup.Length -1)
                {
                    GroupPattern += "|";
                }
            }

            string patternStart = "^(" + GroupPattern + ").*$";
            System.Text.RegularExpressions.Regex regexStart = new System.Text.RegularExpressions.Regex(patternStart, System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            string patternScenario = "^("+ GroupPattern + ")(#SMD_)([0]{0,})([0-9]{1,4})(#SMX_)([0]{0,})([0-9]{1,3})(#TYPE_)([0]{0,})([0-9|A-F]{1,8})(#BIN_)([0]{0,})([0-9]{1,3})(#).*$";
            System.Text.RegularExpressions.Regex regexScenario = new System.Text.RegularExpressions.Regex(patternScenario, System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            string patternStartR100 = "^(FILE_).*$";
            System.Text.RegularExpressions.Regex regexStartR100 = new System.Text.RegularExpressions.Regex(patternStartR100, System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            string patternR100 = "^(FILE_)([0]{0,})([0-9]{1,2})(#SMD_)([0]{0,})([0-9]{1,4})(#SMX_)([0]{0,})([0-9]{1,3})(#TYPE_)([0]{0,})([0-9|A-F]{1,8})(#BIN_)([0]{0,})([0-9]{1,3})(#).*$";
            System.Text.RegularExpressions.Regex regexR100 = new System.Text.RegularExpressions.Regex(patternR100, System.Text.RegularExpressions.RegexOptions.CultureInvariant);

            // load .obj file
            var objLoaderFactory = new ObjLoader.Loader.Loaders.ObjLoaderFactory();
            var objLoader = objLoaderFactory.Create();
            StreamReader streamReader = null;
            ObjLoader.Loader.Loaders.LoadResult arqObj = null;

            try
            {
                streamReader = new StreamReader(objFile, Encoding.ASCII);
                arqObj = objLoader.Load(streamReader);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                streamReader?.Close();
            }

            //lista de materiais usados no modelo
            HashSet<string> ModelMaterialsToUpper = new HashSet<string>();

            FinalWeightMap weightMap = new FinalWeightMap(1, 0, 100, 0, 0, 0, 0);

            //conjunto de struturas
            //id do arquivo, id do SMD/ conteudo para o SMD/BIN
            Dictionary<(int FileId, int SmdID), StartStructure> ObjList = new Dictionary<(int FileId, int SmdID), StartStructure>();
            //FileId
            // -1 Main
            // -2 shared
            // int.MinValue invalid file
            // positive values extra files
            ObjGroupInfosDic = new Dictionary<(int FileId, int SmdID), SmdBaseLine>();

            for (int iG = 0; iG < arqObj.Groups.Count; iG++)
            {
                string GroupName = arqObj.Groups[iG].GroupName.ToUpperInvariant().Trim();
                string materialNameInvariant = arqObj.Groups[iG].MaterialName.ToUpperInvariant().Trim();

                //FIX NAME
                GroupName = GroupName.Replace("_", "#")
                    .Replace("FILE#", "FILE_")
                    .Replace("SMD#", "SMD_")
                    .Replace("SMX#", "SMX_")
                    .Replace("TYPE#", "TYPE_")
                    .Replace("BIN#", "BIN_")
                    ;

                if (regexStart.IsMatch(GroupName) || (regexStartR100.IsMatch(GroupName) && LoadExtraFiles))
                {
                    SmdBaseLine info = null;

                    //REGEX
                    if (regexScenario.IsMatch(GroupName))
                    {
                        Console.WriteLine("Loading in Obj: " + GroupName + " | " + materialNameInvariant);
                        info = getGroupInfo(GroupName);
                    }
                    else if (regexR100.IsMatch(GroupName))
                    {
                        Console.WriteLine("Loading in Obj: " + GroupName + " | " + materialNameInvariant);
                        info = getGroupInfoR100(GroupName);
                    }
                    else
                    {
                        Console.WriteLine("Loading in Obj: " + GroupName + " | " + materialNameInvariant + "  The group name is wrong, group not used;");
                        continue;
                    }

                    var key = (info.FileId, info.SmdId);
                    if (!ObjGroupInfosDic.ContainsKey(key) && info.FileId < LimitConsts.ExtraSmdFileLimit && info.SmdId < LimitConsts.SmdLineLimit)
                    {
                        ObjGroupInfosDic.Add(key, info);
                    }

                    List<List<StartVertex>> facesList = new List<List<StartVertex>>();

                    for (int iF = 0; iF < arqObj.Groups[iG].Faces.Count; iF++)
                    {
                        List<StartVertex> verticeListInObjFace = new List<StartVertex>();

                        for (int iI = 0; iI < arqObj.Groups[iG].Faces[iF].Count; iI++)
                        {
                            StartVertex vertice = new StartVertex();

                            if (arqObj.Groups[iG].Faces[iF][iI].VertexIndex <= 0 || arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1 >= arqObj.Vertices.Count)
                            {
                                throw new ApplicationException("Vertex Position Index is invalid! Value: " + arqObj.Groups[iG].Faces[iF][iI].VertexIndex);
                            }

                            Vector3 position = new Vector3(
                                arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].X,
                                arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].Y,
                                arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].Z
                                );

                            vertice.Position = position;


                            if (arqObj.Groups[iG].Faces[iF][iI].TextureIndex <= 0 || arqObj.Groups[iG].Faces[iF][iI].TextureIndex - 1 >= arqObj.Textures.Count)
                            {
                                vertice.Texture = new Vector2(0, 0);
                            }
                            else
                            {
                                Vector2 texture = new Vector2(
                                arqObj.Textures[arqObj.Groups[iG].Faces[iF][iI].TextureIndex - 1].U,
                                ((arqObj.Textures[arqObj.Groups[iG].Faces[iF][iI].TextureIndex - 1].V - 1) * -1)
                                );

                                vertice.Texture = texture;
                            }


                            if (arqObj.Groups[iG].Faces[iF][iI].NormalIndex <= 0 || arqObj.Groups[iG].Faces[iF][iI].NormalIndex - 1 >= arqObj.Normals.Count)
                            {
                                vertice.Normal = new Vector3(0, 0, 0);
                            }
                            else
                            {
                                float nx = arqObj.Normals[arqObj.Groups[iG].Faces[iF][iI].NormalIndex - 1].X;
                                float ny = arqObj.Normals[arqObj.Groups[iG].Faces[iF][iI].NormalIndex - 1].Y;
                                float nz = arqObj.Normals[arqObj.Groups[iG].Faces[iF][iI].NormalIndex - 1].Z;
                                float NORMAL_FIX = (float)Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
                                NORMAL_FIX = (NORMAL_FIX == 0) ? 1 : NORMAL_FIX;
                                nx /= NORMAL_FIX;
                                ny /= NORMAL_FIX;
                                nz /= NORMAL_FIX;

                                vertice.Normal = new Vector3(nx, ny, nz);
                            }

                            VColor color = new VColor(255, 255, 255, 255);
                            if (LoadColorsFromObjFile)
                            {
                                color = new VColor(
                                (byte)(arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].R * 255),
                                (byte)(arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].G * 255),
                                (byte)(arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].B * 255),
                                (byte)(arqObj.Vertices[arqObj.Groups[iG].Faces[iF][iI].VertexIndex - 1].A * 255)
                                );
                            }

                            vertice.Color = color;
                            vertice.WeightMap = weightMap;

                            verticeListInObjFace.Add(vertice);

                        }

                        if (verticeListInObjFace.Count >= 3)
                        {
                            for (int i = 2; i < verticeListInObjFace.Count; i++)
                            {
                                List<StartVertex> face = new List<StartVertex>();
                                face.Add(verticeListInObjFace[0]);
                                face.Add(verticeListInObjFace[i - 1]);
                                face.Add(verticeListInObjFace[i]);
                                facesList.Add(face);
                            }
                        }

                    }


                    if (info.FileId < LimitConsts.ExtraSmdFileLimit && info.SmdId < LimitConsts.SmdLineLimit)
                    {
                        if (ObjList.ContainsKey(key))
                        {
                            if (ObjList[key].FacesByMaterial.ContainsKey(materialNameInvariant))
                            {
                                ObjList[key].FacesByMaterial[materialNameInvariant].Faces.AddRange(facesList);
                            }
                            else
                            {
                                ModelMaterialsToUpper.Add(materialNameInvariant);
                                ObjList[key].FacesByMaterial.Add(materialNameInvariant, new StartFacesGroup(facesList));
                            }
                        }
                        else
                        {
                            StartStructure startStructure = new StartStructure();
                            ModelMaterialsToUpper.Add(materialNameInvariant);
                            startStructure.FacesByMaterial.Add(materialNameInvariant, new StartFacesGroup(facesList));
                            ObjList.Add(key, startStructure);
                        }

                    }
                    else
                    {
                        Console.WriteLine("This part of the modeling was not added because the FILE_ID or SMD_ID exceed the allowed limit.");
                    }
                }
                else
                {
                    Console.WriteLine("Loading in Obj: " + GroupName + " | " + materialNameInvariant + "   Warning: Group not used;");
                }

            }

            int lastFileId = ObjGroupInfosDic.Keys.DefaultIfEmpty((FileId: int.MinValue, SmdID: int.MinValue)).Max(a => a.FileId);
            for (int f = -1; f <= lastFileId && f < LimitConsts.ExtraSmdFileLimit; f++)
            {
                int lastSmdId = ObjGroupInfosDic.Keys.Where(a => a.FileId == f).DefaultIfEmpty((FileId: int.MinValue, SmdID: int.MinValue)).Max(a => a.SmdID);

                for (int i = 0; i < lastSmdId && i < LimitConsts.SmdLineLimit; i++)
                {
                    if (!ObjGroupInfosDic.ContainsKey((f, i)))
                    {
                        SmdBaseLine smdBaseLine = new SmdBaseLine();
                        smdBaseLine.SmdId = i;
                        smdBaseLine.SmxId = 0xFE;
                        smdBaseLine.Type = 0;
                        smdBaseLine.BinId = 0;
                        ObjGroupInfosDic.Add((f, i), smdBaseLine);
                    }
                }
            }

            //----
            LoadedFiles = new List<(int FileId, int BinID)>();
            SmdLineIdxDic = new Dictionary<(int FileId, int SmdID), SMDLineIdx>();

            ObjList = ObjList.OrderBy(a => a.Key.SmdID).OrderBy(a => a.Key.FileId).ToDictionary(a => a.Key, a => a.Value);

            foreach (var item in ObjList)
            {
                int FileID = item.Key.FileId;
                int BinID = ObjGroupInfosDic[item.Key].BinId;
                bool IsSharedFile = ((ObjGroupInfosDic[item.Key].Type & 0x10) == 0x10) && LoadExtraFiles;
                int FinalFileID = IsSharedFile ? -2 : FileID;

                // FileID
                // -1 main
                // -2 shared
                if (!LoadedFiles.Contains((FinalFileID, BinID)))
                {
                    // faz a compressão das vertives
                    if (FinalFileID > -1)
                    {
                        Console.WriteLine("FILE_ID: " + FileID.ToString("D2") + ", BIN_ID: " + BinID.ToString("D3"));
                    }
                    else if (IsSharedFile)
                    {
                        Console.WriteLine("FILE_ID: SHARED_SMD, BIN_ID: " + BinID.ToString("D3"));
                    }
                    else
                    {
                        Console.WriteLine("FILE_ID: MAIN_SMD, BIN_ID: " + BinID.ToString("D3"));
                    }
                
                    item.Value.CompressAllFaces();
                    //-----

                    SMDLineIdx smdLineIdx = new SMDLineIdx();
                    smdLineIdx.ScaleX = 1f;
                    smdLineIdx.ScaleY = 1f;
                    smdLineIdx.ScaleZ = 1f;

                    if (FileID > -1)
                    {
                        if (idxScenario.ExtraSmdLinesDic.ContainsKey(item.Key))
                        {
                            smdLineIdx = idxScenario.ExtraSmdLinesDic[item.Key].Clone();
                        }
                    }
                    else 
                    {
                        if (idxScenario.SmdLinesDic.ContainsKey(item.Key.SmdID))
                        {
                            smdLineIdx = idxScenario.SmdLinesDic[item.Key.SmdID].Clone();
                        }
                    }

                    ProcessStructure(item.Value, smdLineIdx, FinalFileID, BinID, item.Key);
                    SmdLineIdxDic.Add(item.Key, smdLineIdx);
                    LoadedFiles.Add((FinalFileID, BinID));

                    if (IsSharedFile)
                    {
                        SmdLineIdxDic.Add((-2, BinID), smdLineIdx);
                    }
                }
            }

            // adiciona SmdLineIdx faltantes

            for (int f = -1; f <= lastFileId && f < LimitConsts.ExtraSmdFileLimit; f++)
            {
                int lastSmdId = ObjGroupInfosDic.Keys.Where(a => a.FileId == f).DefaultIfEmpty((FileId: int.MinValue, SmdID: int.MinValue)).Max(a => a.SmdID);

                for (int i = 0; i < lastSmdId && i < LimitConsts.SmdLineLimit; i++)
                {
                    if (!SmdLineIdxDic.ContainsKey((f, i)))
                    {
                        bool has = false;
                        if (f > -1)
                        {
                            if (idxScenario.ExtraSmdLinesDic.ContainsKey((f, i)))
                            {
                                SmdLineIdxDic.Add((f, i), idxScenario.ExtraSmdLinesDic[(f, i)].Clone());
                                has = true;
                            }
                        }
                        else
                        {
                            if (idxScenario.SmdLinesDic.ContainsKey(i))
                            {
                                SmdLineIdxDic.Add((f, i), idxScenario.SmdLinesDic[i].Clone());
                                has = true;
                            }
                        }

                        if (has == false)
                        {
                            SMDLineIdx smdLineIdx = new SMDLineIdx();
                            smdLineIdx.ScaleX = 1f;
                            smdLineIdx.ScaleY = 1f;
                            smdLineIdx.ScaleZ = 1f;
                            SmdLineIdxDic.Add((f, i), smdLineIdx);
                        }
                    }
                }
            }

            // adiciona ObjGroupInfosDic para os bin shared
            int sharedBinCount = SmdLineIdxDic.Keys.Where(a => a.FileId == -2).DefaultIfEmpty((FileId: int.MinValue, SmdID: int.MinValue)).Max(a => a.SmdID) + 1;
            for (int i = 0; i < sharedBinCount; i++)
            {
                SmdBaseLine smdBaseLine = new SmdBaseLine();
                smdBaseLine.SmdId = i;
                smdBaseLine.SmxId = 0xFE;
                smdBaseLine.Type = 0;
                smdBaseLine.BinId = i;
                ObjGroupInfosDic.Add((-2, i), smdBaseLine);
            }
        }

        protected abstract void ProcessStructure(StartStructure startStructure, SMDLineIdx smdLineIdx, int FinalFileID, int BinId, (int FileId, int SmdID) key);

        private static SmdBaseLine getGroupInfo(string GroupName)
        {
            SmdBaseLine line = new SmdBaseLine();
            line.FileId = - 1;

            var split = GroupName.Split('#');

            try
            {
                var subSplit = split[1].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.SmdId = id;
            }
            catch (Exception)
            {
            }

            try
            {
                var subSplit = split[2].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.SmxId = id;
            }
            catch (Exception)
            {
                line.SmxId = 0xFE;
            }

            try
            {
                var subSplit = split[3].Split('_');
                uint type = uint.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                line.Type = type;
            }
            catch (Exception)
            {
            }

            try
            {
                var subSplit = split[4].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.BinId = id;
            }
            catch (Exception)
            {
            }

            return line;
        }

        private static SmdBaseLine getGroupInfoR100(string GroupName)
        {
            SmdBaseLine line = new SmdBaseLine();

            var split = GroupName.Split('#').Where(v => v.Length != 0).ToArray();

            try
            {
                var subSplit = split[0].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.FileId = id;
            }
            catch (Exception)
            {
                line.FileId = int.MinValue;
            }

            try
            {
                var subSplit = split[1].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.SmdId = id;
            }
            catch (Exception)
            {
            }

            try
            {
                var subSplit = split[2].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.SmxId = id;
            }
            catch (Exception)
            {
                line.SmxId = 0xFE;
            }

            try
            {
                var subSplit = split[3].Split('_');
                uint type = uint.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                line.Type = type;
            }
            catch (Exception)
            {
            }

            try
            {
                var subSplit = split[4].Split('_');
                int id = int.Parse(subSplit[1].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                line.BinId = id;
            }
            catch (Exception)
            {
            }

            return line;
        }

    }
}
