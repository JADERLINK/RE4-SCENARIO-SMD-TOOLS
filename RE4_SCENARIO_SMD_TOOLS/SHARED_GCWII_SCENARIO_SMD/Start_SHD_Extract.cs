using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.OutputFiles;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace SHARED_GCWII_SCENARIO_SMD
{
    public static class Start_SHD_Extract
    {
        public static void SHD_Extract(FileInfo fileInfo1) 
        {
            Endianness endianness = Endianness.BigEndian;
            string startGroupName = "SHD";

            string baseDirectory = fileInfo1.DirectoryName;
            string baseFileName = Path.GetFileNameWithoutExtension(fileInfo1.Name);

            Stream smdfile = fileInfo1.OpenRead();

            SmdMagic smdMagic;
            uint OffsetBinArr = 0;
            uint OffsetTplArr = 0;
            SMDLine[] smdLines = SmdExtract.Extract(smdfile, out smdMagic, out OffsetBinArr, out OffsetTplArr, endianness);

            Extract_BIN_Inside_SMD extract_BIN_Inside_SMD = new Extract_BIN_Inside_SMD();

            EXTRACT.Extract_BIN_Content_GCWII extract_BIN_Content = new EXTRACT.Extract_BIN_Content_GCWII();
            extract_BIN_Inside_SMD.ToExtractBin = extract_BIN_Content.ToExtractBin;

            Extract_TPL_Inside_SMD extract_TPL_Inside_SMD = new EXTRACT.Extract_TPL_Inside_SMD_GCWII();

            ToFileMethod_TPL toFileMethod_TPL = new ToFileMethod_TPL(true, baseDirectory, baseFileName);
            extract_TPL_Inside_SMD.ToFileTpl = toFileMethod_TPL.ToFileTpl;

            int BinFilesCount = 0;
            int TplFilesCount = 0;
            CounterBinTpl.Calc(smdLines, out BinFilesCount, out TplFilesCount);

            extract_BIN_Inside_SMD.ExtractBINs(smdfile, endianness, OffsetBinArr, BinFilesCount);
            extract_TPL_Inside_SMD.ExtractTPLs(smdfile, endianness, OffsetTplArr, TplFilesCount);

            smdfile.Close();

            //----------------------

            Dictionary<(SHARED_TOOLS.ALL.MaterialPart mat, ushort MagicID), string> materialDic;
            Dictionary<(string MaterialName, ushort MagicID), SHARED_TOOLS.ALL.MaterialPart> materialWithTplFileIdDic;
            var idxMaterial = MaterialParser.IdxMaterialMultiParser(smdLines, extract_BIN_Content.BIN_DIC, out materialDic, out materialWithTplFileIdDic);


            Console.WriteLine("Creating File: " + baseFileName + ".obj");
            ScenarioOBJ.CreateOBJ(smdLines, extract_BIN_Content.BIN_DIC, materialDic, baseDirectory, baseFileName, startGroupName);


            string tplFileName = baseFileName + ".TPL";


            var TplFileCount = materialWithTplFileIdDic.Any() ? materialWithTplFileIdDic.Max(a => a.Key.MagicID) + 1 : 1;
            SHARED_GCWII_BIN.ALL.IdxMtl _idxMtl = new SHARED_GCWII_BIN.ALL.IdxMtl() { MtlDic = new Dictionary<string, SHARED_GCWII_BIN.ALL.MtlObj>() };

            for (int i = 0; i < TplFileCount; i++)
            {
                string newTplName = baseFileName;
                if (i != 0)
                {
                    newTplName += "." + i.ToString("D");
                }

                var temp_idxMaterial = new SHARED_TOOLS.ALL.IdxMaterial()
                {
                    MaterialDic = materialWithTplFileIdDic.Where(a => a.Key.MagicID == i).ToDictionary(k => k.Key.MaterialName, v => v.Value)
                };

                var temp_mtl = SHARED_GCWII_BIN.ALL.IdxMtlParser.Parser(temp_idxMaterial, newTplName);
                foreach (var mtlObj in temp_mtl.MtlDic)
                {
                    _idxMtl.MtlDic.Add(mtlObj.Key, mtlObj.Value);
                }
            }


            Console.WriteLine("Creating File: " + baseFileName + ".idxmaterial");
            SHARED_GCWII_BIN.EXTRACT.OutputMaterial.CreateIdxMaterial(idxMaterial, baseDirectory, baseFileName);

            Console.WriteLine("Creating File: " + baseFileName + ".mtl");
            SHARED_GCWII_BIN.EXTRACT.OutputMaterial.CreateMTL(_idxMtl, baseDirectory, baseFileName);

            Console.WriteLine("Creating File: " + baseFileName + ".idxggshd");
            string idxShdPath = Path.Combine(baseDirectory, baseFileName + ".idxggshd");
            ScenarioIdx.CreateIdxShd(idxShdPath, smdLines, smdMagic, true, tplFileName);
        }

    }
}
