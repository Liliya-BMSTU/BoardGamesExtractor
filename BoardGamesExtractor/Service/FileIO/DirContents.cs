using System;
using System.IO;
using System.Collections.Generic;

namespace BoardGamesExtractor
{
    public static partial class FileIO
    {
        public static List<string> GetDirFiles(string dirPath, bool doAddPaths, out bool res, out string msg)
        {
            List<string> L = null;
            res = true;
            msg = "";
            if (FileIO.FolderExists(dirPath, out res, out msg))
                try
                {
                    // Make a reference to a directory.
                    DirectoryInfo di = new DirectoryInfo(dirPath);
                    // Get a reference to each file in that directory.
                    FileInfo[] fiArr = di.GetFiles();
                    int m = fiArr.Length;
                    if (dirPath[dirPath.Length - 1] != '\\')
                        dirPath += '\\';
                    string prefix = doAddPaths ? dirPath : "";
                    L = new List<string>(m);
                    for (int i = 0; i < m; i++)
                    {
                        L.Add(prefix + fiArr[i].Name);
                    }
                }
                catch (Exception ex)
                {
                    res = false;
                    msg = "Getting directory files failed: " + ex.Message;
                }
            else
            {
                if (res)
                {
                    res = false; msg = "Directory doesn't exist.";
                }
                else
                {
                    msg = "Error while checking directory existence: " + msg;
                }
            }
            return L;
        }

        public static void DumpDirFiles(string dirPath, bool doRewrite, bool doAddPaths, bool doWriteNumber,
                                        out bool res, out string msg)
        {
            string fName = "Contents.txt";
            List<string> L = GetDirFiles(dirPath, doAddPaths, out res, out msg);
            if (res)
            {
                int N = L.Count;
                if (dirPath[dirPath.Length - 1] != '\\')
                    dirPath += '\\';
                fName = dirPath + fName;
                if (doRewrite)
                {
                    FileIO.ClearFile(fName, out res, out msg);
                    if (!res)
                        msg = "Error while clearing file '" + fName + "'";
                    if (doWriteNumber)
                        FileIO.WriteData(fName, N.ToString(), out res, out msg);
                }
                if (res)
                {
                    bool AllRes = true;
                    for (int i = 0; i < N; i++)
                    {
                        FileIO.WriteData(fName, L[i], out res, out msg);
                        AllRes &= res;
                    }
                    res = AllRes;
                    if (!res)
                        msg = "Error while writing to file '" + fName + "'";
                }
            }
        }
    }
}