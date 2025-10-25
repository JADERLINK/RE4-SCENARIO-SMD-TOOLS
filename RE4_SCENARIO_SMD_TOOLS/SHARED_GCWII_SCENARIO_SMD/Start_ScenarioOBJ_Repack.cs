using SHARED_GCWII_SCENARIO_SMD.REPACK;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.OutputFiles;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SHARED_GCWII_SCENARIO_SMD
{
    public static class Start_ScenarioOBJ_Repack
    {
        public static void ScenarioOBJ_Repack(FileInfo fileInfo1, bool IsR100, bool IsSHD)
        {
            Stream idxStream = fileInfo1.OpenRead();
            IdxScenario idxScenario = IdxScenarioLoader.Loader(idxStream);

            if (IsR100 == false)
            {
                ValidateMagic.Validate(idxScenario.Magic);
            }

            string baseDirectory = fileInfo1.DirectoryName;
            string baseFileName = Path.GetFileNameWithoutExtension(fileInfo1.Name);

            string objPath = Path.Combine(baseDirectory, baseFileName + ".obj");
            string mtlPath = Path.Combine(baseDirectory, baseFileName + ".mtl");
            string idxmaterialPath = Path.Combine(baseDirectory, baseFileName + ".idxmaterial");

            string TplFileName = idxScenario.TplFileName;
            string TplFilePath = Path.Combine(baseDirectory, TplFileName);

            if (File.Exists(TplFilePath) == false)
            {
                throw new ApplicationException("The TPL file does not exist: " + TplFileName);
            }

            Stream objFile = null;
            Stream mtlFile = null;
            Stream idxmaterialFile = null;


            Action CloseOpenedStreams = () => {
                objFile?.Close();
                mtlFile?.Close();
                idxmaterialFile?.Close();

            };

            #region verifica a existencia dos arquivos
            if (File.Exists(objPath))
            {
                Console.WriteLine("Load File: " + baseFileName + ".obj");
                objFile = new FileInfo(objPath).OpenRead();
            }
            else
            {
                Console.WriteLine("Error: OBJ file not found!");
                CloseOpenedStreams();
                return;
            }

            if (idxScenario.UseIdxMaterial)
            {
                if (File.Exists(idxmaterialPath))
                {
                    Console.WriteLine("Load File: " + baseFileName + ".idxmaterial");
                    idxmaterialFile = new FileInfo(idxmaterialPath).OpenRead();
                }
                else
                {
                    Console.WriteLine("Error: IDXMATERIAL file not found!");
                    CloseOpenedStreams();
                    return;
                }

            }
            else
            {
                if (File.Exists(mtlPath))
                {
                    Console.WriteLine("Load File: " + baseFileName + ".mtl");
                    mtlFile = new FileInfo(mtlPath).OpenRead();
                }
                else
                {
                    Console.WriteLine("Error: mtl file not found!");
                    CloseOpenedStreams();
                    return;
                }

            }
            #endregion

            // carrega os materiais

            SHARED_TOOLS.ALL.IdxMaterial material = null;
            SHARED_GCWII_BIN.ALL.IdxMtl idxMtl = null;

            if (idxmaterialFile != null)
            {
                Console.WriteLine("Processing IDXMATERIAL");
                material = SHARED_TOOLS.ALL.IdxMaterialLoad.Load(idxmaterialFile);
                idxmaterialFile.Close();
            }

            if (mtlFile != null) // .MTL
            {
                Console.WriteLine("Processing MTL");
                SHARED_GCWII_BIN.REPACK.MtlLoad.Load(mtlFile, out idxMtl);
                mtlFile.Close();
            }

            if (idxMtl != null)
            {
                Console.WriteLine("Converting MTL");
                SHARED_GCWII_BIN.REPACK.MtlConverter.Convert(idxMtl, out material);
            }


            //-------------------------

            bool loadExtraFiles = IsR100 ? true : false;
            string[] validGroups = new string[] { "UHDSCENARIO", "GGSCENARIO", "UUSCENARIO", "MAINSCENARIO" };
            if (IsSHD)
            {
                validGroups = new string[] { "SHD" };
            }

            Console.WriteLine("Reading and converting OBJ");
            ScenarioRepackGCWII scenarioRepack = new ScenarioRepackGCWII();
            scenarioRepack.RepackOBJ(objFile, idxScenario, validGroups, idxScenario.EnableVertexColor || idxScenario.EnableDinamicVertexColor, loadExtraFiles);

            //mainScenario
            var ObjGroupInfosDic = scenarioRepack.ObjGroupInfosDic.Where(a => a.Key.FileId == -1).ToDictionary(a => a.Key.SmdID, a => a.Value);
            var SmdLineIdxDic = scenarioRepack.SmdLineIdxDic.Where(a => a.Key.FileId == -1).ToDictionary(a => a.Key.SmdID, a => a.Value);

            int smdLinesCount = ObjGroupInfosDic.Any() ? ObjGroupInfosDic.Max(a => a.Key) + 1 : 0;
            int binFilesCount = 0;
            int tplFilesCount = 1;
            int sharedBinFilesCount = 0;

            SMDLine[] SmdLines = SmdLineParcer.Parser(smdLinesCount, SmdLineIdxDic, ObjGroupInfosDic, out binFilesCount, ref sharedBinFilesCount);
            SetTplFileIDInSmdLine.ToSet(ref SmdLines, out tplFilesCount, idxScenario.SmdLinesPart2Dic);

            Console.WriteLine("MAIN FILE INFO:");
            Console.WriteLine("SMD Entry Count: " + smdLinesCount);
            Console.WriteLine("BIN Files Count: " + binFilesCount);
            Console.WriteLine("TPL Files Count: " + tplFilesCount);

            SmdMagic smdMagic = new SmdMagic();
            smdMagic.magic = idxScenario.Magic;

            if (IsR100) // coloca o extra
            {
                smdMagic.magic = 0x0140;
                int extraCount = scenarioRepack.ObjGroupInfosDic.Any() ? scenarioRepack.ObjGroupInfosDic.Max(a => a.Key.FileId) + 1 : 0;
                smdMagic.extraParameters = new uint[extraCount];
                for (int i = 0; i < extraCount; i++)
                {
                    uint value = (uint)(scenarioRepack.ObjGroupInfosDic.Keys.Where(a => a.FileId == i).DefaultIfEmpty((FileId: int.MinValue, SmdID: int.MinValue)).Max(a => a.SmdID) + 1);
                    value = value > 0 ? value : 0; // a resposta do anterior pode vir um número negativo se não tiver nada.
                    smdMagic.extraParameters[i] = value;
                }
            }

            string mainSmdName = idxScenario.SmdFileName;
            if (IsSHD)
            {
                mainSmdName = baseFileName + ".SHD";
            }
            Console.WriteLine("Creating file: " + mainSmdName);

            long BinAreaOffset;
            string smdFilePath = Path.GetFullPath(Path.Combine(baseDirectory, mainSmdName));
            EndianBinaryWriter bw = new EndianBinaryWriter(new FileInfo(smdFilePath).Create(), Endianness.BigEndian);
            MakeSMD_Top.FillTopSmd(bw, Endianness.BigEndian, smdMagic, SmdLines, 0, out BinAreaOffset);

            var finalBinDic = scenarioRepack.FinalBinDic.Where(a => a.Key.FileId == -1).ToDictionary(a => a.Key.BinID, a => a.Value);
            var vertex_scale_Dic = scenarioRepack.vertex_scale_Dic.Where(a => a.Key.FileId == -1).ToDictionary(a => a.Key.BinID, a => a.Value);

            bool CreateBinOrTplFilesInFolder = IsSHD ? false : true;
            string mainBinFolder = idxScenario.BinFolder;
            if (IsR100)
            {
                mainBinFolder = baseFileName + ".FILE_MAIN";
            }
            string binFolderPath = Path.Combine(baseDirectory, mainBinFolder);

            MakeSMD_Fill_BIN_GCWII makeSMD_Fill_BIN = new MakeSMD_Fill_BIN_GCWII(finalBinDic, vertex_scale_Dic, material,
                idxScenario.EnableVertexColor, idxScenario.EnableDinamicVertexColor, 
                CreateBinOrTplFilesInFolder, binFolderPath);

            long TplAreaOffset;
            makeSMD_Fill_BIN.Fill(bw, binFilesCount, (uint)BinAreaOffset, out TplAreaOffset);

            MakeSMD_Fill_TPL makeSMD_Fill_TPL = new MakeSMD_Fill_TPL_GCWII(baseDirectory, TplFileName);

            bool IgnoreFirstTplFile = false;
            if (IsR100)
            {
                makeSMD_Fill_TPL = new MakeSMD_Fill_NO_FIRST_TPL_GCWII(baseDirectory, TplFileName);
                IgnoreFirstTplFile = true;
            }

            long endFileOffset;
            makeSMD_Fill_TPL.Fill(bw, tplFilesCount, (uint)TplAreaOffset, out endFileOffset);

            //coloca os offsets no topo
            bw.Position = 4;
            bw.Write((uint)BinAreaOffset);
            bw.Write((uint)TplAreaOffset);
            bw.Write((uint)endFileOffset);

            bw.Close();

            if (IsSHD == false)
            {
                Console.WriteLine("Creating file: " + baseFileName + ".Repack.idxggsmd");
                string idxSmdPath = Path.Combine(baseDirectory, baseFileName + ".Repack.idxggsmd");
                ScenarioIdx.CreateIdxSmd(idxSmdPath, SmdLines, smdMagic, mainBinFolder, idxScenario.SmdFileName, true, TplFileName, IgnoreFirstTplFile);
            }

            //GSWII
            if (IsR100) // outros arquivos do R100
            {
                int extraCount = scenarioRepack.ObjGroupInfosDic.Keys.DefaultIfEmpty((FileId: int.MinValue, SmdID: int.MinValue)).Max(a => a.FileId) + 1;

                for (int i = 0; i < extraCount; i++)
                {
                    //ExtraFilesScenario
                    var EX_ObjGroupInfosDic = scenarioRepack.ObjGroupInfosDic.Where(a => a.Key.FileId == i).ToDictionary(a => a.Key.SmdID, a => a.Value);
                    var EX_SmdLineIdxDic = scenarioRepack.SmdLineIdxDic.Where(a => a.Key.FileId == i).ToDictionary(a => a.Key.SmdID, a => a.Value);

                    int EX_smdLinesCount = EX_ObjGroupInfosDic.Any() ? EX_ObjGroupInfosDic.Max(a => a.Key) + 1 : 0;
                    int EX_binFilesCount = 0;
                    int EX_tplFilesCount = 1; // os TplFileId diferentes de zero vão aqui.

                    SMDLine[] EX_SmdLines = SmdLineParcer.Parser(EX_smdLinesCount, EX_SmdLineIdxDic, EX_ObjGroupInfosDic, out EX_binFilesCount, ref sharedBinFilesCount);
                    SetTplFileIDInSmdLine.ToSet(ref EX_SmdLines, out EX_tplFilesCount, idxScenario.ExtraSmdLinesPart2Dic.Where(a => a.Key.fileID == i).ToDictionary(k => k.Key.smdID, v => v.Value));

                    Console.WriteLine($"FILE {i:D2} INFO:");
                    Console.WriteLine("SMD Entry Count: " + EX_smdLinesCount);
                    Console.WriteLine("BIN Files Count: " + EX_binFilesCount);
                    Console.WriteLine("TPL Files Count: " + EX_tplFilesCount);

                    SmdMagic EX_smdMagic = new SmdMagic();
                    EX_smdMagic.magic = 0x0040;

                    string ExtraSmdFileName = $"FILE_{i}.SMD";
                    string ExtraTplFileName = $"FILE_{i}.TPL";
                    if (idxScenario.ExtraSmdFileNameDic.ContainsKey(i))
                    {
                        ExtraSmdFileName = idxScenario.ExtraSmdFileNameDic[i];
                    }
                    if (idxScenario.ExtraTplFileNameDic.ContainsKey(i))
                    {
                        ExtraTplFileName = idxScenario.ExtraTplFileNameDic[i];
                    }
                    Console.WriteLine("Creating file: " + ExtraSmdFileName);

                    long EX_BinAreaOffset;
                    string EX_smdFilePath = Path.GetFullPath(Path.Combine(baseDirectory, ExtraSmdFileName));
                    EndianBinaryWriter EX_bw = new EndianBinaryWriter(new FileInfo(EX_smdFilePath).Create(), Endianness.BigEndian);
                    MakeSMD_Top.FillTopSmd(EX_bw, Endianness.BigEndian, EX_smdMagic, EX_SmdLines, 0, out EX_BinAreaOffset);

                    var EX_finalBinDic = scenarioRepack.FinalBinDic.Where(a => a.Key.FileId == i).ToDictionary(a => a.Key.BinID, a => a.Value);
                    var EX_vertex_scale_Dic = scenarioRepack.vertex_scale_Dic.Where(a => a.Key.FileId == i).ToDictionary(a => a.Key.BinID, a => a.Value);

                    string EX_binFolderPath = Path.Combine(baseDirectory, baseFileName + ".FILE_" + i);

                    MakeSMD_Fill_BIN_GCWII EX_makeSMD_Fill_BIN = new MakeSMD_Fill_BIN_GCWII(EX_finalBinDic, EX_vertex_scale_Dic, material,
                    idxScenario.EnableVertexColor, idxScenario.EnableDinamicVertexColor, CreateBinOrTplFilesInFolder, EX_binFolderPath);

                    long EX_TplAreaOffset;
                    EX_makeSMD_Fill_BIN.Fill(EX_bw, EX_binFilesCount, (uint)EX_BinAreaOffset, out EX_TplAreaOffset);

                    MakeSMD_Fill_TPL EX_makeSMD_Fill_TPL = new MakeSMD_Fill_NO_FIRST_TPL_GCWII(baseDirectory, ExtraTplFileName);

                    long EX_endFileOffset;
                    EX_makeSMD_Fill_TPL.Fill(EX_bw, EX_tplFilesCount, (uint)EX_TplAreaOffset, out EX_endFileOffset);

                    //coloca os offsets no topo
                    EX_bw.Position = 4;
                    EX_bw.Write((uint)EX_BinAreaOffset);
                    EX_bw.Write((uint)EX_TplAreaOffset);
                    EX_bw.Write((uint)EX_endFileOffset);

                    EX_bw.Close();

                    Console.WriteLine("Creating file: " + baseFileName + $".Repack.FILE_{i}.idxggsmd");
                    string idxSmdPath = Path.Combine(baseDirectory, baseFileName + $".Repack.FILE_{i}.idxggsmd");
                    ScenarioIdx.CreateIdxSmd(idxSmdPath, EX_SmdLines, EX_smdMagic, baseFileName + ".FILE_" + i, ExtraSmdFileName, true, ExtraTplFileName, true);

                }

                // arquivo Shared smd
                {
                    var SHARED_ObjGroupInfosDic = scenarioRepack.ObjGroupInfosDic.Where(a => a.Key.FileId == -2).ToDictionary(a => a.Key.SmdID, a => a.Value);
                    var SHARED_SmdLineIdxDic = scenarioRepack.SmdLineIdxDic.Where(a => a.Key.FileId == -2).ToDictionary(a => a.Key.SmdID, a => a.Value);

                    int SHARED_smdLinesCount = SHARED_ObjGroupInfosDic.Any() ? SHARED_ObjGroupInfosDic.Max(a => a.Key) + 1 : 0;
                    int SHARED_binFilesCount = sharedBinFilesCount;
                    int SHARED_tplFilesCount = 1; // é sempre um, os outros ficam nos outros SMD's
                    // bin shared sempre são vinculados ao tpl file de id 0, os outros simplemente não carregam.

                    int none = 0;
                    SMDLine[] SHARED_SmdLines = SmdLineParcer.Parser(SHARED_smdLinesCount, SHARED_SmdLineIdxDic, SHARED_ObjGroupInfosDic, out _, ref none);

                    Console.WriteLine($"SHARED FILE INFO:");
                    Console.WriteLine("SMD Entry Count: " + SHARED_smdLinesCount);
                    Console.WriteLine("BIN Files Count: " + SHARED_binFilesCount);
                    Console.WriteLine("TPL Files Count: " + SHARED_tplFilesCount);

                    SmdMagic SHARED_smdMagic = new SmdMagic();
                    SHARED_smdMagic.magic = 0x0040;

                    string SharedSmdFileName = $"FILE_SHARED.SMD";
                    if (idxScenario.SharedFileName != null)
                    {
                        SharedSmdFileName = idxScenario.SharedFileName;
                    }
                    Console.WriteLine("Creating file: " + SharedSmdFileName);

                    long SHARED_BinAreaOffset;
                    string SHARED_smdFilePath = Path.GetFullPath(Path.Combine(baseDirectory, SharedSmdFileName));
                    EndianBinaryWriter SHARED_bw = new EndianBinaryWriter(new FileInfo(SHARED_smdFilePath).Create(), Endianness.BigEndian);
                    MakeSMD_Top.FillTopSmd(SHARED_bw, Endianness.BigEndian, SHARED_smdMagic, SHARED_SmdLines, 0, out SHARED_BinAreaOffset);

                    var SHARED_finalBinDic = scenarioRepack.FinalBinDic.Where(a => a.Key.FileId == -2).ToDictionary(a => a.Key.BinID, a => a.Value);
                    var SHARED_vertex_scale_Dic = scenarioRepack.vertex_scale_Dic.Where(a => a.Key.FileId == -2).ToDictionary(a => a.Key.BinID, a => a.Value);

                    string SHARED_binFolderPath = Path.Combine(baseDirectory, baseFileName + ".FILE_SHARED");

                    MakeSMD_Fill_BIN_GCWII SHARED_makeSMD_Fill_BIN = new MakeSMD_Fill_BIN_GCWII(SHARED_finalBinDic, SHARED_vertex_scale_Dic, material,
                    idxScenario.EnableVertexColor, idxScenario.EnableDinamicVertexColor, 
                    CreateBinOrTplFilesInFolder, SHARED_binFolderPath);

                    long SHARED_TplAreaOffset;
                    SHARED_makeSMD_Fill_BIN.Fill(SHARED_bw, SHARED_binFilesCount, (uint)SHARED_BinAreaOffset, out SHARED_TplAreaOffset);

                    MakeSMD_Fill_TPL SHARED_makeSMD_Fill_TPL = new MakeSMD_Fill_TPL_GCWII(baseDirectory, TplFileName);

                    long SHARED_endFileOffset;
                    SHARED_makeSMD_Fill_TPL.Fill(SHARED_bw, SHARED_tplFilesCount, (uint)SHARED_TplAreaOffset, out SHARED_endFileOffset);

                    //coloca os offsets no topo
                    SHARED_bw.Position = 4;
                    SHARED_bw.Write((uint)SHARED_BinAreaOffset);
                    SHARED_bw.Write((uint)SHARED_TplAreaOffset);
                    SHARED_bw.Write((uint)SHARED_endFileOffset);

                    SHARED_bw.Close();

                    Console.WriteLine("Creating file: " + baseFileName + $".Repack.FILE_SHARED.idxggsmd");
                    string idxSmdPath = Path.Combine(baseDirectory, baseFileName + $".Repack.FILE_SHARED.idxggsmd");
                    ScenarioIdx.CreateIdxSmd(idxSmdPath, SHARED_SmdLines, SHARED_smdMagic, baseFileName + ".FILE_SHARED", SharedSmdFileName, true, TplFileName);
                }

            }

        }
    }
}

