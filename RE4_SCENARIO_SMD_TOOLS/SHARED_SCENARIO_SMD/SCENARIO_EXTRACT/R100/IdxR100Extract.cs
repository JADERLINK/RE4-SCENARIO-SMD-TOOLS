using SHARED_TOOLS.ALL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.R100
{
    public class IdxR100Extract
    {
        public string[] PartFiles = new string[0];
        public string SharedFile = "";
        public string MainFile = "";
    }

    public static class IdxR100ExtractLoader 
    {

        public static IdxR100Extract Loader(Stream idxFile) 
        {
            IdxR100Extract idx = new IdxR100Extract();

            Dictionary<int, string> files = new Dictionary<int, string>();
            int lastFileKey = -1;

            StreamReader reader = new StreamReader(idxFile, Encoding.UTF8);

            while (!reader.EndOfStream)
            {
                string line = reader?.ReadLine()?.Trim()?.ToUpperInvariant();

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
                else if (line.StartsWith("SHAREDFILE"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            string value = split[1].ToLower().Replace('\\', '/')
                             .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                             .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                            value = value.Split('\\').Last();

                            if (value.Length == 0)
                            {
                                value = "null";
                            }

                            idx.SharedFile = Path.GetFileNameWithoutExtension(value) + ".SMD";
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else if (line.StartsWith("MAINFILE"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        try
                        {
                            string value = split[1].ToLower().Replace('\\', '/')
                             .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                             .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                            value = value.Split('/').Last();

                            if (value.Length == 0)
                            {
                                value = "null";
                            }

                            idx.MainFile = Path.GetFileNameWithoutExtension(value) + ".SMD";
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                else if (line.StartsWith("FILE_"))
                {
                    var split = line.Split(':');
                    if (split.Length >= 2)
                    {
                        var keySplit = split[0].Split('_');

                        if (keySplit.Length >= 2)
                        {
                            int key = -1;
                            string file = "";
                            try
                            {
                                key = int.Parse(Utils.ReturnValidDecValue(keySplit[1]), NumberStyles.Integer, CultureInfo.InvariantCulture);
                            }
                            catch (Exception)
                            {

                            }

                            try
                            {
                                string value = split[1].ToLower().Replace('\\', '/')
                                 .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                                 .Replace("<", "").Replace(">", "").Replace("?", "").Replace(" ", "_");

                                value = value.Split('/').Last();

                                if (value.Length == 0)
                                {
                                    value = "null";
                                }

                                file = Path.GetFileNameWithoutExtension(value) + ".SMD";
                            }
                            catch (Exception)
                            {
                            }

                            if (key > -1 && !files.ContainsKey(key) && file.Length != 0)
                            {
                                files.Add(key, file);

                                if (key > lastFileKey && key < 32)
                                {
                                    lastFileKey = key;
                                }
                            }
                        }
                      
                    }
                }
            }

            string[] PartFiles = new string[lastFileKey + 1];

            for (int i = 0; i < PartFiles.Length; i++)
            {
                if (files.ContainsKey(i))
                {
                    PartFiles[i] = files[i];
                }
                else 
                {
                    PartFiles[i] = "null.SMD";
                }
            }
            idx.PartFiles = PartFiles;

            return idx;
        }


    }
}
