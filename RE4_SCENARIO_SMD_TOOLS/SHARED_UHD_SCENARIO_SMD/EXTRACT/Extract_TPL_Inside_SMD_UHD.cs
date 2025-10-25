using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_UHD_SCENARIO_SMD.EXTRACT
{
    public class Extract_TPL_Inside_SMD_UHD : Extract_TPL_Inside_SMD
    {
        public override void ExtractTPLs(Stream fileStream, Endianness endianness, uint offsetTplArr, int TplFilesCount)
        {
            EndianBinaryReader br = new EndianBinaryReader(fileStream, endianness);
            br.Position = offsetTplArr;

            uint[] TplOffsets = new uint[TplFilesCount];

            for (int i = 0; i < TplFilesCount; i++)
            {
                uint offset = br.ReadUInt32();
                if (offset != 0 && offset + offsetTplArr < fileStream.Length)
                {
                    TplOffsets[i] = offset + offsetTplArr;
                }
            }

            for (int i = 0; i < TplFilesCount; i++)
            {
                long endOffset = ToExtractTpl?.Invoke(i, fileStream, TplOffsets[i]) ?? 0;
                ToFileTpl?.Invoke(fileStream, TplOffsets[i], endOffset, i);
            }
        }

    }
}
