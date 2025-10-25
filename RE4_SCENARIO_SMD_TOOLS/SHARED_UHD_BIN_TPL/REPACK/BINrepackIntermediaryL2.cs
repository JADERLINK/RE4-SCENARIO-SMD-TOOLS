using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_TOOLS.REPACK.Structures;
using SHARED_UHD_BIN_TPL.REPACK.Structures;

namespace SHARED_UHD_BIN_TPL.REPACK
{
    public static partial class BinRepack
    {
        private const byte FACE_TYPE_TRIANGLE_LIST = 0x05;
        private const byte FACE_TYPE_TRIANGLE_STRIP = 0x06;
        private const byte FACE_TYPE_QUAD_LIST = 0x08;

        public static IntermediaryLevel2 MakeIntermediaryLevel2(IntermediaryStructure intermediaryStructure)
        {
            IntermediaryLevel2 level2 = new IntermediaryLevel2();

            foreach (var item in intermediaryStructure.Groups)
            {
                level2.Groups.Add(item.Key, MakeIntermediaryLevel2Mesh(item.Value));
            }

            return level2;
        }

        private static IntermediaryLevel2Mesh MakeIntermediaryLevel2Mesh(IntermediaryMesh intermediaryMesh)
        {
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
                        res[0].Vertexs.AddRange(intermediaryMesh.Faces[i].Vertexs);
                    }
                    else // é o primeiro tem que colocar um novo.
                    {
                        IntermediaryLevel2Face level2Face = new IntermediaryLevel2Face();
                        level2Face.Count = count;
                        level2Face.Type = FACE_TYPE_TRIANGLE_LIST;
                        level2Face.Vertexs.AddRange(intermediaryMesh.Faces[i].Vertexs);
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
                        res[0].Vertexs.AddRange(reordered);
                    }
                    else // é o primeiro tem que colocar um novo.
                    {
                        IntermediaryLevel2Face level2Face = new IntermediaryLevel2Face();
                        level2Face.Count = count;
                        level2Face.Type = FACE_TYPE_QUAD_LIST;
                        level2Face.Vertexs.AddRange(reordered);
                        mesh.Faces.Add(level2Face);
                    }

                }
                else if (count > 4) // se for maior que 4 é porque é triangle strip
                {
                    IntermediaryLevel2Face level2Face = new IntermediaryLevel2Face();
                    level2Face.Count = count;
                    level2Face.Type = FACE_TYPE_TRIANGLE_STRIP;
                    level2Face.Vertexs.AddRange(intermediaryMesh.Faces[i].Vertexs);
                    mesh.Faces.Add(level2Face);

                }
            }


            return mesh;
        }

    }
}
