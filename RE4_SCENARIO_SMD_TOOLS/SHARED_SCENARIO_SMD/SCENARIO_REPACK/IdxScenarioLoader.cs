using SHARED_TOOLS.ALL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_REPACK
{
    public static class IdxScenarioLoader
    {
        public static IdxScenario Loader(Stream idxStream)
        {
            IdxScenario idxScenario = new IdxScenario();

            Dictionary<(int fileID, int smdID), SMDLineIdx> ExtraSmdLinesDic = new Dictionary<(int fileID, int smdID), SMDLineIdx>();
            Dictionary<(int fileID, int smdID), SMDLineIdxPart2> ExtraSmdLinesPart2Dic = new Dictionary<(int fileID, int smdID), SMDLineIdxPart2>();

            Dictionary<int, SMDLineIdx> SmdLinesDic = new Dictionary<int, SMDLineIdx>();
            Dictionary<int, SMDLineIdxPart2> SmdLinesPart2Dic = new Dictionary<int, SMDLineIdxPart2>();
            Dictionary<int, uint> ExtraParametersDic = new Dictionary<int, uint>();
            Dictionary<int, string> ExtraSmdFileNameDic = new Dictionary<int, string>();
            Dictionary<int, string> ExtraTplFileNameDic = new Dictionary<int, string>();

            SMDLineIdx tempLine = new SMDLineIdx();
            SMDLineIdxPart2 tempPart2 = new SMDLineIdxPart2();

            StreamReader reader = new StreamReader(idxStream, Encoding.ASCII);

            while (!reader.EndOfStream)
            {
                string lineCaseSensitive = reader?.ReadLine()?.Trim();
                string line = lineCaseSensitive?.ToUpperInvariant();

                if (line == null
                    || line.Length == 0
                    || line.StartsWith("\\")
                    || line.StartsWith("/")
                    || line.StartsWith("#")
                    || line.StartsWith(":")
                    || line.StartsWith("!")
                    || line.StartsWith("@")
                    || line.StartsWith("=")
                    )
                {
                    continue;
                }
                else if (line.StartsWith("SMDFILENAME"))
                {
                    var split = lineCaseSensitive.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            string value = split[1].Replace('\\', '/')
                             .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                             .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                            value = value.Split('\\').Last();

                            if (value.Length == 0)
                            {
                                value = "null";
                            }

                            idxScenario.SmdFileName = Path.GetFileNameWithoutExtension(value) + ".SMD";
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else if (line.StartsWith("SHAREDFILENAME"))
                {
                    var split = lineCaseSensitive.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            string value = split[1].Replace('\\', '/')
                             .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                             .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                            value = value.Split('\\').Last();

                            if (value.Length == 0)
                            {
                                value = "null";
                            }

                            idxScenario.SharedFileName = Path.GetFileNameWithoutExtension(value) + ".SMD";
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else if (line.StartsWith("TPLFILENAME"))
                {
                    var split = lineCaseSensitive.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            string value = split[1].Replace('\\', '/')
                             .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                             .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                            value = value.Split('/').Last();

                            if (value.Length == 0)
                            {
                                value = "null";
                            }

                            idxScenario.TplFileName = Path.GetFileNameWithoutExtension(value) + ".TPL";
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else if (line.StartsWith("BINFOLDER"))
                {
                    var split = lineCaseSensitive.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            string value = split[1].Replace('\\', '/')
                             .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                             .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                            value = value.Split('/').Last();

                            if (value.Length == 0)
                            {
                                value = "null";
                            }

                            idxScenario.BinFolder = value;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else if (line.StartsWith("EXTRASMDFILENAME_")) 
                {
                    var split = lineCaseSensitive.Split(':');
                    if (split.Length >= 2) 
                    {
                        var keysplit = split[0].ToUpperInvariant().Split('_');
                        if (keysplit.Length >=2)
                        {
                            int ID = -1;
                            try
                            {
                                ID = int.Parse(Utils.ReturnValidDecValue(keysplit[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                            }
                            catch (Exception)
                            {
                            }

                            if (ID > -1 && ID < LimitConsts.ExtraSmdFileLimit && ExtraSmdFileNameDic.ContainsKey(ID) == false)
                            {
                                string fileName = "null.SMD";
                                try
                                {
                                    string value = split[1].Replace('\\', '/')
                                     .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                                     .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                                    value = value.Split('\\').Last();

                                    if (value.Length == 0)
                                    {
                                        value = "null";
                                    }

                                    fileName = Path.GetFileNameWithoutExtension(value) + ".SMD";
                                }
                                catch (Exception)
                                {
                                }
                                ExtraSmdFileNameDic.Add(ID, fileName);
                            }

                        }
                    }

                }
                else if (line.StartsWith("EXTRATPLFILENAME_"))
                {
                    var split = lineCaseSensitive.Split(':');
                    if (split.Length >= 2)
                    {
                        var keysplit = split[0].ToUpperInvariant().Split('_');
                        if (keysplit.Length >= 2)
                        {
                            int ID = -1;
                            try
                            {
                                ID = int.Parse(Utils.ReturnValidDecValue(keysplit[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                            }
                            catch (Exception)
                            {
                            }

                            if (ID > -1 && ID < LimitConsts.ExtraSmdFileLimit && ExtraTplFileNameDic.ContainsKey(ID) == false)
                            {
                                string fileName = "null.TPL";
                                try
                                {
                                    string value = split[1].Replace('\\', '/')
                                     .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                                     .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                                    value = value.Split('\\').Last();

                                    if (value.Length == 0)
                                    {
                                        value = "null";
                                    }

                                    fileName = Path.GetFileNameWithoutExtension(value) + ".TPL";
                                }
                                catch (Exception)
                                {
                                }
                                ExtraTplFileNameDic.Add(ID, fileName);
                            }

                        }
                    }
                }
                else if (line.StartsWith("EXTRAPARAMETER_"))
                {
                    var split = lineCaseSensitive.Split(':');
                    if (split.Length >= 2)
                    {
                        var keysplit = split[0].ToUpperInvariant().Split('_');
                        if (keysplit.Length >= 2)
                        {
                            int ID = -1;
                            try
                            {
                                ID = int.Parse(Utils.ReturnValidDecValue(keysplit[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                            }
                            catch (Exception)
                            {
                            }

                            if (ID > -1 && ID < LimitConsts.ExtraSmdFileLimit && ExtraSmdFileNameDic.ContainsKey(ID) == false)
                            {
                                uint value = 0;
                                try
                                {
                                    value = uint.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                }
                                ExtraParametersDic.Add(ID, value);
                            }
                        }
                    }

                }
                else if (line.StartsWith("SMD_"))
                {
                    tempLine = new SMDLineIdx();
                    tempLine.ScaleX = 1;
                    tempLine.ScaleY = 1;
                    tempLine.ScaleZ = 1;

                    tempPart2 = new SMDLineIdxPart2();
                    tempPart2.FixedFF = 0xFF;
                    tempPart2.SmxID = 0xFE;

                    var split = line.Split('_');
                    if (split.Length >= 2)
                    {
                        int ID = -1;
                        try
                        {
                            ID = int.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }

                        if (ID > -1 && ID < LimitConsts.SmdLineLimit && !SmdLinesDic.ContainsKey(ID))
                        {
                            SmdLinesDic.Add(ID, tempLine);
                            SmdLinesPart2Dic.Add(ID, tempPart2);
                        }
                    }
                }
                else if (line.StartsWith("FILE_"))
                {
                    tempLine = new SMDLineIdx();
                    tempLine.ScaleX = 1;
                    tempLine.ScaleY = 1;
                    tempLine.ScaleZ = 1;

                    tempPart2 = new SMDLineIdxPart2();
                    tempPart2.FixedFF = 0xFF;
                    tempPart2.SmxID = 0xFE;

                    var split = line.Split('_');
                    if (split.Length >= 2)
                    {
                        int fileID = -1;
                        try
                        {
                            fileID = byte.Parse(Utils.ReturnValidDecValue(split[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }

                        int smdID = -1;
                        try
                        {
                            smdID = byte.Parse(Utils.ReturnValidDecValue(split[3]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                        }

                        if (fileID > -1 && fileID < LimitConsts.ExtraSmdFileLimit && smdID > -1 && smdID < LimitConsts.SmdLineLimit && !ExtraSmdLinesDic.ContainsKey((fileID, smdID)))
                        {
                            ExtraSmdLinesDic.Add((fileID, smdID), tempLine);
                            ExtraSmdLinesPart2Dic.Add((fileID, smdID), tempPart2);
                        }
                    }
                }
                else
                {
                    _ = Utils.SetBoolean(ref line, "USEIDXMATERIAL", ref idxScenario.UseIdxMaterial)
                     || Utils.SetBoolean(ref line, "USEIDXUHDTPL", ref idxScenario.UseIdxUhdTpl)
                     || Utils.SetBoolean(ref line, "ENABLEVERTEXCOLOR", ref idxScenario.EnableVertexColor)
                     || Utils.SetBoolean(ref line, "ENABLEDINAMICVERTEXCOLOR", ref idxScenario.EnableDinamicVertexColor)
                     || Utils.SetBoolean(ref line, "IGNOREFIRSTTPLFILE", ref idxScenario.IgnoreFirstTplFile)
                     || Utils.SetUshortHex(ref line, "MAGIC", ref idxScenario.Magic)

                     || Utils.SetFloatDec(ref line, "POSITIONX", ref tempLine.PositionX)
                     || Utils.SetFloatDec(ref line, "POSITIONY", ref tempLine.PositionY)
                     || Utils.SetFloatDec(ref line, "POSITIONZ", ref tempLine.PositionZ)

                     || Utils.SetFloatDec(ref line, "ANGLEX", ref tempLine.AngleX)
                     || Utils.SetFloatDec(ref line, "ANGLEY", ref tempLine.AngleY)
                     || Utils.SetFloatDec(ref line, "ANGLEZ", ref tempLine.AngleZ)

                     || Utils.SetFloatDec(ref line, "SCALEX", ref tempLine.ScaleX)
                     || Utils.SetFloatDec(ref line, "SCALEY", ref tempLine.ScaleY)
                     || Utils.SetFloatDec(ref line, "SCALEZ", ref tempLine.ScaleZ)

                     || Utils.SetByteDec(ref line, "BINFILEID", ref tempPart2.BinFileID)
                     || Utils.SetByteDec(ref line, "TPLFILEID", ref tempPart2.TplFileID)
                     || Utils.SetByteDec(ref line, "SMXID", ref tempPart2.SmxID)
                     || Utils.SetByteHex(ref line, "FIXEDFF", ref tempPart2.FixedFF)
                     || Utils.SetUintHex(ref line, "OBJECTSTATUS", ref tempPart2.ObjectStatus)
                     || Utils.SetUintHex(ref line, "UNUSED1", ref tempPart2.Unused1)
                     || Utils.SetUintHex(ref line, "UNUSED2", ref tempPart2.Unused2)
                     || Utils.SetUintHex(ref line, "UNUSED3", ref tempPart2.Unused3)
                     || Utils.SetUintHex(ref line, "UNUSED4", ref tempPart2.Unused4)
                     || Utils.SetUintHex(ref line, "UNUSED5", ref tempPart2.Unused5)
                     || Utils.SetUintHex(ref line, "UNUSED6", ref tempPart2.Unused6)
                     || Utils.SetUintHex(ref line, "UNUSED7", ref tempPart2.Unused7)
                     ;
                }

            }

            idxScenario.SmdLinesDic = SmdLinesDic;
            idxScenario.SmdLinesPart2Dic = SmdLinesPart2Dic;
            idxScenario.ExtraParametersDic = ExtraParametersDic;
            idxScenario.ExtraSmdFileNameDic = ExtraSmdFileNameDic;
            idxScenario.ExtraTplFileNameDic = ExtraTplFileNameDic;
            idxScenario.ExtraSmdLinesPart2Dic = ExtraSmdLinesPart2Dic;
            idxScenario.ExtraSmdLinesDic = ExtraSmdLinesDic;

            idxStream.Close();

            return idxScenario;
        }

    }


    public class IdxScenario
    {
        public string SmdFileName = "null.SMD";
        public string TplFileName = "null.TPL"; // only GcWii
        public string BinFolder = "null";

        public Dictionary<int, SMDLineIdx> SmdLinesDic;

        // only in .idx__scenario
        public bool UseIdxMaterial = false;
        public bool UseIdxUhdTpl = false; // only uhd
        public bool EnableVertexColor = false;
        public bool EnableDinamicVertexColor = false;

        // only in .idx__smd
        public Dictionary<int, SMDLineIdxPart2> SmdLinesPart2Dic;
        public ushort Magic = 0x0040;
        public Dictionary<int, uint> ExtraParametersDic;
        public bool IgnoreFirstTplFile = false;  // only GcWii

        // only in .idx__r100repack
        public string SharedFileName = "shared.SMD";
        public Dictionary<int, string> ExtraSmdFileNameDic;
        public Dictionary<int, string> ExtraTplFileNameDic; // only GcWii
        public Dictionary<(int fileID, int smdID), SMDLineIdx> ExtraSmdLinesDic;
        public Dictionary<(int fileID, int smdID), SMDLineIdxPart2> ExtraSmdLinesPart2Dic;
    }

    public class SMDLineIdx
    {
        public float PositionX;
        public float PositionY;
        public float PositionZ;

        public float AngleX;
        public float AngleY;
        public float AngleZ;

        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
    }

    public class SMDLineIdxPart2
    {
        // only in .idx__smd
        public byte BinFileID;
        public byte TplFileID;
        public byte FixedFF;
        public byte SmxID;
        public uint Unused1;
        public uint Unused2;
        public uint Unused3;
        public uint Unused4;
        public uint Unused5;
        public uint Unused6;
        public uint Unused7;
        public uint ObjectStatus;
    }

}