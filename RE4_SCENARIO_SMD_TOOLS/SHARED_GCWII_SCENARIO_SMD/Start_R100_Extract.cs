using SHARED_GCWII_SCENARIO_SMD.EXTRACT;
using SHARED_SCENARIO_SMD;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_TOOLS.ALL;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace SHARED_GCWII_SCENARIO_SMD
{
    public class Start_R100_Extract : Base_R100_Extract
    {
        public static void R100Extract(FileInfo fileInfo1)
        {
            Start_R100_Extract start = new Start_R100_Extract();
            start.BaseR100Extract(fileInfo1);
        }

        private Start_R100_Extract() { }

        protected override Endianness Get_Endianness()
        {
            return Endianness.BigEndian;
        }

        protected override bool IsGCWii()
        {
            return true;
        }

        protected override string idxr100repack()
        {
            return ".idxggr100repack";
        }

        protected override string idxscenario()
        {
            return ".idxggscenario";
        }

        protected override string idxsmd()
        {
            return ".idxggsmd";
        }

        protected override string Get_MainTplBaseName(string baseFileName)
        {
            return baseFileName + ".TPL";
        }


        protected override void CreateMtlIdxMaterialIdxuhdTpl(Dictionary<(string MaterialName, ushort MagicID), MaterialPart> materialWithTplFileIdDic, 
            IdxMaterial idxMaterial, string baseDirectory, string fileBaseName, string[] TplNames, string mainTplFileBaseName)
        {
            var Magics = materialWithTplFileIdDic.Select(a => a.Key.MagicID).ToHashSet();

            SHARED_GCWII_BIN.ALL.IdxMtl _idxMtl = new SHARED_GCWII_BIN.ALL.IdxMtl() {MtlDic = new Dictionary<string, SHARED_GCWII_BIN.ALL.MtlObj>() };

            foreach (var magicID in Magics)
            {
                int M_SMD_FileID = ((magicID >> 8) - 1); // -1 é o main e shared
                int M_TPL_FIleID = (magicID & 0xFF);

                string newTplName = mainTplFileBaseName;
                if (M_SMD_FileID > -1)
                {
                    newTplName = TplNames[M_SMD_FileID] + "." + M_TPL_FIleID.ToString("D");
                }
                else if (M_TPL_FIleID > 0)
                {
                    newTplName = mainTplFileBaseName + "." + M_TPL_FIleID.ToString("D");
                }

                var temp_idxMaterial = new IdxMaterial()
                {
                    MaterialDic = materialWithTplFileIdDic.Where(a => a.Key.MagicID == magicID).ToDictionary(k => k.Key.MaterialName, v => v.Value)
                };

                var temp_mtl = SHARED_GCWII_BIN.ALL.IdxMtlParser.Parser(temp_idxMaterial, newTplName);
                foreach (var mtlObj in temp_mtl.MtlDic)
                {
                    _idxMtl.MtlDic.Add(mtlObj.Key, mtlObj.Value);
                }
            }
            _idxMtl.MtlDic = _idxMtl.MtlDic.OrderBy(a => a.Key).ToDictionary(K => K.Key, v => v.Value);

            Console.WriteLine("Creating File: " + fileBaseName + ".mtl");
            SHARED_GCWII_BIN.EXTRACT.OutputMaterial.CreateMTL(_idxMtl, baseDirectory, fileBaseName);

            Console.WriteLine("Creating File: " + fileBaseName + ".idxmaterial");
            SHARED_GCWII_BIN.EXTRACT.OutputMaterial.CreateIdxMaterial(idxMaterial, baseDirectory, fileBaseName);
        }

        protected override (string SubDirectoryTpl, string TplName) ReturnSubDirectoryTplAndTplBaseName(string baseDirectory, string FileName)
        {
            return (baseDirectory, FileName);
        }

        protected override void SetUhdTplDic(int inFileID, bool IsSharedFile) { } // não usado no GcWii

        protected override void Set_Extract_BIN_Inside_SMD(out Extract_BIN_Inside_SMD extract_BIN_Inside_SMD, out Extract_BIN_Content extract_BIN_Content)
        {
            extract_BIN_Inside_SMD = new Extract_BIN_Inside_SMD();
            extract_BIN_Content = new Extract_BIN_Content_GCWII();
            extract_BIN_Inside_SMD.ToExtractBin = extract_BIN_Content.ToExtractBin;
        }

        protected override void Set_Extract_TPL_Inside_SMD(out Extract_TPL_Inside_SMD extract_TPL_Inside_SMD, int fileID)
        {
            extract_TPL_Inside_SMD = new Extract_TPL_Inside_SMD_GCWII();
        }

        protected override string GetStartGroupName()
        {
            return "GGSCENARIO";
        }

    }
}
