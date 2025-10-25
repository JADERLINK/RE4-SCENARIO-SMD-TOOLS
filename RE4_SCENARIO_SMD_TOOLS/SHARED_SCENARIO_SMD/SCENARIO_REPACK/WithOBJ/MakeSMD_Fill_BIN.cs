using System;
using System.Collections.Generic;
using System.Text;
using SimpleEndianBinaryIO;

namespace SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ
{
    public abstract class MakeSMD_Fill_BIN
    {
        public void Fill(EndianBinaryWriter bw, int binFilesCount, uint BinAreaOffset, out long endOffset) 
        {
            //---------------------------
            // PARTE DOS ARQUIVOS BINs

            int BinOffsetBlockCount = (((binFilesCount * 4) + 15) / 16) * 16;

            bw.Position = BinAreaOffset;
            bw.Write(new byte[BinOffsetBlockCount]);

            long OffsetToOffsetBin = BinAreaOffset;
            long RealOffsetBin = BinOffsetBlockCount;

            for (int i = 0; i < binFilesCount; i++)
            {
                bw.Position = OffsetToOffsetBin;
                bw.Write((uint)RealOffsetBin);
                long startBinOffset = BinAreaOffset + RealOffsetBin;
                long endBinOffset;

                PutBin(bw, i, startBinOffset, out endBinOffset);

                OffsetToOffsetBin += 4;
                RealOffsetBin = (endBinOffset - BinAreaOffset);
                bw.Position = endBinOffset;
            }

            endOffset = bw.Position;
        }

        protected abstract void PutBin(EndianBinaryWriter bw, int binId, long startBinOffset, out long endBinOffset);
    }
}
