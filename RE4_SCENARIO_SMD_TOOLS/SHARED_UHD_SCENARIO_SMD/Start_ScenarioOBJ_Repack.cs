using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SHARED_UHD_SCENARIO_SMD.REPACK;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.OutputFiles;


namespace SHARED_UHD_SCENARIO_SMD
{
    public static class Start_ScenarioOBJ_Repack
    {
        public static void ScenarioOBJ_Repack(FileInfo fileInfo1, Endianness endianness, bool isPS4NS, bool IsR100, bool IsSHD)
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
            string idxuhdtplPath = Path.Combine(baseDirectory, baseFileName + ".idxuhdtpl");

            Stream objFile = null;
            Stream mtlFile = null;
            Stream idxmaterialFile = null;
            Stream idxuhdtplFile = null;

            Action CloseOpenedStreams = () => {
                objFile?.Close();
                mtlFile?.Close();
                idxmaterialFile?.Close();
                idxuhdtplFile?.Close();
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

            if (idxScenario.UseIdxMaterial || IsSHD)
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

                if (IsSHD == false)
                {
                    if (File.Exists(idxuhdtplPath))
                    {
                        Console.WriteLine("Load File: " + baseFileName + ".idxuhdtpl");
                        idxuhdtplFile = new FileInfo(idxuhdtplPath).OpenRead();
                    }
                    else
                    {
                        Console.WriteLine("Error: IDXUHDTPL file not found. This file is required when using IDXMATERIAL!");
                        CloseOpenedStreams();
                        return;
                    }
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

                if (idxScenario.UseIdxUhdTpl && File.Exists(idxuhdtplPath))
                {
                    Console.WriteLine("Load File: " + baseFileName + ".idxuhdtpl");
                    idxuhdtplFile = new FileInfo(idxuhdtplPath).OpenRead();
                }

            }
            #endregion

            // carrega os materiais

            SHARED_UHD_BIN_TPL.EXTRACT.UhdTPL uhdTPL = null;
            SHARED_TOOLS.ALL.IdxMaterial material = null;
            SHARED_UHD_BIN_TPL.ALL.IdxMtl idxMtl = null;

            if (idxuhdtplFile != null) // .IDXUHDTPL
            {
                Console.WriteLine("Processing IDXUHDTPL");
                uhdTPL = SHARED_UHD_BIN_TPL.ALL.IdxUhdTplLoad.Load(idxuhdtplFile);
                idxuhdtplFile.Close();
            }

            if (idxmaterialFile != null)
            {
                Console.WriteLine("Processing IDXMATERIAL");
                material = SHARED_TOOLS.ALL.IdxMaterialLoad.Load(idxmaterialFile);
                idxmaterialFile.Close();
            }

            if (mtlFile != null) // .MTL
            {
                Console.WriteLine("Processing MTL");
                SHARED_UHD_BIN_TPL.REPACK.MtlLoad.Load(mtlFile, out idxMtl);
                mtlFile.Close();
            }

            if (idxMtl != null)
            {
                Console.WriteLine("Converting MTL");

                new SHARED_UHD_BIN_TPL.REPACK.MtlConverter(baseDirectory).Convert(idxMtl, ref uhdTPL, out material);
                SHARED_UHD_BIN_TPL.EXTRACT.OutputMaterial.CreateIdxUhdTpl(uhdTPL, baseDirectory, baseFileName + ".Repack");
                SHARED_UHD_BIN_TPL.EXTRACT.OutputMaterial.CreateIdxMaterial(material, baseDirectory, baseFileName + ".Repack");
            }


            //-------------------------

            bool loadExtraFiles = IsR100 ? true : false;
            string[] validGroups = new string[] { "UHDSCENARIO", "GGSCENARIO", "UUSCENARIO", "MAINSCENARIO" };
            if (IsSHD)
            {
                validGroups = new string[] { "SHD" };
            }

            Console.WriteLine("Reading and converting OBJ");
            ScenarioRepackUHD scenarioRepack = new ScenarioRepackUHD();
            scenarioRepack.RepackOBJ(objFile, idxScenario, validGroups, idxScenario.EnableVertexColor || idxScenario.EnableDinamicVertexColor, loadExtraFiles);

            //mainScenario
            var ObjGroupInfosDic = scenarioRepack.ObjGroupInfosDic.Where(a => a.Key.FileId == -1).ToDictionary(a => a.Key.SmdID, a => a.Value);
            var SmdLineIdxDic = scenarioRepack.SmdLineIdxDic.Where(a => a.Key.FileId == -1).ToDictionary(a => a.Key.SmdID, a => a.Value);

            int smdLinesCount = ObjGroupInfosDic.Any() ? ObjGroupInfosDic.Max(a => a.Key) + 1 : 0;
            int binFilesCount = 0;
            int tplFilesCount = 1; // Para a versão de UHD e cia, uso sempre um TPL, para simplificar minha lógica.
            int sharedBinFilesCount = 0;

            SMDLine[] SmdLines = SmdLineParcer.Parser(smdLinesCount, SmdLineIdxDic, ObjGroupInfosDic, out binFilesCount, ref sharedBinFilesCount);


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
            EndianBinaryWriter bw = new EndianBinaryWriter(new FileInfo(smdFilePath).Create(), endianness);
            MakeSMD_Top.FillTopSmd(bw, endianness, smdMagic, SmdLines, 0, out BinAreaOffset);

            var finalBinDic = scenarioRepack.FinalBinDic.Where(a => a.Key.FileId == -1).ToDictionary(a => a.Key.BinID, a => a.Value);


            bool CreateBinOrTplFilesInFolder = IsSHD ? false : true;
            string mainBinFolder = idxScenario.BinFolder;
            if (IsR100)
            {
                mainBinFolder = baseFileName + ".FILE_MAIN";
            }
            string binFolderPath = Path.Combine(baseDirectory, mainBinFolder);

            MakeSMD_Fill_BIN_UHD makeSMD_Fill_BIN = new MakeSMD_Fill_BIN_UHD(finalBinDic, material, 
                idxScenario.EnableVertexColor, idxScenario.EnableDinamicVertexColor,
                CreateBinOrTplFilesInFolder, binFolderPath, isPS4NS, endianness);

            long TplAreaOffset;
            makeSMD_Fill_BIN.Fill(bw, binFilesCount, (uint)BinAreaOffset, out TplAreaOffset);

            SHARED_UHD_BIN_TPL.EXTRACT.UhdTPL finalUhdTpl = uhdTPL;
            if (IsR100 || IsSHD) // aqui esse TPL é sempre sem entrys
            {
                finalUhdTpl = new SHARED_UHD_BIN_TPL.EXTRACT.UhdTPL();
                finalUhdTpl.TplArray = new SHARED_UHD_BIN_TPL.EXTRACT.TplInfo[0];
            }

            MakeSMD_Fill_TPL_UHD makeSMD_Fill_TPL = new MakeSMD_Fill_TPL_UHD(finalUhdTpl, CreateBinOrTplFilesInFolder, binFolderPath, isPS4NS, endianness);

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
                Console.WriteLine("Creating file: " + baseFileName + ".Repack.idxuusmd");
                string idxSmdPath = Path.Combine(baseDirectory, baseFileName + ".Repack.idxuusmd");
                ScenarioIdx.CreateIdxSmd(idxSmdPath, SmdLines, smdMagic, mainBinFolder, idxScenario.SmdFileName);
            }

