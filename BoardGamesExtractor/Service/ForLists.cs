using System;
using System.Collections.Generic;

namespace BoardGamesExtractor
{
    public static class ForLists
    {
        /// <summary>Column index in triangle matrix above main diagonal</summary>
        /// <param name="idx">normal index (greater than row!)</param>
        /// <param name="row">row index</param>
        public static int TriIdx(int idx, int row) { return idx - row - 1; }

        private const string FSep = "\t";//KeysForFiles.IDSeparator;
        
        /// <summary>Converting list to string with \t separator</summary>
        public static string LstToString(this List<int> L)
        {
            int i, n = L.Count;
            string res = n > 0 ? L[0].ToString() : "";
            for (i = 1; i < n; i++)
                res += '\t' + L[i].ToString();
            return res;
        }
        public static string LstToString(this List<int> L, char sep)
        {
            string res = "";
            int i, n = L.Count;
            if (n > 0)
                res = L[0].ToString();
            for (i = 1; i < n; i++)
                res += sep + L[i].ToString();
            return res;
        }
        public static string LstToSepIDChain(this List<int> L)
        {
            string res = "";
            int i, n = L.Count;
            if (n > 0)
                res = L[0].ToString();
            for (i = 1; i < n; i++)
                res += FSep + L[i].ToString();
            return res;
        }
        
        /// <summary>Converting list to string with \t separator</summary>
        public static string LstToString(this List<double> L)
        {
            int i, n = L.Count;
            string res = n > 0 ? L[0].ToString("F") : "";
            for (i = 1; i < n; i++)
                res += '\t' + L[i].ToString("F");
            return res;
        }

        /// <summary>Converting list to string with \t separator</summary>
        public static string LstToString(this List<bool> L)
        {
            int i, n = L.Count;
            string res = n > 0 ? (L[0] ? "1" : "0") : "";
            for (i = 1; i < n; i++)
                res += '\t' + (L[i] ? "1" : "0");
            return res;
        }
        public static string LstToString(this List<bool> L, char sep)
        {
            string res = "";
            int i, n = L.Count;
            if (n > 0)
                res = L[0] ? "1" : "0";
            for (i = 1; i < n; i++)
                res += sep + (L[i] ? "1" : "0");
            return res;
        }

        /// <summary>returns false if any unit is present in both lists</summary>
        /// <param name="L1">not empty</param>
        /// <param name="L2">not empty</param>
        public static bool ListsAreOfUniqueUnits(this List<int> L1, List<int> L2)
        {
            bool res = true;
            int i, n = L1.Count;
            for (i = 0; i < n & res; i++)
                res &= !L2.Contains(L1[i]);
            return res;
        }
        
        public static void DumpTriangleToFile(this List<List<int>> L, int dim, string fName,
                                            out bool res, out string msg)
        {
            res = true; msg = "";
            int i, j, m;
            string s;
            FileIO.ClearFile(fName, out res, out msg);
            for (i = 0; i < dim; i++)
            {
                s = "";
                m = dim - i - 1;
                for (j = 0; j <= i; j++)
                    s += '\t';
                for (j = 0; j < m; j++)
                    s += L[i][j].ToString() + '\t';
                FileIO.WriteData(fName, s, out res, out msg);
            }
        }
        public static void DumpTriangleToFile(this List<List<int>> L, int dim, string fName,
                                            string header, out bool res, out string msg)
        {
            res = true; msg = "";
            int i, j, m;
            string s;
            FileIO.ClearFile(fName, out res, out msg);
            FileIO.WriteData(fName, header, out res, out msg);
            for (i = 0; i < dim; i++)
            {
                s = "";
                m = dim - i - 1;
                for (j = 0; j <= i; j++)
                    s += '\t';
                for (j = 0; j < m; j++)
                    s += L[i][j].ToString() + '\t';
                FileIO.WriteData(fName, s, out res, out msg);
            }
        }

        /// <summary>Produces a new list with links to old one's items, but to the ID item (why not removing?)</summary>
        public static List<int> HitItem(this List<int> L, int ID)
        {
            int i, n = L.Count;
            List<int> res = new List<int>(n - 1);
            for (i = 0; i < n; i++)
                if (L[i] != ID)
                    res.Add(L[i]);
            return res;
        }

