using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_GCWII_BIN.REPACK.Structures;
using SHARED_TOOLS.ALL;
using SHARED_TOOLS.REPACK.Structures;

namespace SHARED_GCWII_BIN.REPACK
{
    public static partial class BinRepack
    {
        // FarthestVertex = maior valor encontrado no plano cartesiano (float)
        // Retorna o expoente (0..15) e o fator (2^expoente)
        private static void CalculateFactor(float FarthestVertex, out int exponent, out float factor)
        {
            if (FarthestVertex <= 0)
            {
                exponent = 0;
                factor = 1;
                return;
            }

            // fator ideal (ideal factor)
            double idealFactor = short.MaxValue / FarthestVertex;

            // log2 do fator ideal
            double log2 = Math.Log(idealFactor, 2.0);

            // piso (floor), para nunca ultrapassar short.MaxValue
            exponent = (int)Math.Floor(log2);

            // limitar no intervalo permitido
            if (exponent < 0) { exponent = 0; }
            if (exponent > 0xF) { exponent = 0xF; }

            // fator final = 2^expoente
            factor = (float)Math.Pow(2, exponent);
        }

        public static IntermediaryLevel2 MakeIntermediaryLevel2(IntermediaryStructure intermediaryStructure, bool useAltNormal, bool IsRe1Style, float FarthestVertex, out byte vertex_scale)
        {
            int exponent;
            float factor;
            CalculateFactor(FarthestVertex, out exponent, out factor);
            vertex_scale = (byte)exponent;

            IntermediaryLevel2 level2 = new IntermediaryLevel2();

            foreach (var item in intermediaryStructure.Groups)
            {
                level2.Groups.Add(item.Key, MakeIntermediaryLevel2Mesh(item.Value, factor, useAltNormal, IsRe1Style));
            }

            return level2;
        }

        private static IntermediaryLevel2Mesh MakeIntermediaryLevel2Mesh(IntermediaryMesh intermediaryMesh, float factor, bool useAltNormal, bool IsRe1Style)
        {
            const byte FACE_TYPE_TRIANGLE_LIST = 0x90;
            const byte FACE_TYPE_TRIANGLE_STRIP = 0x98;
            const byte FACE_TYPE_QUAD_LIST = 0x80;

            IntermediaryLevel2Mesh mesh = new IntermediaryLevel2Mesh();
            mesh.MaterialName = intermediaryMesh.MaterialName;

            for (int i = 0; i < intermediaryMesh.Faces.Count; i++)
            {
                ushort count = (ushort)intermediaryMesh.Faces[i].Vertexs.Count;

                if (count == 3) // triangulo
                {
                    var res = (from obj in mesh.Faces
                               where obj.Type == FACE_TYPE_TRIANGLE_LIST && obj.Count < short.MaxValue
                               select obj).ToList();

                    if (res.Count != 0)
                    {
                        res[0].Count += count;
                        res[0].Vertexs.AddRange(intermediaryMesh.Faces[i].Vertexs.Select(x => IntermediaryVertexConvert(x, factor, useAltNormal, IsRe1Style)));
                    }
                    else // é o primeiro tem que colocar um novo.
                    {
                        IntermediaryLevel2Face level2Face = new IntermediaryLevel2Face();
                        level2Face.Count = count;
                        level2Face.Type = FACE_TYPE_TRIANGLE_LIST;
                        level2Face.Vertexs.AddRange(intermediaryMesh.Faces[i].Vertexs.Select(x => IntermediaryVertexConvert(x, factor, useAltNormal, IsRe1Style)));
                        mesh.Faces.Add(level2Face);
                    }

                }
                else if (count == 4) //vai virar quad
                {
                    List<IntermediaryVertex> reordered = new List<IntermediaryVertex>();
                    reordered.Add(intermediaryMesh.Faces[i].Vertexs[2]);
                    reordered.Add(intermediaryMesh.Faces[i].Vertexs[0]);
                    reordered.Add(intermediaryMesh.Faces[i].Vertexs[1]);
                    reordered.Add(intermediaryMesh.Faces[i].Vertexs[3]);

                    var res = (from obj in mesh.Faces
                               where obj.Type == FACE_TYPE_QUAD_LIST && obj.Count < short.MaxValue
                               select obj).ToList();

                    if (res.Count != 0)
                    {
                        res[0].Count += count;
                        res[0].Vertexs.AddRange(reordered.Select(x => IntermediaryVertexConvert(x, factor, useAltNormal, IsRe1Style)));
                    }
                    else // é o primeiro tem que colocar um novo.
                    {
                        IntermediaryLevel2Face level2Face = new IntermediaryLevel2Face();
                        level2Face.Count = count;
                        level2Face.Type = FACE_TYPE_QUAD_LIST;
                        level2Face.Vertexs.AddRange(reordered.Select(x => IntermediaryVertexConvert(x, factor, useAltNormal, IsRe1Style)));
                        mesh.Faces.Add(level2Face);
                    }

                }
                else if (count > 4) // se for maior que 4 é porque é triangle strip
                {
                    IntermediaryLevel2Face level2Face = new IntermediaryLevel2Face();
                    level2Face.Count = count;
                    level2Face.Type = FACE_TYPE_TRIANGLE_STRIP;
                    level2Face.Vertexs.AddRange(intermediaryMesh.Faces[i].Vertexs.Select(x => IntermediaryVertexConvert(x, factor, useAltNormal, IsRe1Style)));
                    mesh.Faces.Add(level2Face);

                }
            }


            return mesh;
        }

        private static IntermediaryVertex2 IntermediaryVertexConvert(IntermediaryVertex vertex, float factor, bool useAltNormal, bool IsRe1Style) 
        {
            IntermediaryVertex2 res = new IntermediaryVertex2();

            res.PosX = Utils.ParseFloatToShort(vertex.PosX * factor);
            res.PosY = Utils.ParseFloatToShort(vertex.PosY * factor);
            res.PosZ = Utils.ParseFloatToShort(vertex.PosZ * factor);

            if (useAltNormal == false || IsRe1Style == true)
            {
                res.NormalX = Utils.ParseFloatToShort(vertex.NormalX * short.MaxValue);
                res.NormalY = Utils.ParseFloatToShort(vertex.NormalY * short.MaxValue);
                res.NormalZ = Utils.ParseFloatToShort(vertex.NormalZ * short.MaxValue);
            }
            else 
            {
                res.NormalX = Utils.ParseFloatToShort(vertex.NormalX * sbyte.MaxValue);
                res.NormalY = Utils.ParseFloatToShort(vertex.NormalY * sbyte.MaxValue);
                res.NormalZ = Utils.ParseFloatToShort(vertex.NormalZ * sbyte.MaxValue);
            }

            if (IsRe1Style)
            {
                res.TextureU = Utils.ParseFloatToShort(vertex.TextureU * short.MaxValue);
                res.TextureV = Utils.ParseFloatToShort(vertex.TextureV * short.MaxValue);
            }
            else 
            {
                res.TextureU = Utils.ParseFloatToShort(vertex.TextureU * byte.MaxValue);
                res.TextureV = Utils.ParseFloatToShort(vertex.TextureV * byte.MaxValue);
            }

            res.ColorA = vertex.ColorA;
            res.ColorR = vertex.ColorR;
            res.ColorG = vertex.ColorG;
            res.ColorB = vertex.ColorB;

            res.WeightMap = vertex.WeightMap;

            return res;
        }


    }
}
