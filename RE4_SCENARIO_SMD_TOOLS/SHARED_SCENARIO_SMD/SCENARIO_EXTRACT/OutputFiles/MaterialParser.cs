using SHARED_TOOLS.ALL;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.OutputFiles
{
    public static class MaterialParser
    {
        public static IdxMaterial IdxMaterialMultiParser(Dictionary<int, GenericModelBIN> BinDic, out Dictionary<MaterialPart, string> invDic)
        {
            IdxMaterial idx = new IdxMaterial();
            idx.MaterialDic = new Dictionary<string, MaterialPart>();
            invDic = new Dictionary<MaterialPart, string>();

            int counter = 0;

            foreach (var item in BinDic)
            {
                for (int i = 0; i < item.Value.Materials.Length; i++)
                {
                    var mat = item.Value.Materials[i].material;

                    if (!invDic.ContainsKey(mat))
                    {
                        string matKey = CONSTs.SCENARIO_MATERIAL + counter.ToString("D3");
                        invDic.Add(mat, matKey);
                        idx.MaterialDic.Add(matKey, mat);
                        counter++;
                    }
                }
            }

            return idx;
        }

        public static IdxMaterial IdxMaterialMultiParser(SMDLine[] smdLines, Dictionary<int, GenericModelBIN> BinDic, 
            out Dictionary<(MaterialPart mat, ushort MagicID), string> invDic, 
            out Dictionary<(string MaterialName, ushort MagicID), MaterialPart> matWithMagicIdDic)
        {
            IdxMaterial idx = new IdxMaterial();
            idx.MaterialDic = new Dictionary<string, MaterialPart>();
            invDic = new Dictionary<(MaterialPart mat, ushort MagicID), string>();
            matWithMagicIdDic = new Dictionary<(string MaterialName, ushort MagicID), MaterialPart>();

            int counter = 0;

            foreach (var smdLine in smdLines)
            {
                if (BinDic.ContainsKey(smdLine.BinFileID))
                {
                    for (int i = 0; i < BinDic[smdLine.BinFileID].Materials.Length; i++)
                    {
                        MaterialPart mat = BinDic[smdLine.BinFileID].Materials[i].material;
                        ushort magicID = smdLine.IsNotSharedBIN() ? smdLine.TplFileID : (byte)0;

                        if (!invDic.ContainsKey((mat, magicID)))
                        {
                            string matKey = CONSTs.SCENARIO_MATERIAL + counter.ToString("D3");
                            invDic.Add((mat, magicID), matKey);
                            idx.MaterialDic.Add(matKey, mat);
                            matWithMagicIdDic.Add((matKey, magicID), mat);
                            counter++;
                        }
                    }

                }

            }

            return idx;
        }

        public static IdxMaterial IdxMaterialMultParser(SMDLine[][] smdLines, Dictionary<int, GenericModelBIN>[] modelList,
            out Dictionary<(MaterialPart mat, ushort MagicID), string> invDic,
            out Dictionary<(string MaterialName, ushort MagicID), MaterialPart> matWithMagicIdDic,
            int[] order, int mainFileID, int sharedFileID)
        {
            IdxMaterial idx = new IdxMaterial();
            idx.MaterialDic = new Dictionary<string, MaterialPart>();
            Dictionary<(MaterialPart mat, ushort MagicID), string> _invDic = new Dictionary<(MaterialPart mat, ushort MagicID), string>();
            Dictionary<(string MaterialName, ushort MagicID), MaterialPart>  _matWithMagicIdDic = new Dictionary<(string MaterialName, ushort MagicID), MaterialPart>();

            int counter = 0;

            for (int ix = 0; ix < order.Length -1; ix++)
            {
                int fileID = order[ix];

                ushort baseMagicID = 0;
                if (fileID != mainFileID)
                {
                    baseMagicID = (ushort)((fileID + 1) * 0x01_00);
                }

                foreach (var smdLine in smdLines[fileID])
                {
                    Action<int> method = (inFileId) => {
                        for (int iz = 0; iz < modelList[inFileId][smdLine.BinFileID].Materials.Length; iz++)
                        {
                            var mat = modelList[inFileId][smdLine.BinFileID].Materials[iz].material;

                            ushort magicID = smdLine.IsNotSharedBIN() ? (ushort)(baseMagicID + smdLine.TplFileID) : (byte)0;
                            magicID = smdLine.TplFileID == 0 ? (byte)0 : magicID;

                            if (!_invDic.ContainsKey((mat, magicID)))
                            {
                                string matKey = CONSTs.SCENARIO_MATERIAL + counter.ToString("D3");
                                _invDic.Add((mat, magicID), matKey);
                                idx.MaterialDic.Add(matKey, mat);
                                _matWithMagicIdDic.Add((matKey, magicID), mat);
                                counter++;
                            }
                        }
                    };

                    if (smdLine.IsNotSharedBIN() && modelList[fileID].ContainsKey(smdLine.BinFileID))
                    {
                        method(fileID);
                    }
                    else if (smdLine.IsSharedBIN() && modelList[sharedFileID].ContainsKey(smdLine.BinFileID))
                    {
                        method(sharedFileID);
                    }
                }
             
            }

            invDic = _invDic;
            matWithMagicIdDic = _matWithMagicIdDic;

            return idx;
        }
    }
}
