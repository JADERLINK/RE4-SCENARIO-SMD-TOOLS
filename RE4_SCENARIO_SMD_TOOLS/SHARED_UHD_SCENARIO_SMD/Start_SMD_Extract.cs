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
    public static class Start_SMD_Extract
    {
        public static void SMD_Extract(FileInfo fileInfo1, bool IsPS4NS, Endianness endianness)
        {

            string startGroupName = "UUSCENARIO";

            string baseDirectory = fileInfo1.DirectoryName;
            string baseFileName = Path.GetFileNameWithoutExtension(fileInfo1.Name);

            string baseNameScenario = baseFileName + ".scenario";
            string baseNameBinFolder = baseFileName + "_BIN";
            string baseNameBinFolderRepack = baseFileName + "_REPACK";
            string baseSubDirectory = Path.Combine(baseDirectory, baseNameBinFolder);
            Stream smdfile = fileInfo1.OpenRead();

            SmdMagic smdMagic;
            uint OffsetBinArr = 0;
            uint OffsetTplArr = 0;
            SMDLine[] smdLines = SmdExtract.Extract(smdfile, out smdMagic, out OffsetBinArr, out OffsetTplArr, endianness);

            Extract_BIN_Inside_SMD extract_BIN_Inside_SMD = new Extract_BIN_Inside_SMD();
            EXTRACT.Extract_BIN_Content_UHD extract_BIN_Content = new EXTRACT.Extract_BIN_Content_UHD(endianness, IsPS4NS);
            extract_BIN_Inside_SMD.ToExtractBin = extract_BIN_Content.ToExtractBin;

            EXTRACT.Extract_TPL_Content_UHD extract_TPL_Content = new EXTRACT.Extract_TPL_Content_UHD(endianness, IsPS4NS);
            Extract_TPL_Inside_SMD extract_TPL_Inside_SMD = new EXTRACT.Extract_TPL_Inside_SMD_UHD();
            extract_TPL_Inside_SMD.ToExtractTpl = extract_TPL_Content.ToExtractTpl;

            ToFileMethod_BIN toFileMethod_BIN = new ToFileMethod_BIN(baseSubDirectory, true);
            extract_BIN_Inside_SMD.ToFileBin = toFileMethod_BIN.ToFileBin;

            ToFileMethod_TPL toFileMethod_TPL = new ToFileMethod_TPL(true, baseSubDirectory);
            extract_TPL_Inside_SMD.ToFileTpl = toFileMethod_TPL.ToFileTpl;

            int BinFilesCount = 0;
            int TplFilesCount = 0;
            CounterBinTpl.Calc(smdLines, out BinFilesCount, out TplFilesCount);

            Console.WriteLine("Extracting BIN and TPL files");
            extract_BIN_Inside_SMD.ExtractBINs(smdfile, endianness, OffsetBinArr, BinFilesCount);
            extract_TPL_Inside_SMD.ExtractTPLs(smdfile, endianness, OffsetTplArr, TplFilesCount);

            smdfile.Close();

            //---------------------

            Dictionary<(SHARED_TOOLS.ALL.MaterialPart mat, ushort MagicID), string> materialDic;
            Dictionary<(string MaterialName, ushort MagicID), SHARED_TOOLS.ALL.MaterialPart> materialWithTplFileIdDic;
            var idxMaterial = MaterialParser.IdxMaterialMultiParser(smdLines, extract_BIN_Content.BIN_DIC, out materialDic, out materialWithTplFileIdDic);


            Console.WriteLine("Creating File: " + baseNameScenario + ".obj");
            ScenarioOBJ.CreateOBJ(smdLines, extract_BIN_Content.BIN_DIC, materialDic, baseDirectory, baseNameScenario, startGroupName);


            SHARED_UHD_BIN_TPL.ALL.IdxMtl _idxMtl = new SHARED_UHD_BIN_TPL.ALL.IdxMtl() { MtlDic = new Dictionary<string, SHARED_UHD_BIN_TPL.ALL.MtlObj>() };
            foreach (var item in extract_TPL_Content.UhdTplDic)
            {
                var temp_idxMaterial = new SHARED_TOOLS.ALL.IdxMaterial()
                {
                    MaterialDic = materialWithTplFileIdDic.Where(a => a.Key.MagicID == item.Key).ToDictionary(k => k.Key.MaterialName, v => v.Value)
                };

                var temp_mtl = SHARED_UHD_BIN_TPL.ALL.IdxMtlParser.Parser(temp_idxMaterial, item.Value, IsPS4NS);
                foreach (var mtlObj in temp_mtl.MtlDic)
                {
                    _idxMtl.MtlDic.Add(mtlObj.Key, mtlObj.Value);
                }

                string idxuhdtplFileName = baseNameScenario;
                if (item.Key != 0)
                {
                    idxuhdtplFileName += "." + item.Key.ToString("D");
                }

                Console.WriteLine("Creating File: " + idxuhdtplFileName + ".idxuhdtpl");
                SHARED_UHD_BIN_TPL.EXTRACT.OutputMaterial.CreateIdxUhdTpl(item.Value, baseDirectory, idxuhdtplFileName);
            }
            _idxMtl.MtlDic = _idxMtl.MtlDic.OrderBy(a => a.Key).ToDictionary(K => K.Key, v => v.Value);

            Console.WriteLine("Creating File: " + baseNameScenario + ".idxmaterial");
            SHARED_UHD_BIN_TPL.EXTRACT.OutputMaterial.CreateIdxMaterial(idxMaterial, baseDirectory, baseNameScenario);

            Console.WriteLine("Creating File: " + baseNameScenario + ".mtl");
            SHARED_UHD_BIN_TPL.EXTRACT.OutputMaterial.CreateMTL(_idxMtl, baseDirectory, baseNameScenario);

            Console.WriteLine("Creating File: " + baseNameScenario + ".idxuuscenario");
            string idxScenarioPath = Path.Combine(baseDirectory, baseNameScenario + ".idxuuscenario");
            ScenarioIdx.CreateIdxScenario(idxScenarioPath, smdLines, smdMagic, baseNameBinFolderRepack, fileInfo1.Name);

            Console.WriteLine("Creating File: " + baseNameScenario + ".idxuusmd");
            string idxSmdPath = Path.Combine(baseDirectory, baseNameScenario + ".idxuusmd");
            ScenarioIdx.CreateIdxSmd(idxSmdPath, smdLines, smdMagic, baseNameBinFolder, fileInfo1.Name);
        }

    }
}
