using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_TOOLS.ALL;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SHARED_TOOLS.SCENARIO;
using SHARED_TOOLS.REPACK.Structures;

namespace SHARED_UHD_SCENARIO_SMD
{
    public static class BINrepackIntermediary
    {
        private const float GLOBAL_NORMAL_FIX_EXTENDED = 545460800000f;
        private const float GLOBAL_NORMAL_FIX_REDUCED = 16384f;

        public static IntermediaryStructure MakeIntermediaryStructure(StartStructure startStructure, SMDLineIdx smdLine, bool UseExtendedNormals)
        {
            float NORMAL_FIX = UseExtendedNormals ? GLOBAL_NORMAL_FIX_EXTENDED : GLOBAL_NORMAL_FIX_REDUCED;

            IntermediaryStructure intermediary = new IntermediaryStructure();

            foreach (var item in startStructure.FacesByMaterial)
            {
                IntermediaryMesh mesh = new IntermediaryMesh();

                for (int i = 0; i < item.Value.Faces.Count; i++)
                {
                    IntermediaryFace face = new IntermediaryFace();

                    for (int iv = 0; iv < item.Value.Faces[i].Count; iv++)
                    {
                        IntermediaryVertex vertex = new IntermediaryVertex();

                        float[] pos1 = new float[3];// 0 = x, 1 = y, 2 = z
                        pos1[0] = item.Value.Faces[i][iv].Position.X * CONSTs.GLOBAL_POSITION_SCALE;
                        pos1[1] = item.Value.Faces[i][iv].Position.Y * CONSTs.GLOBAL_POSITION_SCALE;
                        pos1[2] = item.Value.Faces[i][iv].Position.Z * CONSTs.GLOBAL_POSITION_SCALE;

                        float scaleX = smdLine.ScaleX != 0 ? smdLine.ScaleX : 1;
                        float scaleY = smdLine.ScaleY != 0 ? smdLine.ScaleY : 1;
                        float scaleZ = smdLine.ScaleZ != 0 ? smdLine.ScaleZ : 1;

                        pos1[0] = ((pos1[0]) - (smdLine.PositionX * CONSTs.GLOBAL_POSITION_SCALE)) / scaleX;
                        pos1[1] = ((pos1[1]) - (smdLine.PositionY * CONSTs.GLOBAL_POSITION_SCALE)) / scaleY;
                        pos1[2] = ((pos1[2]) - (smdLine.PositionZ * CONSTs.GLOBAL_POSITION_SCALE)) / scaleZ;

                        pos1 = RotationUtils.RotationInZ(pos1, -smdLine.AngleZ);
                        pos1 = RotationUtils.RotationInY(pos1, -smdLine.AngleY);
                        pos1 = RotationUtils.RotationInX(pos1, -smdLine.AngleX);

                        vertex.PosX = pos1[0];
                        vertex.PosY = pos1[1];
                        vertex.PosZ = pos1[2];

                        float[] normal1 = new float[3];// 0 = x, 1 = y, 2 = z
                        normal1[0] = item.Value.Faces[i][iv].Normal.X;
                        normal1[1] = item.Value.Faces[i][iv].Normal.Y;
                        normal1[2] = item.Value.Faces[i][iv].Normal.Z;

                        normal1 = RotationUtils.RotationInZ(normal1, -smdLine.AngleZ);
                        normal1 = RotationUtils.RotationInY(normal1, -smdLine.AngleY);
                        normal1 = RotationUtils.RotationInX(normal1, -smdLine.AngleX);

                        vertex.NormalX = normal1[0] * NORMAL_FIX;
                        vertex.NormalY = normal1[1] * NORMAL_FIX;
                        vertex.NormalZ = normal1[2] * NORMAL_FIX;

                        vertex.TextureU = item.Value.Faces[i][iv].Texture.U;
                        vertex.TextureV = item.Value.Faces[i][iv].Texture.V;

                        vertex.ColorR = item.Value.Faces[i][iv].Color.R;
                        vertex.ColorG = item.Value.Faces[i][iv].Color.G;
                        vertex.ColorB = item.Value.Faces[i][iv].Color.B;
                        vertex.ColorA = item.Value.Faces[i][iv].Color.A;

                        vertex.WeightMap = item.Value.Faces[i][iv].WeightMap;

                        face.Vertexs.Add(vertex);
                    }

                    mesh.Faces.Add(face);
                }

                mesh.MaterialName = item.Key.ToUpperInvariant();
                intermediary.Groups.Add(mesh.MaterialName, mesh);
            }

            return intermediary;
        }

    }
}
