using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_UHD_BIN_TPL.REPACK.Structures;

namespace SHARED_UHD_SCENARIO_SMD.REPACK
{
    public static class CheckDinamicVertexColor
    {

        public static bool Check(FinalStructure finalStructure, bool EnableDinamicVertexColor) 
        {
            if (EnableDinamicVertexColor)
            {
                foreach (var color in finalStructure.Vertex_Color_Array)
                {
                    if (color.r != 255 || color.g != 255 || color.b != 255 || color.a != 255)
                    {
                        return true;
                    }
                }
            }

            return false;
        }



    }
}
