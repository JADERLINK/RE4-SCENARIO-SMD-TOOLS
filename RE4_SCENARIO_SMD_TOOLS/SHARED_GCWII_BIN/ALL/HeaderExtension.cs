using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_GCWII_BIN.EXTRACT;

namespace SHARED_GCWII_BIN.ALL
{
    public static class HeaderExtension
    {
        public static bool ReturnHasEnableModernStyle(this GcWiiBinHeader header) 
        {
            return ((BinFlags)header.Bin_flags).HasFlag(BinFlags.EnableModernStyle);
        }

        public static bool ReturnsHasNormalsAlternativeTag(this GcWiiBinHeader header)
        {
            return ((BinFlags)header.Bin_flags).HasFlag(BinFlags.EnableAltNormals);
        }

        public static bool ReturnsHasEnableBonepairTag(this GcWiiBinHeader header)
        {
            return ((BinFlags)header.Bin_flags).HasFlag(BinFlags.EnableBonepairTag);
        }

        public static bool ReturnsHasEnableAdjacentBoneTag(this GcWiiBinHeader header)
        {
            return ((BinFlags)header.Bin_flags).HasFlag(BinFlags.EnableAdjacentBoneTag);
        }

        public static bool ReturnsHasEnableVertexColorsTag(this GcWiiBinHeader header)
        {
            return ((BinFlags)header.Bin_flags).HasFlag(BinFlags.EnableVertexColors);
        }

    }
}
