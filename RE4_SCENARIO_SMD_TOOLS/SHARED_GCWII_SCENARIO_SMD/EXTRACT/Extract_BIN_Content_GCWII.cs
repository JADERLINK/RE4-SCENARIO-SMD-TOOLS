using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SHARED_GCWII_BIN.EXTRACT;

namespace SHARED_GCWII_SCENARIO_SMD.EXTRACT
{
    public class Extract_BIN_Content_GCWII : Extract_BIN_Content
    {
        public Extract_BIN_Content_GCWII() : base(){ }

        public override long ToExtractBin(int BinID, Stream fileStream, long StartOffset)
        {
            long endOffset = StartOffset;

            if (StartOffset > 0)
            {
                try
                {
                    var Bin = GcWiiBinDecoder.Decoder(fileStream, StartOffset, out endOffset);
                    BIN_DIC.Add(BinID, GcWiiBin_To_GenericModel.Converter(Bin));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on Read BIN in SMD: " + BinID.ToString("D3") + Environment.NewLine + ex.ToString());
                }
            }

            return endOffset;
        }
    }
}
