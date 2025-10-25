using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.R100
{
    public class R100ToFileMethods
    {
        public int fileID { get; set; }

        public Dictionary<(int fileID, int binID), (long binOffset, long endOffset)> binOffsetList { get; private set; }
        public Dictionary<(int fileID, int tplID), (long tplOffset, long endOffset)> tplOffsetList { get; private set; }

        public R100ToFileMethods()
        {
            fileID = -1;
            binOffsetList = new Dictionary<(int fileId, int binId), (long binOffset, long endOffset)>();
            tplOffsetList = new Dictionary<(int fileId, int binId), (long tplOffset, long endOffset)>();
        }

        public void ToFileBin(Stream fileStream, long binOffset, long endOffset, int binID)
        {
            var key = (fileID, binID);
            var value = (binOffset, endOffset);
            if (!binOffsetList.ContainsKey(key))
            {
                binOffsetList.Add(key, value);
            }
        }

        public void ToFileTpl(Stream fileStream, long tplOffset, long endOffset, int tplID)
        {
            var key = (fileID, tplID);
            var value = (tplOffset, endOffset);
            if (!tplOffsetList.ContainsKey(key))
            {
                tplOffsetList.Add(key, value);
            }
        }

    }
}