            // UHD
            if (IsR100) // outros arquivos do R100
            {
                int extraCount = scenarioRepack.ObjGroupInfosDic.Keys.DefaultIfEmpty((FileId: int.MinValue, SmdID: int.MinValue)).Max(a => a.FileId) + 1;

                for (int i = 0; i < extraCount; i++)
                {
                    //FilesPartsScenario
                    var EX_ObjGroupInfosDic = scenarioRepack.ObjGroupInfosDic.Where(a => a.Key.FileId == i).ToDictionary(a => a.Key.SmdID, a => a.Value);
                    var EX_SmdLineIdxDic = scenarioRepack.SmdLineIdxDic.Where(a => a.Key.FileId == i).ToDictionary(a => a.Key.SmdID, a => a.Value);

                    int EX_smdLinesCount = EX_ObjGroupInfosDic.Any() ? EX_ObjGroupInfosDic.Max(a => a.Key) + 1 : 0;
                    int EX_binFilesCount = 0;
                    int EX_tplFilesCount = 1; // Para a versão de UHD e cia, uso sempre um TPL, para simplificar minha lógica.

                    SMDLine[] EX_SmdLines = SmdLineParcer.Parser(EX_smdLinesCount, EX_SmdLineIdxDic, EX_ObjGroupInfosDic, out EX_binFilesCount, ref sharedBinFilesCount);

                    Console.WriteLine($"FILE {i:D2} INFO:");
                    Console.WriteLine("SMD Entry Count: " + EX_smdLinesCount);
                    Console.WriteLine("BIN Files Count: " + EX_binFilesCount);
                    Console.WriteLine("TPL Files Count: " + EX_tplFilesCount);

                    SmdMagic EX_smdMagic = new SmdMagic();
                    EX_smdMagic.magic = 0x0040;

                    string PartSmdFileName = $"FILE_{i}.SMD";
                    if (idxScenario.ExtraSmdFileNameDic.ContainsKey(i))
                    {
                        PartSmdFileName = idxScenario.ExtraSmdFileNameDic[i];
                    }
                    Console.WriteLine("Creating file: " + PartSmdFileName);

                    long EX_BinAreaOffset;
                    string EX_smdFilePath = Path.GetFullPath(Path.Combine(baseDirectory, PartSmdFileName));
                    EndianBinaryWriter EX_bw = new EndianBinaryWriter(new FileInfo(EX_smdFilePath).Create(), endianness);
                    MakeSMD_Top.FillTopSmd(EX_bw, endianness, EX_smdMagic, EX_SmdLines, 0, out EX_BinAreaOffset);

                    var EX_finalBinDic = scenarioRepack.FinalBinDic.Where(a => a.Key.FileId == i).ToDictionary(a => a.Key.BinID, a => a.Value);

                    string EX_binFolderPath = Path.Combine(baseDirectory, baseFileName + ".FILE_" + i);

                    MakeSMD_Fill_BIN_UHD EX_makeSMD_Fill_BIN = new MakeSMD_Fill_BIN_UHD(EX_finalBinDic, material,
                       idxScenario.EnableVertexColor, idxScenario.EnableDinamicVertexColor,
                       CreateBinOrTplFilesInFolder, EX_binFolderPath, isPS4NS, endianness);

                    long EX_TplAreaOffset;
                    EX_makeSMD_Fill_BIN.Fill(EX_bw, EX_binFilesCount, (uint)EX_BinAreaOffset, out EX_TplAreaOffset);

                    SHARED_UHD_BIN_TPL.EXTRACT.UhdTPL EX_finalUhdTpl = new SHARED_UHD_BIN_TPL.EXTRACT.UhdTPL();
                    EX_finalUhdTpl.TplArray = new SHARED_UHD_BIN_TPL.EXTRACT.TplInfo[0];
      
                    MakeSMD_Fill_TPL_UHD EX_makeSMD_Fill_TPL = new MakeSMD_Fill_TPL_UHD(EX_finalUhdTpl, CreateBinOrTplFilesInFolder, EX_binFolderPath, isPS4NS, endianness);

                    long EX_endFileOffset;
                    EX_makeSMD_Fill_TPL.Fill(EX_bw, EX_tplFilesCount, (uint)EX_TplAreaOffset, out EX_endFileOffset);

                    //coloca os offsets no topo
                    EX_bw.Position = 4;
                    EX_bw.Write((uint)EX_BinAreaOffset);
                    EX_bw.Write((uint)EX_TplAreaOffset);
                    EX_bw.Write((uint)EX_endFileOffset);

                    EX_bw.Close();

                    Console.WriteLine("Creating file: " + baseFileName + $".Repack.FILE_{i}.idxuusmd");
                    string idxSmdPath = Path.Combine(baseDirectory, baseFileName + $".Repack.FILE_{i}.idxuusmd");
                    ScenarioIdx.CreateIdxSmd(idxSmdPath, EX_SmdLines, EX_smdMagic, baseFileName + ".FILE_" + i, PartSmdFileName);
                }

                // arquivo Shared
                {
                    var SHARED_ObjGroupInfosDic = scenarioRepack.ObjGroupInfosDic.Where(a => a.Key.FileId == -2).ToDictionary(a => a.Key.SmdID, a => a.Value);
                    var SHARED_SmdLineIdxDic = scenarioRepack.SmdLineIdxDic.Where(a => a.Key.FileId == -2).ToDictionary(a => a.Key.SmdID, a => a.Value);

                    int SHARED_smdLinesCount = SHARED_ObjGroupInfosDic.Any() ? SHARED_ObjGroupInfosDic.Max(a => a.Key) + 1 : 0;
                    int SHARED_binFilesCount = sharedBinFilesCount;
                    int SHARED_tplFilesCount = 1; // é sempre um, os outros ficam nos outros SMD's

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
                    EndianBinaryWriter SHARED_bw = new EndianBinaryWriter(new FileInfo(SHARED_smdFilePath).Create(), endianness);
                    MakeSMD_Top.FillTopSmd(SHARED_bw, endianness, SHARED_smdMagic, SHARED_SmdLines, 0, out SHARED_BinAreaOffset);

                    var SHARED_finalBinDic = scenarioRepack.FinalBinDic.Where(a => a.Key.FileId == -2).ToDictionary(a => a.Key.BinID, a => a.Value);


                    string SHARED_binFolderPath = Path.Combine(baseDirectory, baseFileName + ".FILE_SHARED");

                    MakeSMD_Fill_BIN_UHD SHARED_makeSMD_Fill_BIN = new MakeSMD_Fill_BIN_UHD(SHARED_finalBinDic, material,
                    idxScenario.EnableVertexColor, idxScenario.EnableDinamicVertexColor,
                    CreateBinOrTplFilesInFolder, SHARED_binFolderPath, isPS4NS, endianness);

                    long SHARED_TplAreaOffset;
                    SHARED_makeSMD_Fill_BIN.Fill(SHARED_bw, SHARED_binFilesCount, (uint)SHARED_BinAreaOffset, out SHARED_TplAreaOffset);

                    SHARED_UHD_BIN_TPL.EXTRACT.UhdTPL SHARED_finalUhdTpl = uhdTPL; // é o shared que tem o TPL real

                    MakeSMD_Fill_TPL_UHD SHARED_makeSMD_Fill_TPL = new MakeSMD_Fill_TPL_UHD(SHARED_finalUhdTpl, CreateBinOrTplFilesInFolder, SHARED_binFolderPath, isPS4NS, endianness);

                    long SHARED_endFileOffset;
                    SHARED_makeSMD_Fill_TPL.Fill(SHARED_bw, SHARED_tplFilesCount, (uint)SHARED_TplAreaOffset, out SHARED_endFileOffset);

                    //coloca os offsets no topo
                    SHARED_bw.Position = 4;
                    SHARED_bw.Write((uint)SHARED_BinAreaOffset);
                    SHARED_bw.Write((uint)SHARED_TplAreaOffset);
                    SHARED_bw.Write((uint)SHARED_endFileOffset);

                    SHARED_bw.Close();

                    Console.WriteLine("Creating file: " + baseFileName + $".Repack.FILE_SHARED.idxuusmd");
                    string idxSmdPath = Path.Combine(baseDirectory, baseFileName + $".Repack.FILE_SHARED.idxuusmd");
                    ScenarioIdx.CreateIdxSmd(idxSmdPath, SHARED_SmdLines, SHARED_smdMagic, baseFileName + ".FILE_SHARED", SharedSmdFileName);
                }

            }

        }
    }
}
