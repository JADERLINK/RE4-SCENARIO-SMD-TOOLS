using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHARED_UHD_BIN_TPL.EXTRACT;

namespace SHARED_UHD_BIN_TPL.ALL
{
    public static class HeaderExtension
    {
        public static bool ReturnsHasNormalsAlternativeTag(this UhdBinHeader header)
        {
            return ((BinFlags)header.Bin_flags).HasFlag(BinFlags.EnableAltNormals);
        }

        public static bool ReturnsHasEnableBonepairTag(this UhdBinHeader header)
        {
            return ((BinFlags)header.Bin_flags).HasFlag(BinFlags.EnableBonepairTag);
        }

        public static bool ReturnsHasEnableAdjacentBoneTag(this UhdBinHeader header)
        {
            return ((BinFlags)header.Bin_flags).HasFlag(BinFlags.EnableAdjacentBoneTag);
        }

        public static bool ReturnsHasEnableVertexColorsTag(this UhdBinHeader header)
        {
            return ((BinFlags)header.Bin_flags).HasFlag(BinFlags.EnableVertexColors);
        }
    }
}
