using System;
using HGNot = BoardGamesExtractor.HobbyGames_Notation;

namespace BoardGamesExtractor
{
    /// <summary>Age range (enum), convertible to years (int) and backwards</summary>
    public enum AgeRangeTag { undef = -1, age_3_5, age_6_7, age_8_12, age_13_15, age_16_17, age_over_18
                                // now those which are missing at the main search webpage
                                , age_over_12 }

    /// <summary>Класс, который хранит интервал возраста игроков (есть значения по дефолту, есть кастование из входной строки и из перечисления AgeRangeTag)</summary>
    public class AgeRange
    {
        public const int MINVALUE = 3;    // though it might as well be 0
        public const int MAXVALUE = 255; // though it might as well be int.MaxValue

        public string RawText;
        public AgeRangeTag Tag;
        public bool HasAgeRangeTag;
        public int MinAge;
        public int MaxAge;

        public AgeRange()
        {
            RawText = "";
            Tag = AgeRangeTag.undef;
            HasAgeRangeTag = false;
            MinAge = MINVALUE;
            MaxAge = MAXVALUE;
        }

        public AgeRange(string rawText)
        {
            MinAge = MINVALUE;
            MaxAge = MAXVALUE;
            Tag = AgeRangeTag.undef;
            HasAgeRangeTag = false;
            RawText = rawText;

            int pos = RawText.IndexOf(HGNot.GameParamsAgeTitleText);
            if (pos >= 0)
            {
                RawText = RawText.Substring(pos + HGNot.GameParamsAgeTitleText.Length).TrimStart();
            }
            pos = RawText.IndexOf(HGNot.GameParamsAgePostfix);
            if (pos >= 0)
            {
                RawText = RawText.Substring(0, pos).Trim();
            }
            // now there can be "0-15" or "от 2 до 10" or "до 360" or "240+"

            RawText.ToRange(MINVALUE, MAXVALUE, out MinAge, out MaxAge);
        }

        public AgeRange(AgeRangeTag trTag)
        {
            Tag = trTag;
            HasAgeRangeTag = true;
            RawText = "";
            switch (trTag)
            {
                case AgeRangeTag.undef:
                    MinAge = MINVALUE;
                    MaxAge = MAXVALUE;
                    break;
                case AgeRangeTag.age_3_5:
                    MinAge = 3;
                    MaxAge = 5;
                    break;
                case AgeRangeTag.age_6_7:
                    MinAge = 6;
                    MaxAge = 7;
                    break;
                case AgeRangeTag.age_8_12:
                    MinAge = 8;
                    MaxAge = 12;
                    break;
                case AgeRangeTag.age_13_15:
                    MinAge = 13;
                    MaxAge = 15;
                    break;
                case AgeRangeTag.age_16_17:
                    MinAge = 16;
                    MaxAge = 17;
                    break;
                case AgeRangeTag.age_over_18:
                    MinAge = 18;
                    MaxAge = MAXVALUE;
                    break;
                case AgeRangeTag.age_over_12:
                    MinAge = 12;
                    MaxAge = MAXVALUE;
                    break;
                default:
                    MinAge = MINVALUE;
                    MaxAge = MAXVALUE;
                    break;
            }
        }
    }

    public static class AgeRangeRoutines
    {
        // ToDo: Question is, what do I have to do with following rules in case age is unspecified? (undef)

        /// <summary>Returns true if age is 3--5</summary>
        public static bool IsAgeForPreschool(this AgeRangeTag value)
        {
            return (value == AgeRangeTag.age_3_5);
        }
        /// <summary>Returns true if age is 3--5</summary>
        public static bool IsAgeForPreschool(this AgeRange value)
        {
            return (value.Tag == AgeRangeTag.age_3_5);
        }

        /// <summary>Returns true if age is 6--12</summary>
        public static bool IsAgeForKids(this AgeRangeTag value)
        {
            return (value == AgeRangeTag.age_6_7) || (value == AgeRangeTag.age_8_12);
        }
        /// <summary>Returns true if age is 6--12</summary>
        public static bool IsAgeForKids(this AgeRange value)
        {
            return (value.Tag == AgeRangeTag.age_6_7) || (value.Tag == AgeRangeTag.age_8_12);
        }

        /// <summary>Returns true if age is 13--17</summary>
        public static bool IsAgeForYouth(this AgeRangeTag value)
        {
            return (value == AgeRangeTag.age_13_15) || (value == AgeRangeTag.age_16_17) || (value == AgeRangeTag.age_over_12);
        }
        /// <summary>Returns true if age is 13--17</summary>
        public static bool IsAgeForYouth(this AgeRange value)
        {
            return (value.Tag == AgeRangeTag.age_13_15) || (value.Tag == AgeRangeTag.age_16_17) || (value.Tag == AgeRangeTag.age_over_12);
        }

        /// <summary>Returns true if age is 18+</summary>
        public static bool IsAgeForAdults(this AgeRangeTag value)
        {
            return (value == AgeRangeTag.age_over_18);
        }
        /// <summary>Returns true if age is 18+</summary>
        public static bool IsAgeForAdults(this AgeRange value)
        {
            return (value.Tag == AgeRangeTag.age_over_18);
        }

        /// <summary>Returns true if age is 12+ (this is quite raw...)</summary>
        public static bool IsAgeForOver12(this AgeRangeTag value)
        {
            return (value == AgeRangeTag.age_over_12) || (value == AgeRangeTag.age_13_15);
        }
        /// <summary>Returns true if age is 12+ (this is quite raw...)</summary>
        public static bool IsAgeForOver12(this AgeRange value)
        {
            return (value.Tag == AgeRangeTag.age_over_12) || (value.Tag == AgeRangeTag.age_13_15);
        }

        /// <summary>Converting an integer value into AgeRange enum</summary>
        public static AgeRangeTag ToAgeRangeTag(this int value)
        {
            return value <= 12 ? 
                        value <= 7 ?
                            value <= 5 ?
                                AgeRangeTag.age_3_5
                                : AgeRangeTag.age_6_7
                            : AgeRangeTag.age_8_12
                        : value <= 15 ?
                            AgeRangeTag.age_13_15
                            : value <= 16 ?
                                AgeRangeTag.age_16_17
                                : AgeRangeTag.age_over_18;
        }
    }
}
