using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ;
using SHARED_TOOLS.REPACK.Structures;
using SHARED_UHD_BIN_TPL.REPACK;
using SHARED_UHD_BIN_TPL.REPACK.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_UHD_SCENARIO_SMD.REPACK
{
    public class ScenarioRepackUHD : ScenarioRepack
    {
        public Dictionary<(int FileId, int BinID), FinalStructure> FinalBinDic { get; protected set; } = new Dictionary<(int FileId, int BinID), FinalStructure>();

        protected override void ProcessStructure(StartStructure startStructure, SMDLineIdx smdLineIdx, int FinalFileID, int BinId, (int FileId, int SmdID) key)
        {
            var intermediary = BINrepackIntermediary.MakeIntermediaryStructure(startStructure, smdLineIdx, true);
            var level2 = BinRepack.MakeIntermediaryLevel2(intermediary);
            var final = BinRepack.MakeFinalStructure(level2);

            if (final.Vertex_Position_Array.Length > ushort.MaxValue)
            {
                string fileN = key.FileId.ToString("D2");
                if (key.FileId < 0)
                {
                    fileN = "MAINSMD";
                }

                Console.WriteLine("Warning: Number of vertices greater than the limit: " + final.Vertex_Position_Array.Length);
                Console.WriteLine("The limit is: " + ushort.MaxValue +
                    "; BIN ID: " + BinId.ToString("D3") +
                    "; SMD ID: " + key.SmdID.ToString("D3") + 
                    "; FILE ID: " + fileN + ";");
                Console.WriteLine("Use above the vertex limit is permitted, but use with caution;");
            }

            FinalBinDic.Add((FinalFileID, BinId), final);
        }
    }
}
