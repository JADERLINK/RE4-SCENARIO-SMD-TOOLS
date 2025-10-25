using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_TOOLS.REPACK.Structures
{
    /// <summary>
    /// Representa o conjunto de pesos associado a um vértice;
    /// </summary>
    public readonly struct FinalWeightMap : IEquatable<FinalWeightMap>
    {
        public readonly byte Links;

        public readonly byte BoneID1;
        public readonly byte Weight1;

        public readonly byte BoneID2;
        public readonly byte Weight2;

        public readonly byte BoneID3;
        public readonly byte Weight3;

        public FinalWeightMap(byte links, byte boneID1, byte weight1, byte boneID2, byte weight2, byte boneID3, byte weight3)
        {
            Links = links;
            BoneID1 = boneID1;
            Weight1 = weight1;
            BoneID2 = boneID2;
            Weight2 = weight2;
            BoneID3 = boneID3;
            Weight3 = weight3;
        }

        public static bool operator ==(FinalWeightMap lhs, FinalWeightMap rhs) => lhs.Equals(rhs);

        public static bool operator !=(FinalWeightMap lhs, FinalWeightMap rhs) => !(lhs == rhs);

        public bool Equals(FinalWeightMap obj)
        {
            return obj.Links == Links
                && obj.BoneID1 == BoneID1
                && obj.BoneID2 == BoneID2
                && obj.BoneID3 == BoneID3
                && obj.Weight1 == Weight1
                && obj.Weight2 == Weight2
                && obj.Weight3 == Weight3;
        }

        public override bool Equals(object obj)
        {
            return obj is FinalWeightMap map
                && map.Links == Links
                && map.BoneID1 == BoneID1
                && map.BoneID2 == BoneID2
                && map.BoneID3 == BoneID3
                && map.Weight1 == Weight1
                && map.Weight2 == Weight2
                && map.Weight3 == Weight3;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Links.GetHashCode();
                hash = hash * 23 + BoneID1.GetHashCode();
                hash = hash * 23 + Weight1.GetHashCode();
                hash = hash * 23 + BoneID2.GetHashCode();
                hash = hash * 23 + Weight2.GetHashCode();
                hash = hash * 23 + BoneID3.GetHashCode();
                hash = hash * 23 + Weight3.GetHashCode();
                return hash;
            }
        }

    }

}
