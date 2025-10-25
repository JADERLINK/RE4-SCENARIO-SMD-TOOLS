using SHARED_SCENARIO_SMD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SHARED_GCWII_SCENARIO_SMD
{
    public static class MainAction
    {
        public static void MainContinue(string[] args)
        {
            Func<FileInfo, string, bool> SMD_EXTRACT = (fileinfo, extension) =>
            {
                if (extension == ".SMD")
                {
                    Start_SMD_Extract.SMD_Extract(fileinfo);
                    return true;
                }
                return false;
            };

            Func<FileInfo, string, bool> SHD_EXTRACT = (fileinfo, extension) =>
            {
                if (extension == ".SHD")
                {
                    Start_SHD_Extract.SHD_Extract(fileinfo);
                    return true;
                }
                return false;
            };

            Func<FileInfo, string, bool> R100_EXTRACT = (fileinfo, extension) =>
            {
                if (extension == ".IDXR100EXTRACT")
                {
                    Start_R100_Extract.R100Extract(fileinfo);
                    return true;
                }
                return false;
            };

            Func<FileInfo, string, bool> IDX__SMD = (fileinfo, extension) =>
            {
                if (extension == ".IDXGGSMD")
                {
                    Start_IdxSMD_Repack.IdxSMD_Repack(fileinfo);
                    return true;
                }
                return false;
            };

            Func<FileInfo, string, bool> IDX__SCENARIO = (fileinfo, extension) =>
            {
                if (extension == ".IDXGGSCENARIO")
                {
                    Start_ScenarioOBJ_Repack.ScenarioOBJ_Repack(fileinfo, false, false);
                    return true;
                }
                return false;
            };

            Func<FileInfo, string, bool> IDX__SHD = (fileinfo, extension) =>
            {
                if (extension == ".IDXGGSHD")
                {
                    Start_ScenarioOBJ_Repack.ScenarioOBJ_Repack(fileinfo, false, true);
                    return true;
                }
                return false;
            };

            Func<FileInfo, string, bool> R100_REPACK = (fileinfo, extension) =>
            {
                if (extension == ".IDXGGR100REPACK")
                {
                    Start_ScenarioOBJ_Repack.ScenarioOBJ_Repack(fileinfo, true, false);
                    return true;
                }
                return false;
            };


            StartMain start = new StartMain();
            start.FileFormatList.Add(SMD_EXTRACT);
            start.FileFormatList.Add(SHD_EXTRACT);
            start.FileFormatList.Add(R100_EXTRACT);
            start.FileFormatList.Add(IDX__SMD);
            start.FileFormatList.Add(IDX__SCENARIO);
            start.FileFormatList.Add(IDX__SHD);
            start.FileFormatList.Add(R100_REPACK);
            start.Continue(args);
        }
    }
}
