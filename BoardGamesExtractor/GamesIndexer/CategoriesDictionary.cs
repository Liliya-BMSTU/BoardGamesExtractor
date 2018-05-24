using System;
using System.Collections.Generic;

namespace BoardGamesExtractor
{
    public static class Categories
    {
        public static List<string> GetDictionary(string FName, out bool res, out string msg)
        {
            int n;
            List<string> CD = FileIO.ReadStringList(FName, out n, out res, out msg);
            if (!res)
            {
                CD = new List<string>();
            }
            return CD;
        }

        public static bool IsACategory(this string s, List<string> CategoriesDict)
        {
            return CategoriesDict.Contains(s);
        }

        public static List<string> SelectCategories(List<string> FromTags, List<string> CategoriesDict)
        {
            List<string> res = new List<string>();
            int i, N = FromTags.Count;
            string s;
            for (i = 0; i < N; i++)
            {
                s = FromTags[i];
                if (s.IsACategory(CategoriesDict))
                    res.Add(s);
            }
            return res;
        }

        public static List<string> SelectThematic(List<string> FromTags, List<string> ThematicDict)
        {
            List<string> res = new List<string>();
            int i, N = FromTags.Count;
            string s;
            for (i = 0; i < N; i++)
            {
                s = FromTags[i];
                if (s.IsAThematic(ThematicDict))
                    res.Add(s);
            }
            return res;
        }

        public static List<string> SelectOptionalTags(List<string> FromTags, List<string> OptionsTagsDict)
        {
            List<string> res = new List<string>();
            int i, N = FromTags.Count;
            string s;
            for (i = 0; i < N; i++)
            {
                s = FromTags[i];
                if (s.IsAnOptionalTag(OptionsTagsDict))
                    res.Add(s);
            }
            return res;
        }

        public static List<string> SelectSeries(List<string> FromTags,
                                                List<string> CategoriesDict, List<string> BlackListDict, List<string> OptionsDict, List<string> ThematicDict)
        {
            List<string> res = new List<string>();
            int i, N = FromTags.Count;
            string s;
            for (i = 0; i < N; i++)
            {
                s = FromTags[i];
                if ((!s.IsACategory(CategoriesDict)) & (!s.IsInBlackList(BlackListDict)) & (!s.IsAnOptionalTag(OptionsDict)) & (!s.IsAThematic(ThematicDict)))
                    res.Add(s);
            }
            return res;
        }

        public static bool IsAnOptionalTag(this string FromTag, List<string> OptionsDict)
        {
            return OptionsDict.Contains(FromTag);
        }

        public static bool IsAThematic(this string FromTag, List<string> ThematicDict)
        {
            return ThematicDict.Contains(FromTag);
        }

        public static bool IsInBlackList(this string FromTag, List<string> BlackListDict)
        {
            return BlackListDict.Contains(FromTag);
        }

        public static bool IsInBlackList(this List<string> FromTags, List<string> BlackListDict)
        {
            bool res = false;
            int i, N = FromTags.Count;
            for (i = 0; i < N; i++)
                if (BlackListDict.Contains(FromTags[i]))
                {
                    res = true;
                    break;
                }
            return res;
        }

        public static void DropBlackList(this List<GameParams> From, List<string> BlackListDict, out int Dropped)
        {
            Dropped = 0;
            int i = 0, N = From.Count;
            while (i < N)
            {
                if (From[i].Tags.IsInBlackList(BlackListDict))
                {
                    From.RemoveAt(i);
                    i--;
                    N--;
                    Dropped++;
                }
                i++;
            }
        }

        public static void DropBlackList(this List<GameParams> From, List<string> BlackListDict, out int Dropped, List<int> CorruptIndices, out List<int> Undropped)
        {
            Dropped = 0;
            Undropped = new List<int>();
            List<int> OutIndices = new List<int>();
            int N = From.Count, i = N - 1;
            while (i >= 0)
            {
                if (From[i].Tags.IsInBlackList(BlackListDict))
                {
                    if (CorruptIndices.Contains(i))
                        OutIndices.Add(i);
                    From.RemoveAt(i);
                    N--;
                    Dropped++;
                }
                i--;
            }
            N = CorruptIndices.Count;
            for (i = 0; i < N; i++)
                if (!OutIndices.Contains(CorruptIndices[i]))
                    Undropped.Add(CorruptIndices[i]);
        }

        public static void ExpandThematic(this GameParams Game)
        {
            string des = Game.DesLongText;
            if (des.Contains("пират") || des.Contains("Пират") || des.Contains("pirat") || des.Contains("Pirat"))
                Game.Thematic.Add("Пираты");
            if (des.Contains("зомб") || des.Contains("Зомб") || des.Contains("zomb") || des.Contains("Zomb"))
                Game.Thematic.Add("Зомби");
            if (des.Contains("апокалип") || des.Contains("Апокалип") || des.Contains("apocalyp") || des.Contains("Apocalyp"))
                Game.Thematic.Add("Апокалипсис");
            if (des.Contains("стимпанк") || des.Contains("Стимпанк") || des.Contains("steampunk") || des.Contains("Steampunk"))
                Game.Thematic.Add("Стимпанк");
            if (des.Contains("детектив") || des.Contains("Детектив") || des.Contains("раскрыт") || des.Contains("Раскрыт")
                 || des.Contains("убий") || des.Contains("Убий") || des.Contains("detective") || des.Contains("Detective")
                 || des.Contains("murder") || des.Contains("Murder"))
                Game.Thematic.Add("Детектив");
            if (des.Contains("шпион") || des.Contains("Шпион") || des.Contains("spy") || des.Contains("Spy"))
                Game.Thematic.Add("Шпионы");
            if (des.Contains("рыцар") || des.Contains("Рыцар") || des.Contains("knight") || des.Contains("Knight"))
                Game.Thematic.Add("С рыцарями");
            if (des.Contains("средневек") || des.Contains("Средневек") || des.Contains("средние века") || des.Contains("Средние века")
                 || des.Contains("средних век") || des.Contains("Средних век") || des.Contains("средним века") || des.Contains("Средним века")
                 || des.Contains("средними века") || des.Contains("Средними века") || des.Contains("middle age") || des.Contains("Middle age"))
                Game.Thematic.Add("Средневековье");
        }
    }
}
