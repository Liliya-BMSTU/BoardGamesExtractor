using System;
using System.Collections.Generic;

namespace BoardGamesExtractor
{
    /// <summary>Описание игры</summary>
    public class GameParams
    {
        /// <summary>This is intended to be true if the scale for complexity, activity,
        /// planning is 0-based [0..4], otherwise [1..5]</summary>
        public readonly bool ZeroBasedScale;

        /// <summary>Game title (hopefully, unique)</summary>
        public string Title;
        /// <summary>A description for the game (a short text, frequently the slogan)</summary>
        public string DesShortText;
        /// <summary>A description for the game (a long text, a few paragraphs)</summary>
        public string DesLongText;
        /// <summary>Game tags (categories and series)</summary>
        public List<string> Tags;
        /// <summary>Game tag links (for categories and series from Tags list)</summary>
        public List<string> TagLinks;

        // 3D vector of <complexity(easy|hard), activity(move|think), planning(chance|tactics)>
        /// <summary>complexity(easy|hard), scale: ZeroBasedScale ? [0..4] : [1..5]<summary>
        private int _complexity;
        /// <summary>activity(move|think), scale: ZeroBasedScale ? [0..4] : [1..5]<summary>
        private int _activity;
        /// <summary>planning(chance|tactics), scale: ZeroBasedScale ? [0..4] : [1..5]<summary>
        private int _planning;

        /// <summary>complexity(easy|hard) accessor, scale: ZeroBasedScale ? [0..4] : [1..5], default = minimum<summary>
        public int Complexity { get { return _complexity; } set { _complexity = ZeroBasedScale ? value < 0 ? 0 : value > 4 ? 4 : value : value < 1 ? 1 : value > 5 ? 5 : value; } }
        /// <summary>activity(move|think) accessor, scale: ZeroBasedScale ? [0..4] : [1..5], default = minimum<summary>
        public int Activity   { get { return _activity; }   set { _activity   = ZeroBasedScale ? value < 0 ? 0 : value > 4 ? 4 : value : value < 1 ? 1 : value > 5 ? 5 : value; } }
        /// <summary>planning(chance|tactics) accessor, scale: ZeroBasedScale ? [0..4] : [1..5], default = minimum<summary>
        public int Planning   { get { return _planning; }   set { _planning   = ZeroBasedScale ? value < 0 ? 0 : value > 4 ? 4 : value : value < 1 ? 1 : value > 5 ? 5 : value; } }
        
        /// <summary>Game time range</summary>
        public TimeRange Timing;
        /// <summary>Game age range</summary>
        public AgeRange Age;
        /// <summary>Game players number</summary>
        public PlayersRange Players;

        /// <summary>Game series (e.g., Carcassonne)</summary>
        public List<string> Series;
        /// <summary>Game categories (e.g., Adventure games, Logic games, etc.)</summary>
        public List<string> Categories;
        /// <summary>Game thematic (e.g., Pirates, Knights, etc.)</summary>
        public List<string> Thematic;
        /// <summary>Game options (e.g., EN, Games for the road, Games for pairs, etc.)</summary>
        public List<string> OptionalTags;

        /// <summary>Manufacturer ID (if any, -1 by default)</summary>
        public int ManufacturerID;
        /// <summary>Manufacturer name (if any)</summary>
        public string ManufacturerName;
        /// <summary>Year of issue (if any)</summary>
        public int YearOfIssue;
        /// <summary>Author (if any)</summary>
        public string Author;
        /// <summary>Webpage</summary>
        public string WebPage;

        /// <summary>Whether the site has this game absent or present</summary>
        public bool Absent;
        /// <summary>Whether the site has this game absent, but announced</summary>
        public bool AbsentButAnnounced;

        /// <summary>GameParams constructor. Warning: set the ZeroBasedScale parameter corectly!</summary>
        /// <param name="zeroBasedScale">Set as true if the scale for complexity, activity,
        /// planning is 0-based [0..4], otherwise [1..5]</param>
        public GameParams(bool zeroBasedScale)
        {
            ZeroBasedScale = zeroBasedScale;
            // Other parameters are set by default
            Title = "";
            DesShortText = "";
            DesLongText = "";
            Tags = new List<string>(1);
            TagLinks = new List<string>(1);
            Complexity = ZeroBasedScale ? 0 : 1;
            Activity = ZeroBasedScale ? 0 : 1;
            Planning = ZeroBasedScale ? 0 : 1;
            Timing = new TimeRange(TimeRangeTag.undef);
            Age = new AgeRange(AgeRangeTag.undef);
            Players = new PlayersRange();
            ManufacturerID = -1;
            ManufacturerName = "";
            YearOfIssue = 0;
            Author = "";
            WebPage = "";
            Series = new List<string>(0);//new GameSeries(-1, "");
            Categories = new List<string>(0);//new GameSeries(-1, "");
            Thematic = new List<string>(0);
            OptionalTags = new List<string>(0);
            Absent = false;
            AbsentButAnnounced = false;
        }

