using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.OutputFiles;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.R100;
using SHARED_TOOLS.ALL;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SHARED_SCENARIO_SMD
{
    public abstract class Base_R100_Extract
    {
        protected abstract Endianness Get_Endianness();
        protected abstract bool IsGCWii();

        protected abstract string idxr100repack();
        protected abstract string idxscenario();
        protected abstract string idxsmd();

        protected void BaseR100Extract(FileInfo fileInfo1)
        {
            string baseDirectory = fileInfo1.DirectoryName;
            string baseFileName = Path.GetFileNameWithoutExtension(fileInfo1.Name);

            string baseName_allparts = baseFileName + ".allparts";
            string baseName_custom = baseFileName + ".custom";
            string baseName_custom_BinFolderRepack = baseFileName + "_REPACK";
            string baseBinSubDirectory = Path.Combine(baseDirectory, baseFileName);
            string MainTplBaseName = Get_MainTplBaseName(baseFileName); // nome do TPL principal (shared/main)
            string baseMainTplName = MainTplBaseName + ".TPL";

            Stream idxfile = fileInfo1.OpenRead();
            IdxR100Extract idxextract = IdxR100ExtractLoader.Loader(idxfile);
            idxfile.Close();

            string[] SmdFilesName = R100_Extract.ValidateIdxR100Extract(idxextract, baseDirectory);
            int FileAmount = SmdFilesName.Length;
            int mainFileID = SmdFilesName.Length - 1;
            int sharedFileID = SmdFilesName.Length - 2;

            string[] TplFilesFullName = new string[FileAmount];
            string[] TplFilesBaseName = new string[FileAmount];

            SMDLine[][] smdLinesList = new SMDLine[FileAmount][];
            Dictionary<int, GenericModelBIN>[] modelList = new Dictionary<int, GenericModelBIN>[FileAmount];

            R100ToFileMethods offsets = new R100ToFileMethods();

            int commonBinAmount = 0;

            int[] order = new int[FileAmount];
            order[0] = mainFileID; // primeiro na ordem é o main
            order[order.Length - 1] = sharedFileID; // o ultimo na ordem é o shared
            // no meio são os outros
            for (int i = 0; i < FileAmount - 2; i++)
            {
                order[i + 1] = i;
            }

            for (int i = 0; i < FileAmount; i++)
            {
                int fileID = order[i];
                offsets.fileID = fileID;

                string smdpath = Path.Combine(baseDirectory, SmdFilesName[fileID]);
                FileInfo smdfileinfo = new FileInfo(smdpath);
                string FileName = baseFileName + ".FILE_" + fileID;
                if (fileID == mainFileID)
                {
                    FileName = baseFileName + ".FILE_MAIN";
                }
                else if (fileID == sharedFileID)
                {
                    FileName = baseFileName + ".FILE_SHARED";
                }
                string SubDirectory = Path.Combine(baseDirectory, FileName);
                var (SubDirectoryTpl, TplBaseName) = ReturnSubDirectoryTplAndTplBaseName(baseDirectory, FileName);

                string TPLBaseName0 = TplBaseName;
                string TPLBaseNameOthers = TplBaseName;
                
                bool UseAltBaseName = false;
                bool IgnoreFirstTplFile = false;

                string TplName = TplBaseName;

                // definição dos nomes dos tpl, para o gcwii, ja que ficam na mesma pasta, já no uhd ficam em subpastas com o nome tpl;
                if (fileID == sharedFileID)
                {
                    TPLBaseName0 = MainTplBaseName;
                    UseAltBaseName = true;

                    TplName = MainTplBaseName;
                }
                else if (fileID == mainFileID)
                {
                    TPLBaseNameOthers = MainTplBaseName;
                    IgnoreFirstTplFile = true;
                    UseAltBaseName = true;

                    TplName = MainTplBaseName;
                }
                else 
                { 
                    IgnoreFirstTplFile = true; 
                }

                TplFilesBaseName[fileID] = TplName;
                TplName += ".TPL";
                TplFilesFullName[fileID] = TplName;


                Stream smdfile = smdfileinfo.OpenRead();

                SmdMagic smdMagic;
                uint OffsetBinArr = 0;
                uint OffsetTplArr = 0;
                SMDLine[] smdLines = SmdExtract.Extract(smdfile, out smdMagic, out OffsetBinArr, out OffsetTplArr, Get_Endianness());

                ToFileMethod_BIN toFileMethod_BIN = new ToFileMethod_BIN(SubDirectory, true);
                ToFileMethod_TPL toFileMethod_TPL = new ToFileMethod_TPL(true, SubDirectoryTpl, TPLBaseName0, TPLBaseNameOthers, UseAltBaseName);

                Extract_BIN_Inside_SMD extract_BIN_Inside_SMD;
                Extract_BIN_Content extract_BIN_Content;
                Set_Extract_BIN_Inside_SMD(out extract_BIN_Inside_SMD, out extract_BIN_Content);

                Extract_TPL_Inside_SMD extract_TPL_Inside_SMD;
                Set_Extract_TPL_Inside_SMD(out extract_TPL_Inside_SMD, fileID);

                extract_BIN_Inside_SMD.ToFileBin += offsets.ToFileBin;
                extract_TPL_Inside_SMD.ToFileTpl += offsets.ToFileTpl;
                extract_BIN_Inside_SMD.ToFileBin += toFileMethod_BIN.ToFileBin;
                extract_TPL_Inside_SMD.ToFileTpl += toFileMethod_TPL.ToFileTpl;

                int BinFilesCount = 0;
                int TplFilesCount = 0;
                CounterBinTpl.Calc(smdLines, out BinFilesCount, out TplFilesCount);

                if (fileID == sharedFileID)
                {
                    BinFilesCount = commonBinAmount;
                }
                else
                {
                    R100_Extract.CommonBINcheck(ref commonBinAmount, smdLines);
                }

                extract_BIN_Inside_SMD.ExtractBINs(smdfile, Get_Endianness(), OffsetBinArr, BinFilesCount);
                extract_TPL_Inside_SMD.ExtractTPLs(smdfile, Get_Endianness(), OffsetTplArr, TplFilesCount);
                smdfile.Close();

                smdLinesList[fileID] = smdLines;
                modelList[fileID] = extract_BIN_Content.BIN_DIC;

                SetUhdTplDic(fileID, fileID == sharedFileID); //shared.SMD contem o tplFileId de Id 0 (zero)

                //cria idx__smd
                Console.WriteLine("Creating File: " + FileName + idxsmd());
                string idxSmdPath = Path.Combine(baseDirectory, FileName + idxsmd());
                ScenarioIdx.CreateIdxSmd(idxSmdPath, smdLines, smdMagic, FileName, SmdFilesName[fileID], IsGCWii(), TplName, IgnoreFirstTplFile);
            }

            //materials
            Dictionary<(MaterialPart mat, ushort MagicID), string> materialDic;
            Dictionary<(string MaterialName, ushort MagicID), MaterialPart> materialWithTplFileIdDic;
            var idxMaterial = MaterialParser.IdxMaterialMultParser(smdLinesList, modelList, out materialDic, out materialWithTplFileIdDic, order, mainFileID, sharedFileID);
           
            //allparts

            Console.WriteLine("Creating File: " + baseName_allparts + ".obj");
            ScenarioOBJ.R100CreateOBJ(smdLinesList, modelList, materialDic, baseDirectory, baseName_allparts, sharedFileID, mainFileID);

            Console.WriteLine("Creating File: " + baseName_allparts + idxr100repack());
            string idxr100repackPath = Path.Combine(baseDirectory, baseName_allparts + idxr100repack());
            ScenarioIdx.CreateIdxR100Repack(idxr100repackPath, smdLinesList, SmdFilesName, sharedFileID, mainFileID, IsGCWii(), baseMainTplName, TplFilesFullName);

            CreateMtlIdxMaterialIdxuhdTpl(materialWithTplFileIdDic, idxMaterial, baseDirectory, baseName_allparts, TplFilesBaseName, MainTplBaseName);

            //custom

            SmdMagic smdMagicCustom = new SmdMagic();
            SMDLine[] newSmdLines = null;
            Dictionary<int, GenericModelBIN> binList = null;
            var newBinOrder = R100_Extract.ConverterToSingleSMD(smdLinesList, modelList, out newSmdLines, out binList, sharedFileID, mainFileID);

            Console.WriteLine("Creating File: " + baseName_custom + ".obj");
            ScenarioOBJ.CreateOBJ(newSmdLines, binList, materialDic, baseDirectory, baseName_custom, GetStartGroupName());

            Console.WriteLine("Creating File: " + baseName_custom + idxscenario());
            string idxscenarioPath = Path.Combine(baseDirectory, baseName_custom + idxscenario());
            ScenarioIdx.CreateIdxScenario(idxscenarioPath, newSmdLines, smdMagicCustom, baseName_custom_BinFolderRepack, SmdFilesName[mainFileID], IsGCWii(), baseMainTplName);

            Console.WriteLine("Creating File: " + baseName_custom + idxsmd());
            string idxsmdPath = Path.Combine(baseDirectory, baseName_custom + idxsmd());
            ScenarioIdx.CreateIdxSmd(idxsmdPath, newSmdLines, smdMagicCustom, baseFileName, SmdFilesName[mainFileID], IsGCWii(), baseMainTplName);

            CreateMtlIdxMaterialIdxuhdTpl(materialWithTplFileIdDic, idxMaterial, baseDirectory, baseName_custom, TplFilesBaseName, MainTplBaseName);

            //extra os arquivos bin para custom

            for (int i = 0; i < FileAmount; i++)
            {
                string smdpath = Path.Combine(baseDirectory, SmdFilesName[i]);
                FileInfo smdfileinfo = new FileInfo(smdpath);
                Stream smdfile = smdfileinfo.OpenRead();

                ToFileMethod_BIN toFileMethod_BIN = new ToFileMethod_BIN(baseBinSubDirectory, true);
                
                var list = offsets.binOffsetList.Where(w => w.Key.fileID == i).ToList();
                foreach (var item in list)
                {
                    var key = (item.Key.fileID, item.Key.binID);
                    if (newBinOrder.ContainsKey(key))
                    {
                        int newId = newBinOrder[key];
                        toFileMethod_BIN.ToFileBin(smdfile, item.Value.binOffset, item.Value.endOffset, newId);
                    }
                }

                if (i == sharedFileID && IsGCWii() == false)//shared.SMD contem o tpl valido
                {
                    var (SubDirectoryTpl, TplBaseName) = ReturnSubDirectoryTplAndTplBaseName(baseDirectory, baseFileName);
                    ToFileMethod_TPL toFileMethod_TPL = new ToFileMethod_TPL(true, SubDirectoryTpl, TplBaseName);
                    foreach (var item in offsets.tplOffsetList)
                    {
                        var offsetToTpl = offsets.tplOffsetList[item.Key];
                        toFileMethod_TPL.ToFileTpl(smdfile, offsetToTpl.tplOffset, offsetToTpl.endOffset, item.Key.tplID);
                    }

                }

                smdfile.Close();
            }

        }

        protected abstract string Get_MainTplBaseName(string baseFileName);

        protected abstract (string SubDirectoryTpl, string TplName) ReturnSubDirectoryTplAndTplBaseName(string baseDirectory, string FileName);

        protected abstract void Set_Extract_BIN_Inside_SMD(out Extract_BIN_Inside_SMD extract_BIN_Inside_SMD, out Extract_BIN_Content extract_BIN_Content);

        protected abstract void Set_Extract_TPL_Inside_SMD(out Extract_TPL_Inside_SMD extract_TPL_Inside_SMD, int fileID);

        protected abstract void SetUhdTplDic(int inFileID, bool IsSharedFile);

        protected abstract void CreateMtlIdxMaterialIdxuhdTpl(
            Dictionary<(string MaterialName, ushort MagicID), MaterialPart> materialWithTplFileIdDic, 
            IdxMaterial idxMaterial, string baseDirectory, string fileBaseName,
            string[] TplNames, string mainTplFileBaseName);

        protected abstract string GetStartGroupName();
    }
}
