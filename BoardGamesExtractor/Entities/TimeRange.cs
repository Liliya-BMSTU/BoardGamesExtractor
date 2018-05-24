using System;
using HGNot = BoardGamesExtractor.HobbyGames_Notation;

namespace BoardGamesExtractor
{
    /// <summary>Time range (enum), convertible to minutes (int) and backwards</summary>
    public enum TimeRangeTag { undef = -1, time_0_15, time_16_30, time_31_60, time_61_120, time_121_240, time_over_2hrs
                                // now those which are missing at the main search webpage
                                , time_121_360 }

    /// <summary>Класс, который хранит интервал времени игры (есть значения по дефолту, есть кастование из входной строки и из перечисления TimeRangeTag)</summary>
    public class TimeRange
    {
        public const int MINVALUE = 0;       // though it might as well be anything else
        public const int MAXVALUE = 65535;   // though it might as well be int.MaxValue

        public bool Over2hrs;
        public string RawText;
        public bool HasTimeRangeTag;
        public TimeRangeTag Tag;
        public int MinTime;
        public int MaxTime;

        public TimeRange()
        {
            RawText = "";
            Tag = TimeRangeTag.undef;
            HasTimeRangeTag = false;
            MinTime = MINVALUE;
            MaxTime = MAXVALUE;
        }

        public TimeRange(string rawText)
        {
            Tag = TimeRangeTag.undef;
            HasTimeRangeTag = false;
            MinTime = MINVALUE;
            MaxTime = MAXVALUE;
            RawText = rawText;
            int pos = RawText.IndexOf(HGNot.GameParamsTimeTitleText);
            if (pos >= 0)
            {
                RawText = RawText.Substring(pos + HGNot.GameParamsTimeTitleText.Length).TrimStart();
            }
            pos = RawText.IndexOf(HGNot.GameParamsTimePostfix);
            if (pos >= 0)
            {
                RawText = RawText.Substring(0, pos).Trim();
            }
            // now there can be "0-15" or "от 2 до 10" or "до 360" or "240+"

            RawText.ToRange(MINVALUE, MAXVALUE, out MinTime, out MaxTime);
        }

        public TimeRange(TimeRangeTag trTag)
        {
            Tag = trTag;
            HasTimeRangeTag = true;
            RawText = "";
            switch (trTag)
            {
                case TimeRangeTag.undef:
                    MinTime = MINVALUE;
                    MaxTime = MAXVALUE;
                    break;
                case TimeRangeTag.time_0_15:
                    MinTime = 0;
                    MaxTime = 15;
                    break;
                case TimeRangeTag.time_16_30:
                    MinTime = 16;
                    MaxTime = 30;
                    break;
                case TimeRangeTag.time_31_60:
                    MinTime = 31;
                    MaxTime = 60;
                    break;
                case TimeRangeTag.time_61_120:
                    MinTime = 61;
                    MaxTime = 120;
                    break;
                case TimeRangeTag.time_121_240:
                    MinTime = 121;
                    MaxTime = 240;
                    break;
                case TimeRangeTag.time_over_2hrs:
                    MinTime = 121;
                    MaxTime = MAXVALUE;
                    break;
                case TimeRangeTag.time_121_360:
                    MinTime = 121;
                    MaxTime = 360;
                    break;
                default:
                    MinTime = MINVALUE;
                    MaxTime = MAXVALUE;
                    break;
            }
        }
    }

    public static class TimeRangeRoutines
    {
        /// <summary>Retrieving minimum time for a specified TimeRange enum value (in minutes)</summary>
        public static int MinTime_Min(this TimeRangeTag value)
        {
            int res = 0;
            switch (value)
            {
                case TimeRangeTag.time_0_15:
                    res = 0;
                    break;
                case TimeRangeTag.time_16_30:
                    res = 16;
                    break;
                case TimeRangeTag.time_31_60:
                    res = 31;
                    break;
                case TimeRangeTag.time_61_120:
                    res = 61;
                    break;
                case TimeRangeTag.time_121_240:
                    res = 121;
                    break;
                case TimeRangeTag.time_over_2hrs:
                    res = 121;
                    break;
            }
            return res;
        }

        /// <summary>Retrieving maximum time for a specified TimeRange enum value (in minutes)</summary>
        public static int MaxTime_Min(this TimeRangeTag value)
        {
            int res = int.MaxValue;
            switch (value)
            {
                case TimeRangeTag.time_0_15:
                    res = 15;
                    break;
                case TimeRangeTag.time_16_30:
                    res = 30;
                    break;
                case TimeRangeTag.time_31_60:
                    res = 60;
                    break;
                case TimeRangeTag.time_61_120:
                    res = 120;
                    break;
                case TimeRangeTag.time_121_240:
                    res = 240;
                    break;
                case TimeRangeTag.time_over_2hrs:
                    res = int.MaxValue;
                    break;
            }
            return res;
        }

        /// <summary>Retrieving minimum time for a specified TimeRange enum value (in hours)</summary>
        public static int MinTime_Hrs(this TimeRangeTag value)
        {
            int res = 0;
            switch (value)
            {
                case TimeRangeTag.time_0_15:
                    res = 0;
                    break;
                case TimeRangeTag.time_16_30:
                    res = 0;
                    break;
                case TimeRangeTag.time_31_60:
                    res = 0;
                    break;
                case TimeRangeTag.time_61_120:
                    res = 1;
                    break;
                case TimeRangeTag.time_121_240:
                    res = 2;
                    break;
                case TimeRangeTag.time_over_2hrs:
                    res = 2;
                    break;
            }
            return res;
        }

        /// <summary>Retrieving maximum time for a specified TimeRange enum value (in hours)</summary>
        public static int MaxTime_Hrs(this TimeRangeTag value)
        {
            int res = 0;
            switch (value)
            {
                case TimeRangeTag.time_0_15:
                    res = 0;
                    break;
                case TimeRangeTag.time_16_30:
                    res = 0;
                    break;
                case TimeRangeTag.time_31_60:
                    res = 1;
                    break;
                case TimeRangeTag.time_61_120:
                    res = 2;
                    break;
                case TimeRangeTag.time_121_240:
                    res = 4;
                    break;
                case TimeRangeTag.time_over_2hrs:
                    res = int.MaxValue;
                    break;
            }
            return res;
        }

        /// <summary>Converting an integer value into TimeRange enum from minutes</summary>
        public static TimeRangeTag ToTimeRangeTag(this int value)
        {
            return value <= 60 ? 
                        value <= 30 ?
                            value <= 15 ?
                                TimeRangeTag.time_0_15
                                : TimeRangeTag.time_16_30
                            : TimeRangeTag.time_31_60
                        : value <= 120 ?
                            TimeRangeTag.time_61_120
                            : value <= 240 ?
                                TimeRangeTag.time_121_240
                                : TimeRangeTag.time_over_2hrs;
        }

        /// <summary>Converting a string value into TimeRange enum from minutes</summary>
        /// <param name="value">input string</param>
        /// <param name="OptionalTimeRange">TimeRange (intended to be optional)</param>
        public static TimeRangeTag ToTimeRangeTag(this string value)
        {
            // Sample: <div class="time" title="Время игры: от 60 до 120 минут">
            TimeRangeTag res = TimeRangeTag.undef;
            TimeRange TR = new TimeRange(value);
            // now converting to a tag
            
            
            //else    // case 'undef', by default
            return res;
        }
    }
}
