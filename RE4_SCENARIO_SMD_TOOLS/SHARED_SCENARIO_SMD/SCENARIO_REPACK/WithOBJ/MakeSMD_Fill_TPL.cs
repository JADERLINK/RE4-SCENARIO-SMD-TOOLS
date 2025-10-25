using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_REPACK.WithOBJ
{
    public abstract class MakeSMD_Fill_TPL
    {
        public void Fill(EndianBinaryWriter bw, int tplFilesCount, uint TplAreaOffset, out long endOffset)
        {
            //---------------------------
            // PARTE DOS ARQUIVOS TPLs

            int TplOffsetBlockCount = (int)((((TplAreaOffset + (tplFilesCount * 4) + (TplAlignment() - 1)) / TplAlignment()) * TplAlignment()) - TplAreaOffset);

            bw.Position = TplAreaOffset;
            bw.Write(new byte[TplOffsetBlockCount]);

            long OffsetToOffsetTpl = TplAreaOffset;
            long RealOffsetTpl = TplOffsetBlockCount;

            for (int i = 0; i < tplFilesCount; i++)
            {
                bw.Position = OffsetToOffsetTpl;
                bw.Write((uint)RealOffsetTpl);
                long startTplOffset = TplAreaOffset + RealOffsetTpl;
                long endTplOffset;

                PutTpl(bw, i, startTplOffset, out endTplOffset);

                OffsetToOffsetTpl += 4;
                RealOffsetTpl = (endTplOffset - TplAreaOffset);
                bw.Position = endTplOffset;
            }

            endOffset = bw.Position;
        }

        protected abstract void PutTpl(EndianBinaryWriter bw, int tplId, long startTplOffset, out long endTplOffset);

        protected abstract int TplAlignment();

    }
}
