using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT
{
    public class Extract_BIN_Inside_SMD
    {
        //Action<Stream fileStream, long binOffset, long endOffset, int BinID>
        public Action<Stream, long, long, int> ToFileBin;

        //Func<int BinID, Stream fileStream, long binOffset, Endianness endianness, long endOffset>
        public Func<int, Stream, long, long> ToExtractBin;

        public void ExtractBINs(Stream fileStream, Endianness endianness, uint offsetBinArr, int BinFilesCount)
        {
            EndianBinaryReader br = new EndianBinaryReader(fileStream, endianness);
            br.Position = offsetBinArr;

            uint[] binOffsets = new uint[BinFilesCount];

            for (int i = 0; i < BinFilesCount; i++)
            {
                uint offset = br.ReadUInt32();
                if (offset != 0 && offset + offsetBinArr < fileStream.Length)
                {
                    binOffsets[i] = offset + offsetBinArr;
                }
            }

            for (int i = 0; i < BinFilesCount; i++)
            {
                long endOffset = ToExtractBin?.Invoke(i, fileStream, binOffsets[i]) ?? 0;
                ToFileBin?.Invoke(fileStream, binOffsets[i], endOffset, i);
            }
        }


    }
}
