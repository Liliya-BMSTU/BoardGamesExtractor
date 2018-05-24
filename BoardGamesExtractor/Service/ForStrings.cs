using System;
using System.Collections.Generic;

namespace BoardGamesExtractor
{
    public static class ForStrings
    {
        public const string EndLine = "\n\r";

        public static bool IsEqual(this string sOrig, string sTarg)
        {
            bool f = true;
            int i, N = sOrig.Length;
            if (N == sTarg.Length)
            {
                for (i = 0; (i < N) & f; i++)
                    f &= sOrig[i] == sTarg[i];
            }
            else
            {
                f = false;
            }
            return f;
        }
        
        /// <summary>Extract all fields from s, using Splitters</summary>
        public static string[] ExtractFields(this string s, char[] Splitters)
        {
            return s.Split(Splitters);
        }

        /// <summary>Extract fields #idx from s, using Splitters</summary>
        public static string ExtractField(this string s, int idx, char[] Splitters)
        {
            string res = "";
            string[] Fields = s.Split(Splitters);
            if (Fields.Length > idx)
                res = Fields[idx];
            return res;
        }

        /// <summary>Extract fields #idx from s, using Splitters</summary>
        public static int ExtractFieldInt(this string s, int idx, char[] Splitters)
        {
            int x = -1;
            string res = "";
            string[] Fields = s.Split(Splitters);
            if (Fields.Length > idx)
            {
                res = Fields[idx];
                try { x = Convert.ToInt32(res); }
                catch { x = -1; }
            }
            return x;
        }

        public static bool Contains(this char[] arr, char value)
        {
            bool res = false;
            int i, N = arr.Length;
            for (i = 0; i < N; i++)
                if (arr[i] == value)
                {
                    res = true;
                    break;
                }
            return res;
        }

        public static bool ContainsSubs(this string s, string subs)
        {
            bool res = false;
            if (subs == "")
                res = true;
            else
            {
                int i, j, M = subs.Length, N = s.Length - M;
                for (i = 0; i <= N; i++)
                {
                    j = 0;
                    if (s[i] == subs[j])
                    {
                        res = true;
                        for (j = 1; j < M; j++)
                            res &= s[i + j] == subs[j];
                    }
                    if (res)
                        break;
                }
            }
            return res;
        }

        public static bool ContainsSubs(this string s, string subs, bool SearchFromEnd)
        {
            bool res = false;
            if (subs == "")
                res = true;
            else
            {
                int i, j, M = subs.Length, N = s.Length - M;
                if (SearchFromEnd)
                    for (i = N; i >= 0; i--)
                    {
                        j = 0;
                        if (s[i] == subs[j])
                        {
                            res = true;
                            for (j = 1; j < M; j++)
                                res &= s[i + j] == subs[j];
                        }
                        if (res)
                            break;
                    }
                else
                    for (i = 0; i <= N; i++)
                    {
                        j = 0;
                        if (s[i] == subs[j])
                        {
                            res = true;
                            for (j = 1; j < M; j++)
                                res &= s[i + j] == subs[j];
                        }
                        if (res)
                            break;
                    }
            }
            return res;
        }

        /// <summary>Returns a string with replaced field #idx, using Splitters. If idx is bigger than present, Substitute is placed in the target position in the end.</summary>
        /// <param name="idx">0-based index to insert at</param>
        public static string ReplaceField(this string s, int idx, string Substitute, char[] Splitters)
        {
            int x = 0, pos = 0;
            string res = "";
            string[] Fields = s.Split(Splitters);
            if ((s == "") | (Fields.Length <= idx))
            {
                res += s;
                //x = s.TrimEnd(Splitters).Length;
                //s = s.TrimEnd(Splitters) + (Splitters.Length == 0 ? '\t' : Splitters[0]) + Substitute + s.Substring(x);
                char c = (Splitters.Length == 0 ? '\t' : Splitters[0]);
                for (x = Fields.Length - (Fields.Length == 1 ? 1 : 0); x < idx; x++)
                    res += c;
                res += Substitute.ToString();
            }
            else
            {
                for (x = 0; x <= idx; x++)
                {
                    pos = s.IndexOf(Fields[x], pos) + (x == idx ? 0 : Fields[x].Length);
                }
                // output: pos of target field
                string s1 = pos == 0 ? "" : s.Substring(0, pos);
                res = s1 + Substitute + s.Substring(pos + Fields[idx].Length);
            }
            return res;
        }

        /// <summary>Returns a string with replaced field #idx from s, using Splitters. If idx is bigger than present, Substitute is placed in the target position in the end.</summary>
        /// <param name="idx">0-based index to insert at</param>
        public static string ReplaceFieldInt(this string s, int idx, int Substitute, char[] Splitters)
        {
            int x = 0, pos = 0;
            string res = "";
            string[] Fields = s.Split(Splitters);
            if ((s == "") | (Fields.Length <= idx))
            {
                res += s;
                //x = s.TrimEnd(Splitters).Length;
                //s = s.TrimEnd(Splitters) + (Splitters.Length == 0 ? '\t' : Splitters[0]) + Substitute.ToString() + s.Substring(x);
                char c = (Splitters.Length == 0 ? '\t' : Splitters[0]);
                for (x = Fields.Length - (Fields.Length == 1 ? 1 : 0); x < idx; x++)
                    res += c;
                res += Substitute.ToString();
            }
            else
            {
                for (x = 0; x <= idx; x++)
                {
                    if (Fields[x].Length > 0)
                        pos = s.IndexOf(Fields[x], pos) + (x == idx ? 0 : Fields[x].Length);
                    else
                        pos++;
                }
                // output: pos of target field
                string s1 = pos == 0 ? "" : s.Substring(0, pos);
                res = s1 + Substitute.ToString() + s.Substring(pos + Fields[idx].Length);
            }
            return res;
        }

        public static string RemoveZeroField(this string s, char[] Splitters)
        {
            string res = "";
            string[] Fields = s.Split(Splitters);
            int N = Fields.Length;
            if (N > 0)
                if (N > 1)
                {
                    N = s.IndexOf(Fields[1], Fields[0].Length);
                    res = s.Substring(N);
                }
            return res;
        }

        public static string ReplaceWithIndexOfLength(this string src, string target, uint index, int numsymbols)
        {
            int Len = 0;
            uint z = index;
            if (z == 0)
                Len = 1;
            else
                while (z != 0)
                {
                    Len++;
                    z /= 10;
                }
            string replacer = "";
            while (Len < numsymbols)
            {
                replacer += '0';
                numsymbols--;
            }
            replacer += index.ToString();
            return src.Replace(target, replacer);
        }

        public static string IndexOfLength(this uint index, int numsymbols)
        {
            int Len = 0;
            uint z = index;
            if (z == 0)
                Len = 1;
            else
                while (z != 0)
                {
                    Len++;
                    z /= 10;
                }
            string replacer = "";
            while (Len < numsymbols)
            {
                replacer += '0';
                numsymbols--;
            }
            replacer += index.ToString();
            return replacer;
        }

        public static string IndexOfLength(this int index, int numsymbols)
        {
            int Len = 0;
            int z = Math.Abs(index);
            if (z == 0)
                Len = 1;
            else
                while (z != 0)
                {
                    Len++;
                    z /= 10;
                }
            string replacer = index >= 0 ? "" : "-";
            if (index < 0)
                numsymbols--;
            while (Len < numsymbols)
            {
                replacer += '0';
                numsymbols--;
            }
            z = Math.Abs(index);
            replacer += z.ToString();
            return replacer;
        }
    }
}
