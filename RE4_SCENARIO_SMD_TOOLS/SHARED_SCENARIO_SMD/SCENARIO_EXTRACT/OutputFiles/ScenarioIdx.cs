using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using SHARED_TOOLS.ALL;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.OutputFiles
{
    public static class ScenarioIdx
    {
        private static void PrintMagicInIDX(TextWriter text, SmdMagic smdMagic)
        {
            if (smdMagic.magic != 0x0040)
            {
                text.WriteLine("Magic:" + smdMagic.magic.ToString("X4"));
            }

            if (smdMagic.extraParameters.Length != 0)
            {
                for (int i = 0; i < smdMagic.extraParameters.Length; i++)
                {
                    text.WriteLine($"ExtraParameter_{i}:" + smdMagic.extraParameters[i]);
                }
            }
        }


        public static void CreateIdxScenario(string idxFullName, SMDLine[] smdLines, SmdMagic smdMagic, string binFolder, string SmdFileName, 
            bool IsGcWii = false, string TplFileName = "")
        {
            TextWriter text = new FileInfo(idxFullName).CreateText();
            text.WriteLine(SHARED_TOOLS.Shared.HeaderText());
            text.WriteLine("");

            PrintMagicInIDX(text, smdMagic);
            text.WriteLine("SmdFileName:" + SmdFileName);
            if (IsGcWii)
            {
                text.WriteLine("TplFileName:" + TplFileName);
            }
            text.WriteLine("BinFolder:" + binFolder); 
            if (!IsGcWii)
            {
                text.WriteLine("UseIdxUhdTpl:false");
            }
            text.WriteLine("UseIdxMaterial:false");
            text.WriteLine("EnableVertexColor:false");
            text.WriteLine("EnableDinamicVertexColor:true");

            text.WriteLine("");
            text.WriteLine("");

            for (int i = 0; i < smdLines.Length; i++)
            {
                text.WriteLine("SMD_" + i.ToString("D3"));
                CreateIdxScenario_Parts(ref text, smdLines[i]);
                CreateIdxScenario_TplFileID(ref text, smdLines[i], IsGcWii);
                text.WriteLine("");
                text.WriteLine("");
            }

            text.Close();


        }

        private static void CreateIdxScenario_Parts(ref TextWriter text, SMDLine smdLine)
        {
            string positionX = (smdLine.PositionX / CONSTs.GLOBAL_POSITION_SCALE).ToFloatString();
            string positionY = (smdLine.PositionY / CONSTs.GLOBAL_POSITION_SCALE).ToFloatString();
            string positionZ = (smdLine.PositionZ / CONSTs.GLOBAL_POSITION_SCALE).ToFloatString();
            text.WriteLine("PositionX:" + positionX);
            text.WriteLine("PositionY:" + positionY);
            text.WriteLine("PositionZ:" + positionZ);

            string angleX = (smdLine.AngleX).ToFloatString();
            string angleY = (smdLine.AngleY).ToFloatString();
            string angleZ = (smdLine.AngleZ).ToFloatString();
            text.WriteLine("AngleX:" + angleX);
            text.WriteLine("AngleY:" + angleY);
            text.WriteLine("AngleZ:" + angleZ);

            string scaleX = (smdLine.ScaleX).ToFloatString();
            string scaleY = (smdLine.ScaleY).ToFloatString();
            string scaleZ = (smdLine.ScaleZ).ToFloatString();
            text.WriteLine("ScaleX:" + scaleX);
            text.WriteLine("ScaleY:" + scaleY);
            text.WriteLine("ScaleZ:" + scaleZ);
        }

        private static void CreateIdxScenario_TplFileID(ref TextWriter text, SMDLine smdLine, bool IsGcWii) 
        {
            if (IsGcWii && smdLine.TplFileID != 0)
            {
                text.WriteLine("TplFileID:" + smdLine.TplFileID);
            }
        }

        public static void CreateIdxSmd(string idxFullName, SMDLine[] smdLines, SmdMagic smdMagic, string binFolder, string SmdFileName,
            bool IsGcWii = false, string TplFileName = "", bool IgnoreFirstTplFile = false)
        {
            TextWriter text = new FileInfo(idxFullName).CreateText();
            text.WriteLine(SHARED_TOOLS.Shared.HeaderText());
            text.WriteLine("");

            PrintMagicInIDX(text, smdMagic);
            text.WriteLine("SmdFileName:" + SmdFileName);
            if (IsGcWii)
            {
                text.WriteLine("TplFileName:" + TplFileName);
                if (IgnoreFirstTplFile)
                {
                    text.WriteLine("IgnoreFirstTplFile:" + IgnoreFirstTplFile);
                }
            }
            text.WriteLine("BinFolder:" + binFolder);

            text.WriteLine("");
            text.WriteLine("");

            for (int i = 0; i < smdLines.Length; i++)
            {
                text.WriteLine("SMD_" + i.ToString("D3"));
                CreateIdxScenario_Parts(ref text, smdLines[i]);
                CreateIdxSmd_Parts(ref text, smdLines[i]);

                text.WriteLine("");
                text.WriteLine("");
            }

            text.Close();

        }


        private static void CreateIdxSmd_Parts(ref TextWriter text, SMDLine smdLine) 
        {
            text.WriteLine("BinFileID:" + smdLine.BinFileID);
            text.WriteLine("TplFileID:" + smdLine.TplFileID);
            text.WriteLine("SmxID:" + smdLine.SmxID);

            if (smdLine.FixedFF != 0xFF)
            {
                text.WriteLine("FixedFF:" + smdLine.FixedFF.ToString("X2"));
            }

            text.WriteLine("ObjectStatus:" + smdLine.ObjectStatus.ToString("X2"));

            if (smdLine.Unused1 != 0)
            {
                text.WriteLine("Unused1:" + smdLine.Unused1.ToString("X8"));
            }

            if (smdLine.Unused2 != 0)
            {
                text.WriteLine("Unused2:" + smdLine.Unused2.ToString("X8"));
            }

            if (smdLine.Unused3 != 0)
            {
                text.WriteLine("Unused3:" + smdLine.Unused3.ToString("X8"));
            }

            if (smdLine.Unused4 != 0)
            {
                text.WriteLine("Unused4:" + smdLine.Unused4.ToString("X8"));
            }

            if (smdLine.Unused5 != 0)
            {
                text.WriteLine("Unused5:" + smdLine.Unused5.ToString("X8"));
            }

            if (smdLine.Unused6 != 0)
            {
                text.WriteLine("Unused6:" + smdLine.Unused6.ToString("X8"));
            }

            if (smdLine.Unused7 != 0)
            {
                text.WriteLine("Unused7:" + smdLine.Unused7.ToString("X8"));
            }

        }


        public static void CreateIdxShd(string idxFullName, SMDLine[] smdLines, SmdMagic smdMagic,
          bool IsGcWii = false, string TplFileName = "")
        {
            TextWriter text = new FileInfo(idxFullName).CreateText();
            text.WriteLine(SHARED_TOOLS.Shared.HeaderText());
            text.WriteLine("");

            PrintMagicInIDX(text, smdMagic);
            if (IsGcWii)
            {
                text.WriteLine("TplFileName:" + TplFileName);
                text.WriteLine("UseIdxMaterial:false");
            }
            text.WriteLine("EnableVertexColor:false");
            text.WriteLine("EnableDinamicVertexColor:true");

            text.WriteLine("");
            text.WriteLine("");

            for (int i = 0; i < smdLines.Length; i++)
            {
                text.WriteLine("SMD_" + i.ToString("D3"));
                CreateIdxScenario_Parts(ref text, smdLines[i]);
                CreateIdxScenario_TplFileID(ref text, smdLines[i], IsGcWii);
                text.WriteLine("");
                text.WriteLine("");
            }

            text.Close();

        }


        public static void CreateIdxR100Repack(string idxFullName, SMDLine[][] smdLinesList, string[] smdFiles, int sharedFileId, int mainFileID,
            bool IsGcWii = false, string TplFileName = "", string[] tplFiles = null)
        {
            TextWriter text = new FileInfo(idxFullName).CreateText();
            text.WriteLine(SHARED_TOOLS.Shared.HeaderText());
            text.WriteLine("");


            text.WriteLine("SmdFileName:" + smdFiles[mainFileID]);
            text.WriteLine("SharedFileName:" + smdFiles[sharedFileId]);

            for (int fileId = 0; fileId < smdFiles.Length - 2; fileId++)
            {
                text.WriteLine("ExtraSmdFileName_" + fileId + ":" + smdFiles[fileId]);
            }

            if (IsGcWii)
            {
                text.WriteLine("TplFileName:" + TplFileName);

                if (tplFiles != null)
                {
                    for (int fileId = 0; fileId < tplFiles.Length - 2; fileId++)
                    {
                        text.WriteLine("ExtraTplFileName_" + fileId + ":" + tplFiles[fileId]);
                    }
                }
            }

            if (!IsGcWii)
            {
                text.WriteLine("UseIdxUhdTpl:false");
            }

            text.WriteLine("UseIdxMaterial:false");
            text.WriteLine("EnableVertexColor:false");
            text.WriteLine("EnableDinamicVertexColor:true");

            text.WriteLine("");
            text.WriteLine("");

            for (int i = 0; i < smdLinesList[mainFileID].Length; i++)
            {
                text.WriteLine("SMD_" + i.ToString("D3"));
                CreateIdxScenario_Parts(ref text, smdLinesList[mainFileID][i]);
                CreateIdxScenario_TplFileID(ref text, smdLinesList[mainFileID][i], IsGcWii);
                text.WriteLine("");
                text.WriteLine("");
            }

            for (int o = 0; o < smdLinesList.Length -2; o++)
            {
                for (int i = 0; i < smdLinesList[o].Length; i++)
                {
                    text.WriteLine("FILE_"+ o.ToString("D2") +"_SMD_" + i.ToString("D3"));
                    CreateIdxScenario_Parts(ref text, smdLinesList[o][i]);
                    CreateIdxScenario_TplFileID(ref text, smdLinesList[o][i], IsGcWii);
                    text.WriteLine("");
                    text.WriteLine("");
                }
            }

            text.Close();
        }

    }

}
