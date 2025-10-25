using SHARED_TOOLS.REPACK.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHARED_UHD_BIN_TPL.REPACK.Structures
{
    public class IntermediaryLevel2
    {
        public Dictionary<string, IntermediaryLevel2Mesh> Groups { get; set; }

        public IntermediaryLevel2()
        {
            Groups = new Dictionary<string, IntermediaryLevel2Mesh>();
        }
    }

    public class IntermediaryLevel2Mesh
    {
        public string MaterialName { get; set; }

        public List<IntermediaryLevel2Face> Faces { get; set; }

        public IntermediaryLevel2Mesh()
        {
            Faces = new List<IntermediaryLevel2Face>();
        }
    }


    public class IntermediaryLevel2Face
    {
        public ushort Type;
        public ushort Count;

        public List<IntermediaryVertex> Vertexs { get; set; }

        public IntermediaryLevel2Face()
        {
            Vertexs = new List<IntermediaryVertex>();
        }
    }
}
