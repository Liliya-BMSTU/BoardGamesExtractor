using System;
using HGNot = BoardGamesExtractor.HobbyGames_Notation;

namespace BoardGamesExtractor
{
    public static class RangeRoutines
    {
        /// <summary>Converts a string into a range limited by [CONSTMINVAL, CONSTMAXVAL].
        /// Possible samples: "0-15" or "от 2 до 10" or "до 360" or "240+" or "свыше 361"
        /// </summary>
        /// <param name="value">an input string</param>
        /// <param name="CONSTMINVAL">the lower bound for min</param>
        /// <param name="CONSTMAXVAL">the upper bound for max</param>
        /// <param name="min">the lower bound of the extracted range</param>
        /// <param name="max">the upper bound of the extracted range</param>
        public static void ToRange(this string value, int CONSTMINVAL, int CONSTMAXVAL, out int min, out int max)
        {
            min = CONSTMINVAL;
            max = CONSTMAXVAL;
            string s = value.Trim();

            int pos2 = s.IndexOf(HGNot.GameParamsGE);
            if (pos2 >= 0)  // value like "18+"
            {
                string t = s.Substring(0, pos2).TrimEnd();
                try { min = Convert.ToInt32(t); }
                catch { min = CONSTMINVAL; }
            }
            else
            {
                pos2 = s.IndexOf(HGNot.GameParamsOver);
                if (pos2 >= 0)  // "свыше 100"
                {
                    string t = s.Substring(pos2 + HGNot.GameParamsOver.Length).TrimStart();
                    try { min = Convert.ToInt32(t); }
                    catch { min = CONSTMINVAL; }

                }
                else
                {
                    pos2 = s.IndexOf(HGNot.GameParamsSeparator);
                    if (pos2 >= 0)  // "2-10"
                    {
                        string t = s.Substring(0, pos2).TrimEnd();
                        try { min = Convert.ToInt32(t); }
                        catch { min = CONSTMINVAL; }
                        t = s.Substring(pos2 + HGNot.GameParamsSeparator.Length).TrimStart();
                        try { max = Convert.ToInt32(t); }
                        catch { max = CONSTMAXVAL; }
                    }
                    else              // an interval
                    {
                        pos2 = s.IndexOf(HGNot.GameParamsTo);
                        if (pos2 >= 0)
                        {
                            string t = s.Substring(pos2 + HGNot.GameParamsTo.Length).TrimStart();
                            try { min = Convert.ToInt32(t); }
                            catch { min = CONSTMINVAL; }
                                    
                            int pos = s.IndexOf(HGNot.GameParamsFrom);
                            if (pos >= 0)
                            {
                                // "  от 9 до"
                                //  0123456789
                                t = s.Substring(pos + HGNot.GameParamsFrom.Length,
                                    pos2 - pos - HGNot.GameParamsFrom.Length).Trim();
                                try { max = Convert.ToInt32(t); }
                                catch { max = CONSTMAXVAL; }
                            }
                        }
                        else
                        {
                            pos2 = s.IndexOf(HGNot.GameParamsFrom);
                            if (pos2 >= 0)  // "от 100"
                            {
                                string t = s.Substring(pos2 + HGNot.GameParamsFrom.Length).TrimStart();
                                int pos = s.IndexOf(HGNot.GameParamsTo);
                                if (pos >= 0)  // "от 100 до 500"
                                {
                                    string t2 = t.Substring(0, pos).Trim();
                                    try { min = Convert.ToInt32(t2); }
                                    catch { min = CONSTMINVAL; }
                                    t2 = t.Substring(pos + HGNot.GameParamsTo.Length).Trim();
                                    try { max = Convert.ToInt32(t2); }
                                    catch { max = CONSTMAXVAL; }
                                }
                                else
                                {
                                    try { min = Convert.ToInt32(t); }
                                    catch { min = CONSTMINVAL; }
                                }
                            }
                            else  // hence, a single value?..
                            {
                                try { min = max = Convert.ToInt32(s); }
                                catch { min = CONSTMINVAL; max = CONSTMAXVAL; }
                            }
                        }
                    }
                }
            }
        }

        public static int ToScaledParam(this string SpanString)
        {
            int res = 1, pos1, pos2, pos0 = 0, L = HGNot.SpanUnchecked.Length;
            //<span></span><span class="selected"></span><span></span><span></span><span></span>
            pos1 = SpanString.IndexOf(HGNot.SpanChecked);
            pos2 = SpanString.IndexOf(HGNot.SpanUnchecked, pos0);
            while ((pos2 < pos1) && (pos2 >= 0))
            {
                res++;
                pos0 = pos2 + L;
                pos1 = SpanString.IndexOf(HGNot.SpanChecked);
                pos2 = SpanString.IndexOf(HGNot.SpanUnchecked, pos0);
            }
            return res;
        }
    }
}
