using System;

namespace BoardGamesExtractor
{
    public static class TimeStamping
    {
        /// <summary>Returns string value: YYMMDD</summary>
        public static string StringStamp_YYMMDD()
        {
            DateTime dtNow = DateTime.Now;
            return ((dtNow.Year % 100) * 10000 +  dtNow.Month * 100 +
                dtNow.Day).ToString("D6");
        }

        /// <summary>Returns string value: HHMMSS</summary>
        public static string StringStamp_HHMMSS()
        {
            DateTime dtNow = DateTime.Now;
            return (dtNow.Hour * 10000 +
                dtNow.Minute * 100 + dtNow.Second).ToString("D6");
        }
    }
}