        /*public static bool LstContains2Way(this List<List<int>> L, List<int> val)
        {
            int i, j, n = L.Count, m = val.Count;
            bool res = true;
            if (n > 0 & m > 0)
            {
                for (i = 0; i < n & res; i++)
                    if (L[i].Count == m)
                    {
                        for (j = 0; j < m & res; j++)
                            if (L[i][j] != val[j])
                                res = false;
                        res = !res;
                        if (res)
                        {
                            for (j = 0; j < m & res; j++)
                                if (L[i][j] != val[m - j - 1])
                                    res = false;
                            res = !res;
                        }
                    }
            }
            return !res;
        }*/
        public static bool LstContains1Way(this List<List<int>> L, List<int> val)
        {
            int i, j, n = L.Count, m = val.Count;
            bool res = true;
            if (n > 0 & m > 0)
            {
                for (i = 0; i < n & res; i++)
                    if (L[i].Count == m)
                    {
                        for (j = 0; j < m & res; j++)
                            if (L[i][j] != val[j])
                                res = false;
                        res = !res;
                    }
            }
            return !res;
        }

        public static List<List<int>> Transpon(this List<List<int>> L, int dim1, int dim2)
        {
            int i, j;
            List<List<int>> res = new List<List<int>>();
            for (i = 0; i < dim2; ++i)
            {
                res.Add(new List<int>());
                for (j = 0; j < dim1; ++j)
                    res[i].Add(L[j][i]);
            }
            return res;
        }

        public static List<int> ConcatWith(this List<int> L1, ref List<int> L2)
        {
            List<int> res = L1.Copy();
            res.AddRange(L2.Copy());
            return res;
        }

        public static List<int> ToList(this int[] arr, int cnt)
        {
            List<int> res = new List<int>(cnt);
            for (int i = 0; i < cnt; i++)
                res.Add(arr[i]);
            return res;
        }

        public static List<List<int>> ToList(this List<int>[] arr, int cnt)
        {
            List<List<int>> res = new List<List<int>>(cnt);
            for (int i = 0; i < cnt; i++)
                res.Add(arr[i]);
            return res;
        }

        public static bool AddIntToIdx(this List<int> L, int Lcnt, int cand)
        {
            bool res = true;
            if (Lcnt == 0)
                L.Add(cand);
            else
            {
                bool b = true;
                int i;
                for (i = 0; i < Lcnt & b; i++)
                    if (cand == L[i])
                        res = b = false;
                    else
                        if (cand < L[i])
                        {
                            L.Insert(i, cand);
                            b = false;
                        }
                if (i == Lcnt & b)
                    L.Add(cand);
            }
            return res;
        }

        public static bool AddStringToIdx(this List<string> L, ref int Lcnt, string cand)
        {
            bool res = false;
            if (Lcnt == 0)
            {
                L.Add(cand);
                res = true;
            }
            else
            {
                bool isEq, b = true;
                int i, len = cand.Length;
                for (i = 0; i < Lcnt & b; i++)
                {
                    res = cand.IsLeftFrom(L[i], len, out isEq);
                    if (isEq)
                    {
                        res = b = false;
                    }
                    else
                    {
                        if (res)
                        {
                            L.Insert(i, cand);
                            b = false;
                        }
                    }
                }
                if (i == Lcnt & b)
                {
                    L.Add(cand);
                    res = true;
                }
            }
            if (res)
                Lcnt++;
            return res;
        }

        public static bool AddStringToIdx(this List<string> L, ref int Lcnt, string cand, ref List<int> Counter)
        {
            bool res = false;
            if (Lcnt == 0)
            {
                L.Add(cand);
                Counter.Add(1);
                res = true;
            }
            else
            {
                bool isEq, b = true;
                int i, len = cand.Length;
                for (i = 0; i < Lcnt & b; i++)
                {
                    res = cand.IsLeftFrom(L[i], len, out isEq);
                    if (isEq)
                    {
                        res = b = false;
                        Counter[i]++;
                    }
                    else
                    {
                        if (res)
                        {
                            L.Insert(i, cand);
                            Counter.Insert(i, 1);
                            b = false;
                        }
                    }
                }
                if (i == Lcnt & b)
                {
                    L.Add(cand);
                    Counter.Add(1);
                    res = true;
                }
            }
            if (res)
                Lcnt++;
            return res;
        }

