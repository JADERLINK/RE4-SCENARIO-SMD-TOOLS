using SHARED_GCWII_BIN.REPACK;
using SHARED_GCWII_BIN.REPACK.Structures;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ;
using SHARED_TOOLS.REPACK.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_GCWII_SCENARIO_SMD.REPACK
{
    public class ScenarioRepackGCWII : ScenarioRepack
    {
        public Dictionary<(int FileId, int BinID), FinalStructure> FinalBinDic { get; protected set; } = new Dictionary<(int FileId, int BinID), FinalStructure>();
        public Dictionary<(int FileId, int BinID), byte> vertex_scale_Dic { get; protected set; } = new Dictionary<(int FileId, int BinID), byte>();

        protected override void ProcessStructure(StartStructure startStructure, SMDLineIdx smdLineIdx, int FinalFileID, int BinId, (int FileId, int SmdID) key)
        {
            (float X, float Y, float Z) position = (0, 0, 0);
            float FarthestVertex = 0;
            byte vertex_scale = 0;

            var intermediaryStructure = BINrepackIntermediary.MakeIntermediaryStructure(startStructure, smdLineIdx, out position, out FarthestVertex);
            IntermediaryLevel2 level2 = BinRepack.MakeIntermediaryLevel2(intermediaryStructure,
                        true, false, FarthestVertex, out vertex_scale);
            var final = BinRepack.MakeFinalStructure(level2);

            smdLineIdx.PositionX = position.X;
            smdLineIdx.PositionY = position.Y;
            smdLineIdx.PositionZ = position.Z;

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
                string erroMesage = "Use above the vertex limit is prohibited, as there is no way to place excess information in the BIN file;";
                Console.WriteLine(erroMesage);
                throw new ApplicationException(erroMesage);
            }

            vertex_scale_Dic.Add((FinalFileID, BinId), vertex_scale);
            FinalBinDic.Add((FinalFileID, BinId), final);
        }
    }

}
