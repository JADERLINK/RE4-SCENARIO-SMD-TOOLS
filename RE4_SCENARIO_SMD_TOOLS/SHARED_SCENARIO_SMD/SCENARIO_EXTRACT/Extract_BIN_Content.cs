using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT
{
    public abstract class Extract_BIN_Content
    {
        public Dictionary<int, GenericModelBIN> BIN_DIC { get; private set; }

        protected Extract_BIN_Content() 
        {
            BIN_DIC = new Dictionary<int, GenericModelBIN>();
        }

        public abstract long ToExtractBin(int BinID, Stream fileStream, long StartOffset);
    }
}
