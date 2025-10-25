using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_UHD_SCENARIO_SMD
{
    public static class Start_IdxSMD_Repack
    {
        public static void IdxSMD_Repack(FileInfo fileInfo, Endianness endianness, bool isPS4NS)
        {
            Stream idxFile = fileInfo.OpenRead();
            IdxScenario idxScenario = IdxScenarioLoader.Loader(idxFile);
            MakeSMD_WithBinFolder.CreateSMD(fileInfo.DirectoryName, idxScenario, endianness, false, isPS4NS);
        }
    }
}
