using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_UHD_BIN_TPL.EXTRACT;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_UHD_SCENARIO_SMD.EXTRACT
{
    public class Extract_BIN_Content_UHD : Extract_BIN_Content
    {
        private Endianness _endianness;
        private bool _isPS4NS;

        public Extract_BIN_Content_UHD(Endianness endianness, bool isPS4NS) : base()
        {
            _endianness = endianness;
            _isPS4NS = isPS4NS;
        }

        public override long ToExtractBin(int BinID, Stream fileStream, long StartOffset)
        {
            long endOffset = StartOffset;
            if (StartOffset > 0)
            {
                try
                {
                    var uhdBin = UhdBinDecoder.Decoder(fileStream, StartOffset, out endOffset, _isPS4NS, _endianness);
                    BIN_DIC.Add(BinID, UhdBin_To_GenericModel.Converter(uhdBin));
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
