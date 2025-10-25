using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHARED_TOOLS.REPACK.Structures
{
    public class IntermediaryStructure
    {
        public Dictionary<string, IntermediaryMesh> Groups { get; set; }

        public IntermediaryStructure()
        {
            Groups = new Dictionary<string, IntermediaryMesh>();
        }
    }

    public class IntermediaryMesh
    {
        public string MaterialName { get; set; }

        public List<IntermediaryFace> Faces { get; set; }

        public IntermediaryMesh()
        {
            Faces = new List<IntermediaryFace>();
        }
    }


    public class IntermediaryFace
    {
        public List<IntermediaryVertex> Vertexs { get; set; }

        public IntermediaryFace()
        {
            Vertexs = new List<IntermediaryVertex>();
        }
    }

    public class IntermediaryVertex
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public float NormalX { get; set; }
        public float NormalY { get; set; }
        public float NormalZ { get; set; }

        public float TextureU { get; set; }
        public float TextureV { get; set; }

        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public byte ColorA { get; set; }
        public FinalWeightMap WeightMap { get; set; }
    }


}
