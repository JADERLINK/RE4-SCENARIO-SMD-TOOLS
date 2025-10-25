using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.OutputFiles;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace SHARED_UHD_SCENARIO_SMD
{
    public static class Start_SHD_Extract
    {
        public static void SHD_Extract(FileInfo fileInfo1, bool IsPS4NS, Endianness endianness) 
        {

            string startGroupName = "SHD";

            string baseDirectory = fileInfo1.DirectoryName;
            string baseFileName = Path.GetFileNameWithoutExtension(fileInfo1.Name);

            Stream smdfile = fileInfo1.OpenRead();

            SmdMagic smdMagic;
            uint OffsetBinArr = 0;
            uint OffsetTplArr = 0;
            SMDLine[] smdLines = SmdExtract.Extract(smdfile, out smdMagic, out OffsetBinArr, out OffsetTplArr, endianness);

            Extract_BIN_Inside_SMD extract_BIN_Inside_SMD = new Extract_BIN_Inside_SMD();
            EXTRACT.Extract_BIN_Content_UHD extract_BIN_Content = new EXTRACT.Extract_BIN_Content_UHD(endianness, IsPS4NS);
            extract_BIN_Inside_SMD.ToExtractBin = extract_BIN_Content.ToExtractBin;

            int BinFilesCount = 0;
            int TplFilesCount = 0;
            CounterBinTpl.Calc(smdLines, out BinFilesCount, out TplFilesCount);

            extract_BIN_Inside_SMD.ExtractBINs(smdfile, endianness, OffsetBinArr, BinFilesCount);

            smdfile.Close();

            //------------------------

            Console.WriteLine("Creating File: " + baseFileName + ".obj");
            Dictionary<SHARED_TOOLS.ALL.MaterialPart, string> materialList;
            var idxMaterial = MaterialParser.IdxMaterialMultiParser(extract_BIN_Content.BIN_DIC, out materialList);
            ScenarioOBJ.CreateOBJ(smdLines, extract_BIN_Content.BIN_DIC, materialList.ToDictionary(k => (k.Key, (ushort)0), v => v.Value), baseDirectory, baseFileName, startGroupName);

            Console.WriteLine("Creating File: " + baseFileName + ".idxmaterial");
            SHARED_UHD_BIN_TPL.EXTRACT.OutputMaterial.CreateIdxMaterial(idxMaterial, baseDirectory, baseFileName);

            Console.WriteLine("Creating File: " + baseFileName + ".idxuushd");
            string idxShdPath = Path.Combine(baseDirectory, baseFileName + ".idxuushd");
            ScenarioIdx.CreateIdxShd(idxShdPath, smdLines, smdMagic);
        }
    }
}