        /// <summary>Determines whether s1 is on the left from s2 in
        /// alphabetical order (for ex., 'ab' &lt; 'abc' &lt; 'abd')</summary>
        public static bool IsLeftFrom(this string s1, string s2, out bool isEq)
        {
            int i = 0, len1 = s1.Length, len2 = s2.Length;
            isEq = len1 == len2;
            bool res = true;
            // case 0: len(s1) = len(s2) - maybe EQ, maybe not
            if (isEq)
            {
                res = len1 > 0;
                if (res)
                    while (isEq)
                    {
                        res = s1[i] == s2[i];
                        if (res)
                        {
                            i++;
                            isEq = i < len1;
                        }
                        else
                        {
                            res = s1[i] < s2[i];
                            isEq = false;
                        }
                    }
                isEq = i == len1; // strings ends reached
            }
            else
                // case 1: s1 = '', s2 > '', result is true
                if (len1 == 0)
                    res = true;
                else
                // case 2: s1 > '', s2 = '', result is false
                    if (len2 == 0)
                        res = false;
                    else
                // case 3|else: len(s1) != len(s2), both > 0 - not EQ
                    {
                        int t = len1 < len2 ? len1 : len2;
                        isEq = true;
                        while (isEq)
                        {
                            res = s1[i] == s2[i];
                            if (res)
                            {
                                i++;
                                isEq = i < t;
                                if (!isEq)  // one string's end reached, it's the left one's
                                    res = i == len1;
                            }
                            else
                            {
                                res = s1[i] < s2[i];
                                isEq = false;
                            }
                        }
                    }
            return res;
        }
        /// <summary>Determines whether s1 is on the left from s2 in
        /// alphabetical order (for ex., 'ab' &lt; 'abc' &lt; 'abd')</summary>
        public static bool IsLeftFrom(this string s1, string s2, int s1size, out bool isEq)
        {
            int i = 0, len1 = s1size, len2 = s2.Length;
            isEq = len1 == len2;
            bool res = true;
            // case 0: len(s1) = len(s2) - maybe EQ, maybe not
            if (isEq)
            {
                res = len1 > 0;
                if (res)
                    while (isEq)
                    {
                        res = s1[i] == s2[i];
                        if (res)
                        {
                            i++;
                            isEq = i < len1;
                        }
                        else
                        {
                            res = s1[i] < s2[i];
                            isEq = false;
                        }
                    }
                isEq = i == len1; // strings ends reached
            }
            else
                // case 1: s1 = '', s2 > '', result is true
                if (len1 == 0)
                    res = true;
                else
                // case 2: s1 > '', s2 = '', result is false
                    if (len2 == 0)
                        res = false;
                    else
                // case 3|else: len(s1) != len(s2), both > 0 - not EQ
                    {
                        int t = len1 < len2 ? len1 : len2;
                        isEq = true;
                        while (isEq)
                        {
                            res = s1[i] == s2[i];
                            if (res)
                            {
                                i++;
                                isEq = i < t;
                                if (!isEq)  // one string's end reached, it's the left one's
                                    res = i == len1;
                            }
                            else
                            {
                                res = s1[i] < s2[i];
                                isEq = false;
                            }
                        }
                    }
            return res;
        }

        public static List<int> GetTopValuesIdxs(this List<double> Lst, int LstSize, int NumTopVals)
        {
            List<int> Top = new List<int>(NumTopVals);
            if (NumTopVals > 0 & LstSize > 0)
            {
                int i, j;
                if (NumTopVals == 1)
                {
                    // Get Maximum
                    j = 0;  // already LstSize >= 0
                    for (i = 1; i < LstSize; i++)
                        if (Lst[i] > Lst[j])
                            j = i;
                    Top.Add(j);
                }
                else
                {
                    // Get Top Values
                    bool b;
                    int N;
                    Top.Add(0);
                    if (LstSize >= NumTopVals)
                        N = NumTopVals;
                    else
                        N = LstSize;
                    for (i = 1; i < N; i++)
                    {
                        b = true;
                        for (j = i - 1; j >= 0 & b; j--)
                            if (Lst[i] <= Lst[Top[j]])
                            {
                                b = false;
                                Top.Insert(j + 1, i);
                            }
                        if ((j == -1) & b)
                            Top.Insert(0, i);
                    }
                    N = NumTopVals - 2;
                    for (i = NumTopVals; i < LstSize; i++)
                        if (Lst[i] > Lst[Top[N + 1]])
                        {
                            b = true;
                            for (j = N; j >= 0 & b; j--)
                                if (Lst[i] <= Lst[Top[j]])
                                {
                                    b = false;
                                    Top.Insert(j + 1, i);
                                    Top.RemoveAt(N + 1);
                                }
                            if ((j == -1) & b)
                            {
                                Top.Insert(0, i);
                                Top.RemoveAt(N + 1);
                            }
                        }
                }
            }
            return Top;
        }

