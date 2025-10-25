using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SHARED_SCENARIO_SMD.SCENARIO_EXTRACT.R100
{
    public static class R100_Extract
    {
        //validação dos arquivos que estão no arquivo idxr100extract
        public static string[] ValidateIdxR100Extract(IdxR100Extract idxextract, string baseDirectory) 
        {
            string[] files = new string[idxextract.PartFiles.Length + 2];
            for (int i = 0; i < idxextract.PartFiles.Length; i++)
            {
                files[i] = idxextract.PartFiles[i];
            }
            files[files.Length - 2] = idxextract.SharedFile;
            files[files.Length - 1] = idxextract.MainFile;

            Console.WriteLine("SMD files:");
            for (int fil = 0; fil < files.Length; fil++)
            {
                Console.WriteLine("FILE:" + files[fil]);
                string smdpath = Path.Combine(baseDirectory, files[fil]);
                if (!File.Exists(smdpath))
                {
                    string error = "Error the file does not exist: " + files[fil];
                    throw new ApplicationException(error);
                }
            }

            return files;
        }

        public static void CommonBINcheck(ref int commonBinAmount, SMDLine[] smdLines)
        {
            for (int i = 0; i < smdLines.Length; i++)
            {
                SMDLine smdLine = smdLines[i];
                int BinID = smdLine.BinFileID;
                if (smdLine.IsSharedBIN())
                {
                    if (commonBinAmount <= BinID)
                    {
                        commonBinAmount = BinID + 1;
                    }
                }
            }
        }

        // converter os varios SMD, para um so SMD
        public static Dictionary<(int file, int binID), int> ConverterToSingleSMD(
            SMDLine[][] smdLinesList,
            Dictionary<int, GenericModelBIN>[] modelList,
            out SMDLine[] newSmdLines, 
            out Dictionary<int, GenericModelBIN> binList,
            int sharedFileID,
            int mainFileID)
        {
            int Length = 0;
            for (int o = 0; o < smdLinesList.Length; o++)
            {
                if (o != sharedFileID)
                {
                    Length += smdLinesList[o].Length;
                }
            }

            newSmdLines = new SMDLine[Length];
            binList = new Dictionary<int, GenericModelBIN>();

            // key (id do arquivo, seu bin, "IsSharedBin"), novo endereço do bin
            Dictionary<(int file, int binID, bool IsSharedBin), ushort> NewBinIdDic = new Dictionary<(int file, int binID, bool IsSharedBin), ushort>();

            Dictionary<(int file, int binID), int> returnNewBinIdDic = new Dictionary<(int file, int binID), int>();

            ushort newBinCounter = 0;

            //adiciono na lista os bins do arquivo shared.smd sharedFileID
            foreach (var binID in modelList[sharedFileID].Keys)
            {
                var key = (sharedFileID, binID, false);
                if (!NewBinIdDic.ContainsKey(key))
                {
                    returnNewBinIdDic.Add((sharedFileID, binID), binID);

                    NewBinIdDic.Add(key, (ushort)binID);
                    if (newBinCounter < binID)
                    {
                        newBinCounter = (ushort)binID;
                    }
                }
            }
            newBinCounter++;

            // arquivo main.smd mainFileID

            for (int i = 0; i < smdLinesList[mainFileID].Length; i++)
            {
                bool IsSharedBin = smdLinesList[mainFileID][i].IsSharedBIN();
                var keyA = (mainFileID, smdLinesList[mainFileID][i].BinFileID);
                var keyB = (mainFileID, smdLinesList[mainFileID][i].BinFileID, IsSharedBin);
                if (!NewBinIdDic.ContainsKey(keyB))
                {
                    if (IsSharedBin)
                    {
                        NewBinIdDic.Add(keyB, smdLinesList[mainFileID][i].BinFileID);
                    }
                    else
                    {
                        returnNewBinIdDic.Add(keyA, newBinCounter);
                        NewBinIdDic.Add(keyB, newBinCounter);
                        newBinCounter++;
                    }
                }

            }

            // smd dentro dos .dat
            for (int fil = 0; fil < smdLinesList.Length -2; fil++)
            {
                for (int i = 0; i < smdLinesList[fil].Length; i++)
                {
                    bool IsSharedBin = smdLinesList[fil][i].IsSharedBIN();
                    var keyA = (fil, smdLinesList[fil][i].BinFileID);
                    var keyB = (fil, smdLinesList[fil][i].BinFileID, IsSharedBin);

                    if (!NewBinIdDic.ContainsKey(keyB))
                    {
                        if (IsSharedBin)
                        {
                            NewBinIdDic.Add(keyB, smdLinesList[fil][i].BinFileID);
                        }
                        else
                        {
                            returnNewBinIdDic.Add(keyA, newBinCounter);
                            NewBinIdDic.Add(keyB, newBinCounter);
                            newBinCounter++;
                        }
                    }
                }
            }

            //-----------------------
            int smdCounter = 0;

            // cria o novo newSmdLines
            for (int fil = smdLinesList.Length - 1; fil >= 0; fil--)
            {
                if (fil != sharedFileID)
                {
                    for (int i = 0; i < smdLinesList[fil].Length; i++)
                    {
                        var newLine = smdLinesList[fil][i].Clone();
                        bool isBinShared = smdLinesList[fil][i].IsSharedBIN();
                        newLine.BinFileID = (byte)NewBinIdDic[(fil, smdLinesList[fil][i].BinFileID, isBinShared)];
                        newLine.ObjectStatus = smdLinesList[fil][i].ObjectStatus & 0xFF_FF_FF_8F;
                        newSmdLines[smdCounter] = newLine;
                        smdCounter++;
                    }
                }
            }

            //------------------
            // cria o novo binList
            foreach (var item in NewBinIdDic)
            {
                if (item.Key.IsSharedBin == false)
                {
                    if (modelList[item.Key.file].ContainsKey(item.Key.binID))
                    {
                        if (!binList.ContainsKey(item.Value))
                        {
                            binList.Add(item.Value, modelList[item.Key.file][item.Key.binID]);
                        }
                    }
                }

            }

            return returnNewBinIdDic;
        }

    }

}
