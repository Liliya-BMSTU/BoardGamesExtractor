using System;
using HGNot = BoardGamesExtractor.HobbyGames_Notation;

namespace BoardGamesExtractor
{
    /// <summary>Класс, который хранит интервал количества игроков (есть значения по дефолту, есть кастование из входной строки)</summary>
    public class PlayersRange
    {
        public const int MINVALUE = 0; // though it might as well be 0
        public const int MAXVALUE = 0; // though it might as well be int.MaxValue

        /// <summary>Raw text avaliable from the HTML markup, e.g. "2-6 игроков"</summary>
        public string RawText;
        public int MinPlayers;
        public int MaxPlayers;

        public PlayersRange()
        {
            RawText = "";
            MinPlayers = MINVALUE;
            MaxPlayers = MAXVALUE;
        }

        public PlayersRange(string rawText)
        {
            MinPlayers = MINVALUE;
            MaxPlayers = MAXVALUE;
            RawText = rawText;

            int pos = RawText.IndexOf(HGNot.GameParamsPlayersTitleText);
            if (pos >= 0)
            {
                RawText = RawText.Substring(pos + HGNot.GameParamsPlayersTitleText.Length).TrimStart();
            }
            pos = RawText.IndexOf(HGNot.GameParamsPlayersPostfix);
            if (pos >= 0)
            {
                RawText = RawText.Substring(0, pos).Trim();
            }
            // now there can be "0-15" or "от 2 до 10" or "до 360" or "240+"

            RawText.ToRange(MINVALUE, MAXVALUE, out MinPlayers, out MaxPlayers);
        }
    }
}
