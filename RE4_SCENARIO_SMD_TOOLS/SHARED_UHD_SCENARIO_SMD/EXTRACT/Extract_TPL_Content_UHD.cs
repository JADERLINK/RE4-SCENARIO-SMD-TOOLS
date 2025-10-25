using SHARED_UHD_BIN_TPL.EXTRACT;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_UHD_SCENARIO_SMD.EXTRACT
{
    public class Extract_TPL_Content_UHD
    {
        public Dictionary<int, UhdTPL> UhdTplDic { get; private set; }

        private Endianness _endianness;
        private bool _isPS4NS;

        public Extract_TPL_Content_UHD(Endianness endianness, bool isPS4NS)
        {
            UhdTplDic = new Dictionary<int, UhdTPL>();
            _endianness = endianness;
            _isPS4NS = isPS4NS;
        }

        public long ToExtractTpl(int tplID, Stream fileStream, long StartOffset)
        {
            long endOffset = StartOffset;
            if (StartOffset > 0)
            {
                try
                {
                    var uhdTpl = UhdTplDecoder.Decoder(fileStream, StartOffset, out endOffset, _isPS4NS, _endianness);
                    UhdTplDic.Add(tplID, uhdTpl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error on Read TPL in SMD: " + tplID + Environment.NewLine + ex.ToString());
                }
            }

            return endOffset;
        }

    }
}
