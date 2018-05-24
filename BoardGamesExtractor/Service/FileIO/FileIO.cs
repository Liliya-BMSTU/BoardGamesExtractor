using System;
using System.IO;
using System.Collections.Generic;

namespace BoardGamesExtractor
{
    public static partial class FileIO
    {   
        public const string sTxt = ".txt";

        private static char[] TabSyms { get { return new char[2] { ' ', '\t' }; } }

        /// <summary>Returns YYMMDDHHMMSS (this old method uses TimeStamping methods)</summary>
        public static string CurDateTime()
        {
            return TimeStamping.StringStamp_YYMMDD() + TimeStamping.StringStamp_HHMMSS();
        }

        public static string GetFolderChildName(string fName)
        {
            string res = fName;
            int i = fName.LastIndexOf("\\");
            if (i > 0)
                if (i == fName.Length - 1)
                {
                    res = fName.Substring(0, fName.Length - 1);
                    i = res.LastIndexOf("\\");
                    if (i > 0)
                        res = res.Substring(i + 1); 
                }
                else
                    res = res.Substring(i + 1);
            return res;
        }

        public static string GetFolderName(string fName)
        {
            string res = fName;
            int i = fName.LastIndexOf("\\");
            if (i > 0)
                res = res.Substring(0, i+1);
            return res;
        }

        public static bool FileExists(string fName, out bool res, out string msg)
        {
            bool fe = false;
            res = true; msg = "";
            try
            {
                fe = File.Exists(fName);
            }
            catch (Exception ex)
            {
                res = false; msg = "Ошибка проверки существования файла '" + fName + "' : " + ex.Message;
                fe = false;
                //Msgs.ShowMsg(5);
            }
            return fe;
        }
        public static bool FolderExists(string fPath, out bool res, out string msg)
        {
            bool fe = false;
            res = true; msg = "";
            try
            {
                fe = Directory.Exists(fPath);
            }
            catch (Exception ex)
            {
                res = false; msg = "Ошибка проверки существования папки '" + fPath + "' : " + ex.Message;
                fe = false;
                //Msgs.ShowMsg(5);
            }
            return fe;
        }

        public static void CreateDirectory(string fPath, bool isStraightPath, out bool res, out string msg)
        {
            res = true; msg = "";
            string s = fPath;
            if (!isStraightPath)
                s = s.Substring(0, s.LastIndexOf('\\') + 1);
            try
            {
                if (s.Trim() != "")
                    if (!Directory.Exists(s))
                        Directory.CreateDirectory(s);
            }
            catch (Exception ex)
            {
                res = false; msg = "Ошибка создания папки '" + fPath + "' : " + ex.Message;
                //Msgs.ShowMsg(5);
            }
        }

        public static string ShortName(string fullName)
        {
            string res = "";
            if (fullName.Length > 0)
            {
                int i = fullName.LastIndexOf('\\');
                res = (i > 0 & i < fullName.Length) ? i == fullName.Length - 1 ? "" : fullName.Substring(i + 1) : fullName;
            }
            return res;
        }

        public static List<int> ReadFileLinesToListInt(string fName, out int num, out bool res, out string msg)
        {
            num = 0;
            int x, i;
            string[] Lines = ReadFileLines(fName, out res, out msg);
            num = Lines.Length;
            List<int> OutList = new List<int>(num);
            if (res)
                for (i = 0; i < num; i++)
                    if (Lines[i].Trim() == "")
                            num--;
                        else
                            try
                            {
                                x = Convert.ToInt32(Lines[i]);
                                OutList.Add(x);
                            }
                            catch (Exception ex)
                            {
                                res = false; msg = "Error when reading int list from file: " + ex.Message;
                                break;
                            }
            return OutList;
        }

        private static string[] ReadFileLines(string fName, out bool res, out string msg)
        {
            res = true; msg = ""; string[] sarr = null;
            try { sarr = File.ReadAllLines(fName); }
            catch (Exception ex) { res = false; msg = "Error reading file (ReadFileLines): " + ex.Message; }
            return res ? sarr : null;
        }

        /// <summary>Read from file a bunch of string lines</summary>
        /// <param name="num">From file</param>
        public static List<string> ReadStringList(string fName, out int num, out bool res, out string msg)
        {
            string[] Lines = ReadAllLines(fName, out num, out res, out msg);
            List<string> Lst = new List<string>(num);
            for (int i = 0; i < num; i++)
                Lst.Add(Lines[i]);
            return Lst;
        }

        /// <summary>Read from file a bunch of string lines</summary>
        /// <param name="num">From file</param>
        public static string[] ReadAllLines(string fName, out int num, out bool res, out string msg)
        {
            string[] Lines = ReadFileLines(fName, out res, out msg);
            num = Lines.Length;
            return Lines;
        }
        /// <summary>
        /// Запись строки в файл
        /// </summary>
        /// <param name="fName">имя файла</param>
        /// <param name="data1">string</param>
        public static void WriteData(string fName, string data1,
                                        out bool res, out string msg)
        {
            res = true; msg = "";
            CreateDirectory(fName, false, out res, out msg);
            FileStream fs = null; StreamWriter tw = null;
            try
            {
                fs = new FileStream(fName, FileMode.OpenOrCreate);
                fs.Seek(0, SeekOrigin.End);
                tw = new StreamWriter(fs);
                tw.WriteLine(data1);
            }
            catch (Exception ex)
            {
                res = false; msg = "Error when writing data (WriteData, 2str): " + ex.Message;
            }
            finally
            {
                if (tw != null) try { tw.Close(); } catch { }
                if (fs != null) try { fs.Close(); } catch { }
            }
        }
        /// <summary>
        /// Запись строк в файл
        /// </summary>
        /// <param name="fName">имя файла</param>
        /// <param name="data1">List&lt;string&gt;</param>
        public static void WriteData(string fName, List<string> data1,
                                        out bool res, out string msg)
        {
            res = true; msg = "";
            CreateDirectory(fName, false, out res, out msg);
            FileStream fs = null; StreamWriter tw = null;
            int i = -1, n = data1.Count;
            try
            {
                fs = new FileStream(fName, FileMode.OpenOrCreate);
                fs.Seek(0, SeekOrigin.End);
                tw = new StreamWriter(fs);
                for (i = 0; i < n; i++)
                    tw.WriteLine(data1[i]);
            }
            catch (Exception ex)
            {
                res = false; msg = "Error when writing data (WriteData, lst)" +
                    (i == -1 ? "" : "[line #" + i.ToString() + "]") + ": " + ex.Message;
            }
            finally
            {
                if (tw != null) try { tw.Close(); }
                    catch { }
                if (fs != null) try { fs.Close(); }
                    catch { }
            }
        }

        public static void ClearFile(string fName, out bool res, out string msg)
        {
            res = true; msg = "";
            FileStream fs = null;
            try
            {
                CreateDirectory(fName, false, out res, out msg);
                try { fs = new FileStream(fName, FileMode.Truncate); }
                catch { fs = new FileStream(fName, FileMode.OpenOrCreate); }
            }
            catch (Exception ex)
            {
                res = false; msg = "Error when clearing file: " + ex.Message;
            }
            finally
            {
                if (fs != null) try { fs.Close(); } catch { }
            }
        }
    }
}
