using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_GCWII_SCENARIO_SMD.REPACK
{
    public static class SetTplFileIDInSmdLine
    {
        public static void ToSet(ref SMDLine[] SmdLines, out int tplFilesCount, Dictionary<int, SMDLineIdxPart2> SmdLineIdxPart2Dic) 
        {
            tplFilesCount = 1;

            for (int i = 0; i < SmdLines.Length; i++)
            {
                if (SmdLineIdxPart2Dic.ContainsKey(i) && SmdLines[i].IsNotSharedBIN())
                    // shared BIN não funciona TPL file diferente de 0, não carrega no jogo, sempre carrega o tpl file de id 0
                {
                    SmdLines[i].TplFileID = SmdLineIdxPart2Dic[i].TplFileID;
                }

                if (SmdLines[i].TplFileID >= tplFilesCount)
                {
                    tplFilesCount = SmdLines[i].TplFileID + 1;
                }
            }

        } 

    }
}
