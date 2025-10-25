using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_SCENARIO_SMD.SCENARIO_REPACK;
using SHARED_TOOLS.ALL;
using SHARED_TOOLS.REPACK.Structures;
using SHARED_TOOLS.SCENARIO;

namespace SHARED_GCWII_SCENARIO_SMD
{
    public static class BINrepackIntermediary
    {
        public static IntermediaryStructure MakeIntermediaryStructure(StartStructure startStructure, SMDLineIdx smdLine,
            out (float X, float Y, float Z) position, out float FarthestVertex)
        {
            // FarthestVertex valor que representa a maior distancia do modelo, tanto para X, Y ou Z
            FarthestVertex = 0;

            // passo 1: pegar valor para centralizar o modelo 3d

            Dictionary<Limits, float> limits = new Dictionary<Limits, float>();

            if (startStructure.FacesByMaterial.Count >= 1)
            {
                var pos = startStructure.FacesByMaterial.First().Value.Faces[0][0].Position;

                limits.Add(Limits.MaxX, pos.X);
                limits.Add(Limits.MinX, pos.X);

                limits.Add(Limits.MaxY, pos.Y);
                limits.Add(Limits.MinY, pos.Y);

                limits.Add(Limits.MaxZ, pos.Z);
                limits.Add(Limits.MinZ, pos.Z);
            }
            else
            {
                limits.Add(Limits.MaxX, 0);
                limits.Add(Limits.MinX, 0);

                limits.Add(Limits.MaxY, 0);
                limits.Add(Limits.MinY, 0);

                limits.Add(Limits.MaxZ, 0);
                limits.Add(Limits.MinZ, 0);
            }

            foreach (var faceGroup in startStructure.FacesByMaterial)
            {
                var Faces = faceGroup.Value.Faces;

                for (int i = 0; i < Faces.Count; i++)
                {
                    for (int t = 0; t < Faces[i].Count; t++)
                    {
                        var item = Faces[i][t].Position;

                        if (item.X < limits[Limits.MinX])
                        {
                            limits[Limits.MinX] = item.X;
                        }

                        if (item.X > limits[Limits.MaxX])
                        {
                            limits[Limits.MaxX] = item.X;
                        }

                        if (item.Y < limits[Limits.MinY])
                        {
                            limits[Limits.MinY] = item.Y;
                        }

                        if (item.Y > limits[Limits.MaxY])
                        {
                            limits[Limits.MaxY] = item.Y;
                        }

                        if (item.Z < limits[Limits.MinZ])
                        {
                            limits[Limits.MinZ] = item.Z;
                        }

                        if (item.Z > limits[Limits.MaxZ])
                        {
                            limits[Limits.MaxZ] = item.Z;
                        }

                    }
                }
            }

            float distanceX = (limits[Limits.MinX] + limits[Limits.MaxX]) / 2;
            float distanceY = (limits[Limits.MinY] + limits[Limits.MaxY]) / 2;
            float distanceZ = (limits[Limits.MinZ] + limits[Limits.MaxZ]) / 2;

            position = (distanceX, distanceY, distanceZ);

            // segunda e terceira etapas
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

                        pos1[0] = ((pos1[0]) - (distanceX * CONSTs.GLOBAL_POSITION_SCALE)) / scaleX;
                        pos1[1] = ((pos1[1]) - (distanceY * CONSTs.GLOBAL_POSITION_SCALE)) / scaleY;
                        pos1[2] = ((pos1[2]) - (distanceZ * CONSTs.GLOBAL_POSITION_SCALE)) / scaleZ;

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

                        vertex.NormalX = normal1[0];
                        vertex.NormalY = normal1[1];
                        vertex.NormalZ = normal1[2];

                        vertex.TextureU = item.Value.Faces[i][iv].Texture.U;
                        vertex.TextureV = item.Value.Faces[i][iv].Texture.V;

                        vertex.ColorR = item.Value.Faces[i][iv].Color.R;
                        vertex.ColorG = item.Value.Faces[i][iv].Color.G;
                        vertex.ColorB = item.Value.Faces[i][iv].Color.B;
                        vertex.ColorA = item.Value.Faces[i][iv].Color.A;

                        vertex.WeightMap = item.Value.Faces[i][iv].WeightMap;

                        face.Vertexs.Add(vertex);

                        //-------------
                        // --- verifica o vertice mais distante

                        float temp = vertex.PosX;
                        if (temp < 0)
                        {
                            temp *= -1;
                        }
                        if (temp > FarthestVertex)
                        {
                            FarthestVertex = temp;
                        }

                        temp = vertex.PosY;
                        if (temp < 0)
                        {
                            temp *= -1;
                        }
                        if (temp > FarthestVertex)
                        {
                            FarthestVertex = temp;
                        }

                        temp = vertex.PosZ;
                        if (temp < 0)
                        {
                            temp *= -1;
                        }
                        if (temp > FarthestVertex)
                        {
                            FarthestVertex = temp;
                        }

                    }

                    mesh.Faces.Add(face);
                }

                mesh.MaterialName = item.Key.ToUpperInvariant();
                intermediary.Groups.Add(mesh.MaterialName, mesh);
            }

            return intermediary;
        }

        private enum Limits
        {
            MinX,
            MaxX,
            MinY,
            MaxY,
            MinZ,
            MaxZ
        }
    }
}
