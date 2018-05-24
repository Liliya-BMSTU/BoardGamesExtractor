using System;
using System.Collections.Generic;

namespace BoardGamesExtractor
{
    /// <summary>Серия игры</summary>
    public class GameSeries
    {
        // Серии игр, их же много. Значит, их надо брать либо по ID их базы, либо по ID своей базы, либо по имени серии.
        // Предположим, будем хранить ID и строку. Это класс, если надо, модифицируем.
        // Когда/если выгружу словарь серий игр с сайта, станет понятно.

        public readonly int RawID;
        public readonly int DictID;
        public readonly string Name;

        public GameSeries(int _ID, string _Name)
        {
            RawID = _ID;
            Name = _Name;
            DictID = SetDictID();
        }

        private int SetDictID()
        {
            int res = -1;           // ToDo: to implement
            // concerning RawID and Name 
            return res;
        }
    }
}
