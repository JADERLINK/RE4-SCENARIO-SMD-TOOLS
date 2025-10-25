using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT
{
    public abstract class Extract_TPL_Inside_SMD
    {
        //Action<Stream fileStream, long tplOffset, long endOffset, int TplID>
        public Action<Stream, long, long, int> ToFileTpl;

        //Func<int TplID, Stream fileStream, long tplOffset, long endOffset>
        public Func<int, Stream, long, long> ToExtractTpl;

        public abstract void ExtractTPLs(Stream fileStream, Endianness endianness, uint offsetTplArr, int TplFilesCount);
    }
}