        /// <summary>GameParams constructor. Warning: set the ZeroBasedScale parameter corectly!</summary>
        /// <param name="zeroBasedScale">Set as true if the scale for complexity, activity,
        /// planning is 0-based [0..4], otherwise [1..5]</param>
        /// <param name="title">Game title</param>
        /// <param name="desShortText">Game short description (most often a slogan)</param>
        /// <param name="desLongText">Game long description (plain text)</param>
        /// <param name="complexity">complexity(easy|hard), scale: ZeroBasedScale ? [0..4] : [1..5]</param>
        /// <param name="activity">activity(move|think), scale: zeroBasedScale ? [0..4] : [1..5]</param>
        /// <param name="planning">planning(chance|tactics), scale: zeroBasedScale ? [0..4] : [1..5]</param>
        /// <param name="gamersNumber">number of gamers (0 by default), scale: zeroBasedScale ? [0..4] : [1..5]</param>
        /// <param name="timing">Game time range</param>
        /// <param name="age">Game age range</param>
        /// <param name="players">Game players range</param>
        /// <param name="series">Game series (e.g., Carcassonne)</param>
        /// <param name="categories">Game categories (e.g., Adventure games, Logic games, etc.)</param>
        /// <param name="thematic">Game thematic (e.g., Pirates, Knights, etc.)</param>
        /// <param name="optionalTags">Game options (e.g., EN, Games for the road, Games for pairs, etc.)</param>
        /// <param name="manufacturerID">Manufacturer ID</param>
        /// <param name="manufacturerName">Manufacturer name</param>
        /// <param name="yearOfIssue">Year of issue</param>
        /// <param name="author">Game author</param>
        /// <param name="webPage">Webpage</param>
        public GameParams(bool zeroBasedScale, string title, string desShortText, string desLongText,
                            int complexity, int activity, int planning,
                            TimeRange timing, AgeRange age, PlayersRange players,
                            List<string> series, List<string> categories, List<string> thematic, List<string> optionalTags,
                            int manufacturerID, string manufacturerName, int yearOfIssue, string author, string webPage)
        {
            ZeroBasedScale = zeroBasedScale;
            Title = title;
            DesShortText = desShortText;
            DesLongText = desLongText;
            Tags = new List<string>(1);
            TagLinks = new List<string>(1);
            Complexity = complexity;        // via accessor
            Activity = activity;            // via accessor
            Planning = planning;            // via accessor
            Timing = timing;
            Age = age;
            Players = players;
            ManufacturerID = manufacturerID;
            ManufacturerName = manufacturerName;
            WebPage = webPage;
            YearOfIssue = yearOfIssue;
            Author = author;
            Series = series;
            Categories = categories;
            Thematic = thematic;
            OptionalTags = optionalTags;
            Absent = false;
            AbsentButAnnounced = false;
        }

        /// <summary>GameParams constructor. Warning: set the ZeroBasedScale parameter corectly!</summary>
        /// <param name="zeroBasedScale">Set as true if the scale for complexity, activity,
        /// planning is 0-based [0..4], otherwise [1..5]</param>
        /// <param name="title">Game title</param>
        /// <param name="desShortText">Game short description (most often a slogan)</param>
        /// <param name="desLongText">Game long description (plain text)</param>
        /// <param name="complexity">complexity(easy|hard), scale: ZeroBasedScale ? [0..4] : [1..5]</param>
        /// <param name="activity">activity(move|think), scale: zeroBasedScale ? [0..4] : [1..5]</param>
        /// <param name="planning">planning(chance|tactics), scale: zeroBasedScale ? [0..4] : [1..5]</param>
        /// <param name="gamersNumber">number of gamers (0 by default), scale: zeroBasedScale ? [0..4] : [1..5]</param>
        /// <param name="timing">Game time range</param>
        /// <param name="age">Game age range</param>
        /// <param name="players">Game players range</param>
        /// <param name="series">Game series (e.g., Carcassonne)</param>
        /// <param name="categories">Game categories (e.g., Adventure games, Logic games, etc.)</param>
        /// <param name="thematic">Game thematic (e.g., Pirates, Knights, etc.)</param>
        /// <param name="optionalTags">Game options (e.g., EN, Games for the road, Games for pairs, etc.)</param>
        /// <param name="manufacturerID">Manufacturer ID</param>
        /// <param name="manufacturerName">Manufacturer name</param>
        /// <param name="yearOfIssue">Year of issue</param>
        /// <param name="author">Game author</param>
        /// <param name="webPage">Webpage</param>
        /// <param name="absent">Whether the site has this game absent or present</param>
        public GameParams(bool zeroBasedScale, string title, string desShortText, string desLongText,
                            int complexity, int activity, int planning,
                            TimeRange timing, AgeRange age, PlayersRange players,
                            List<string> series, List<string> categories, List<string> thematic, List<string> optionalTags,
                            int manufacturerID, string manufacturerName, int yearOfIssue, string author, string webPage,
                            bool absent, bool absentButAnnounced)
        {
            ZeroBasedScale = zeroBasedScale;
            Title = title;
            DesShortText = desShortText;
            DesLongText = desLongText;
            Tags = new List<string>(1);
            TagLinks = new List<string>(1);
            Complexity = complexity;        // via accessor
            Activity = activity;            // via accessor
            Planning = planning;            // via accessor
            Timing = timing;
            Age = age;
            Players = players;
            ManufacturerID = manufacturerID;
            ManufacturerName = manufacturerName;
            WebPage = webPage;
            YearOfIssue = yearOfIssue;
            Author = author;
            Series = series;
            Categories = categories;
            Thematic = thematic;
            OptionalTags = optionalTags;
            Absent = absent;
            AbsentButAnnounced = absentButAnnounced;
        }

        public void SetManufacturerID(ref List<string> ManufacturersDict)
        {
            ManufacturerID = ManufacturersDict.IndexOf(ManufacturerName);
        }

        public void SetManufacturerID(ref ManufacturersDictionary ManufacturersDict)
        {
            ManufacturerID = ManufacturersDict.IndexOf(ManufacturerName);
        }
    }
}
