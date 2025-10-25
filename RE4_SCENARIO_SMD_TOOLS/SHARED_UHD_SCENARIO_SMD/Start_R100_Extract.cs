using SHARED_SCENARIO_SMD;
using SHARED_SCENARIO_SMD.SCENARIO_EXTRACT;
using SHARED_TOOLS.ALL;
using SHARED_UHD_SCENARIO_SMD.EXTRACT;
using SHARED_UHD_BIN_TPL.EXTRACT;
using SimpleEndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using SHARED_UHD_BIN_TPL.ALL;

namespace SHARED_UHD_SCENARIO_SMD
{
    public class Start_R100_Extract : Base_R100_Extract
    {
        private bool IsPS4NS;
        private Endianness endianness;

        private Dictionary<int, Extract_TPL_Content_UHD> TPL_Content_Dic;

        private Dictionary<ushort, UhdTPL> UhdTplDic; // magicID, TPL

        public static void R100Extract(FileInfo fileInfo1, bool IsPS4NS, Endianness endianness)
        {
            Start_R100_Extract start = new Start_R100_Extract(IsPS4NS, endianness);
            start.BaseR100Extract(fileInfo1);
        }

        private Start_R100_Extract(bool IsPS4NS, Endianness endianness)
        {
            this.IsPS4NS = IsPS4NS;
            this.endianness = endianness;

            TPL_Content_Dic = new Dictionary<int, Extract_TPL_Content_UHD>();
            UhdTplDic = new Dictionary<ushort, UhdTPL>();
        }

        protected override Endianness Get_Endianness()
        {
            return endianness;
        }

        protected override bool IsGCWii()
        {
            return false;
        }

        protected override string idxr100repack()
        {
            return ".idxuur100repack";
        }

        protected override string idxscenario()
        {
            return ".idxuuscenario";
        }

        protected override string idxsmd()
        {
            return ".idxuusmd";
        }

        protected override string Get_MainTplBaseName(string baseFileName)
        {
            return "TPL";
        }

        protected override void CreateMtlIdxMaterialIdxuhdTpl(Dictionary<(string MaterialName, ushort MagicID), MaterialPart> materialWithTplFileIdDic,
             IdxMaterial idxMaterial, string baseDirectory, string fileBaseName, string[] TplNames, string mainTplFileBaseName)
        {
            IdxMtl _idxMtl = new IdxMtl() { MtlDic = new Dictionary<string, MtlObj>() };

            foreach (var item in UhdTplDic)
            {
                var temp_idxMaterial = new IdxMaterial()
                {
                    MaterialDic = materialWithTplFileIdDic.Where(a => a.Key.MagicID == item.Key).ToDictionary(k => k.Key.MaterialName, v => v.Value)
                };

                var temp_mtl = IdxMtlParser.Parser(temp_idxMaterial, item.Value, IsPS4NS);
                foreach (var mtlObj in temp_mtl.MtlDic)
                {
                    _idxMtl.MtlDic.Add(mtlObj.Key, mtlObj.Value);
                }

                // etapas para criar o idxuhdtpl com o nome correto.
                string idxuhdtplFileName = fileBaseName;
                if (item.Key != 0)
                {
                    idxuhdtplFileName += "." + item.Key.ToString("D");
                }

                int M_SMD_FileID = ((item.Key >> 8) - 1); // -1 é o main e shared
                int M_TPL_FIleID = (item.Key & 0xFF);

                if (M_SMD_FileID > -1)
                {
                    idxuhdtplFileName = TplNames[M_SMD_FileID] + "." + M_TPL_FIleID.ToString("D");
                }

                Console.WriteLine("Creating File: " + idxuhdtplFileName + ".idxuhdtpl");
                OutputMaterial.CreateIdxUhdTpl(item.Value, baseDirectory, idxuhdtplFileName);
            }
            _idxMtl.MtlDic = _idxMtl.MtlDic.OrderBy(a => a.Key).ToDictionary(K => K.Key, v => v.Value);

            Console.WriteLine("Creating File: " + fileBaseName + ".idxmaterial");
            OutputMaterial.CreateIdxMaterial(idxMaterial, baseDirectory, fileBaseName);

            Console.WriteLine("Creating File: " + fileBaseName + ".mtl");
            OutputMaterial.CreateMTL(_idxMtl, baseDirectory, fileBaseName);

        }

        protected override (string SubDirectoryTpl, string TplName) ReturnSubDirectoryTplAndTplBaseName(string baseDirectory, string FileName)
        {
            return (Path.Combine(baseDirectory, FileName), "TPL");
        }

        protected override void SetUhdTplDic(int inFileID, bool IsSharedFile)
        {

            if (TPL_Content_Dic.ContainsKey(inFileID))
            {
                if (IsSharedFile && TPL_Content_Dic[inFileID].UhdTplDic.ContainsKey(0))
                {
                    UhdTplDic.Add(0, TPL_Content_Dic[inFileID].UhdTplDic[0]);
                }
                else if (IsSharedFile == false)
                {
                    foreach (var item in TPL_Content_Dic[inFileID].UhdTplDic)
                    {
                        if (item.Key != 0)
                        {
                            ushort magicID = (ushort)(((inFileID + 1) * 0x01_00) + item.Key);

                            UhdTplDic.Add(magicID, item.Value);
                        }
                    }
                }
            }

        }

        protected override void Set_Extract_BIN_Inside_SMD(out Extract_BIN_Inside_SMD extract_BIN_Inside_SMD, out Extract_BIN_Content extract_BIN_Content)
        {
            extract_BIN_Inside_SMD = new Extract_BIN_Inside_SMD();
            extract_BIN_Content = new Extract_BIN_Content_UHD(endianness, IsPS4NS);
            extract_BIN_Inside_SMD.ToExtractBin = extract_BIN_Content.ToExtractBin;
        }

        protected override void Set_Extract_TPL_Inside_SMD(out Extract_TPL_Inside_SMD extract_TPL_Inside_SMD, int fileID)
        {
            extract_TPL_Inside_SMD = new Extract_TPL_Inside_SMD_UHD();
            Extract_TPL_Content_UHD extract_TPL_Content = new Extract_TPL_Content_UHD(endianness, IsPS4NS);
            extract_TPL_Inside_SMD.ToExtractTpl = extract_TPL_Content.ToExtractTpl;
            TPL_Content_Dic.Add(fileID, extract_TPL_Content);
        }

        protected override string GetStartGroupName()
        {
            return "UUSCENARIO";
        }
    }
}