        public static List<int> CopyReverse(this List<int> L, bool doReverse)
        {
            List<int> res;
            if (doReverse)
            {
                int i, n = L.Count, t = n - 1;
                res = new List<int>();
                for (i = 0; i < n; i++)
                    res.Add(L[t - i]);
            }
            else
                res = L.Copy();
            return res;
        }
        public static List<List<int>> CopyReverse(this List<List<int>> L, bool doReverse)
        {
            List<List<int>> res;
            if (doReverse)
            {
                int i, j, m, n = L.Count, t;
                res = new List<List<int>>();
                for (i = 0; i < n; i++)
                {
                    m = L[i].Count;
                    t = m - 1;
                    if (m > 0)
                        res.Add(new List<int>());
                    for (j = 0; j < m; j++)
                        res[i].Add(L[i][t - j]);
                }
            }
            else
                res = L.Copy();
            return res;
        }

        public static List<int> InvertedPath(this List<int> L)
        {
            List<int> res = new List<int>();
            int i;
            for (i = L.Count - 1; i >= 0; i--)
                res.Add(L[i]);
            return res;
        }

        /// <summary>Retrieving amount of unique items in list</summary>
        public static int DeDuplicatedAmount(this int[] L)
        {
            int NUnique = L.Length;
            int i, j, N = NUnique;
            for (i = 0; i < N; i++)
            {
                for (j = i + 1; j < N; j++)
                {
                    if (L[i] == L[j])
                    {
                        NUnique--;
                        break;
                    }
                }
            }
            return NUnique;
        }

        /// <summary>Retrieving amount of unique items in list</summary>
        public static int DeDuplicatedAmount(this List<int> L)
        {
            int NUnique = L.Count;
            int i, j, N = NUnique;
            for (i = 0; i < N; i++)
            {
                for (j = i + 1; j < N; j++)
                {
                    if (L[i] == L[j])
                    {
                        NUnique--;
                        break;
                    }
                }
            }
            return NUnique;
        }
        
        /// <summary>Of course, overloaded</summary>
        public static List<int> Copy(this List<int> L)
        {
            int i, n = L.Count;
            List<int> res = new List<int>(n);
            for (i = 0; i < n; i++)
                res.Add(L[i]);
            return res;
        }

        /// <summary>Of course, overloaded</summary>
        public static List<List<int>> Copy(this List<List<int>> L)
        {
            int i, j, m, n = L.Count;
            List<List<int>> res = new List<List<int>>(n);
            for (i = 0; i < n; i++)
            {
                m = L[i].Count;
                if (m > 0)
                    res.Add(new List<int>(m));
                for (j = 0; j < m; j++)
                    res[i].Add(L[i][j]);
            }
            return res;
        }

        /// <summary>Of course, overloaded</summary>
        public static List<List<List<List<int>>>> Copy(this List<List<List<List<int>>>> L)
        {
            int i, j, n, m;
            n = L.Count;
            List<List<List<List<int>>>> res = new List<List<List<List<int>>>>(n);
            for (i = 0; i < n; i++)
            {
                m = L[i].Count;
                res.Add(new List<List<List<int>>>(m));
                for (j = 0; j < m; j++)
                {
                    res[i].Add(L[i][j].Copy());
                }
            }
            return res;
        }

        /// <summary>Of course, overloaded</summary>
        public static List<bool> Copy(this List<bool> L)
        {
            int i, n = L.Count;
            List<bool> res = new List<bool>(n);
            for (i = 0; i < n; i++)
                res.Add(L[i]);
            return res;
        }

        /// <summary>Of course, overloaded</summary>
        public static List<double> Copy(this List<double> L)
        {
            int i, n = L.Count;
            List<double> res = new List<double>(n);
            for (i = 0; i < n; i++)
                res.Add(L[i]);
            return res;
        }

        /// <summary>Of course, overloaded</summary>
        public static List<double> CopyToDouble(this List<int> L)
        {
            int i, n = L.Count;
            List<double> res = new List<double>(n);
            for (i = 0; i < n; i++)
                res.Add(L[i]);
            return res;
        }

        /// <summary>Of course, overloaded</summary>
        public static List<decimal> Copy(this List<decimal> L)
        {
            int i, n = L.Count;
            List<decimal> res = new List<decimal>(n);
            for (i = 0; i < n; i++)
                res.Add(L[i]);
            return res;
        }

        /// <summary>Of course, overloaded</summary>
        public static List<string> Copy(this List<string> L)
        {
            int i, n = L.Count;
            List<string> res = new List<string>(n);
            for (i = 0; i < n; i++)
                res.Add(L[i]);
            return res;
        }
    }
}
