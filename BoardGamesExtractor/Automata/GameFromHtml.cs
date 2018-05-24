using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HGNot = BoardGamesExtractor.HobbyGames_Notation;

namespace BoardGamesExtractor
{
    // "If you automate a mess, you get an automated mess." - Rod Michael

    // see https://hobbygames.ru/catalog-all?results_per_page=60&sort=date_added&order=ASC&all=1
    // see https://hobbygames.ru/catalog-all?results_per_page=60&page=2&sort=date_added&order=ASC&all=1
    // Amount is script-driven, let it be 7662 as @2018-04-05, then xml lines

    public static partial class GamesFromHtml
    {
        /// <summary>The states are represented in straight order, excepr for availability, which is variative, and Categories, which are a list
        /// -> Undef -> { Undef, Start };
        /// Start -> WebPageSectionStart -> WebPageAddressStart -> WebPageAddressEnd -> WebPageSectionSet -> ProductInfoTag -> ... -> AvailabilityTag;
        /// AvailabilityTag -> {AvailabilityTagToCart, AvailabilityTagAbsent, AvailabilityTagAnnounced } -> GameParamsTag;
        /// GameParamsTag -> { GameParamsTimeTag, GameParamsPlayersTag1, GameParamsAgeTag1, GameParamsScaledTag1 };
        /// GameParamsTimeTag -> GameParamsTimeTitleTag -> GameParamsTimeTitleEnd -> GameParamsTag /λ/;
        /// GameParamsPlayersTag1 -> GameParamsPlayersTag2 -> GameParamsPlayersTitleTag -> GameParamsPlayersTitleEnd -> GameParamsTag /λ/;
        /// GameParamsAgeTag1 -> GameParamsAgeTag2 -> GameParamsAgeTitleTag -> GameParamsAgeTitleEnd -> GameParamsTag /λ/;
        /// GameParamsScaledTag1 -> { GameParamsScaledTag2, CategoryTagStart1, NotABoardGame };
        /// GameParamsScaledTag2 -> { GameParamsScaledComplexityTitle, GameParamsScaledActivityTitle, GameParamsScaledPlanningTitle, CategoryTagStart1 };
        /// GameParamsScaledComplexityTitle -> GameParamsScaledComplexityStart -> GameParamsScaledComplexityEnd -> GameParamsScaledTag2 /λ/; 
        /// GameParamsScaledActivityTitle -> GameParamsScaledActivityStart -> GameParamsScaledActivityEnd -> GameParamsScaledTag2 /λ/;
        /// GameParamsScaledPlanningTitle -> GameParamsScaledPlanningStart -> GameParamsScaledPlanningEnd -> GameParamsScaledTag2 /λ/;
        /// CategoryTagStart1 ->  CategoryTagStart2 -> {CategoryItemStart, CategoryTagEnd };
        /// CategoryItemStart -> CategoryItemLinkStart -> CategoryItemLinkEnd -> CategoryItemEnd -> { CategoryItemStart, CategoryTagEnd };
        /// CategoryTagEnd -> { DescriptionTextStart, ManufacturersSection };
        /// DescriptionTextStart -> DescriptionTextEnd -> ManufacturersSection;
        /// ManufacturersSection -> ManufacturerItem;
        /// ManufacturerItem -> { ManufacturerItemTitleYear, ManufacturerItemTitleAuthor, ManufacturerItemTitleOrg };
        /// ManufacturerItemTitleYear -> ManufacturerItemTitleEnd -> ManufacturerItemValueStart -> ManufacturerItemValueEnd;
        /// ManufacturerItemTitleAuthor -> ManufacturerItemTitleEnd -> ManufacturerItemValueStart -> ManufacturerItemValueEnd;
        /// ManufacturerItemTitleOrg -> ManufacturerItemTitleEnd -> ManufacturerItemValueEnd;
        /// ManufacturerItemValueEnd -> { ManufacturerItem, End };
        /// {Any, except End and ManufacturerItemValueEnd } -> Undef
        /// </summary>
        private enum SingleGameAutomState { Undef, Start,
            WebPageSectionStart, WebPageSectionSet, WebPageAddressStart, WebPageAddressEnd,
            ProductInfoTag, ProductInfoHead, ProductNameStart, ProductNameEnd,
            ProductDesStart, ProductDesEnd,
            AvailabilityTag, AvailabilityTagToCart, AvailabilityTagAbsent, AvailabilityTagAnnounced,
            GameParamsTag,
                GameParamsTimeTag, GameParamsTimeTitleTag, GameParamsTimeTitleEnd,
                GameParamsPlayersTag1, GameParamsPlayersTag2, GameParamsPlayersTitleTag, GameParamsPlayersTitleEnd,
                GameParamsAgeTag1, GameParamsAgeTag2, GameParamsAgeTitleTag, GameParamsAgeTitleEnd,
            GameParamsScaledTag1, GameParamsScaledTag2,
                GameParamsScaledComplexityTitle, GameParamsScaledComplexityStart, GameParamsScaledComplexityEnd,
                GameParamsScaledActivityTitle, GameParamsScaledActivityStart, GameParamsScaledActivityEnd,
                GameParamsScaledPlanningTitle, GameParamsScaledPlanningStart, GameParamsScaledPlanningEnd,
            CategoryTagStart1, CategoryTagStart2, CategoryItemStart, CategoryItemLinkStart, CategoryItemLinkEnd, CategoryItemEnd, CategoryTagEnd,
            DescriptionTextStart, DescriptionTextEnd,
            ManufacturersSection, ManufacturerItem, ManufacturerItemTitleYear, ManufacturerItemTitleAuthor, ManufacturerItemTitleOrg, 
                ManufacturerItemTitleEnd, ManufacturerItemValueStart, ManufacturerItemValueEnd,
            NotABoardGame,
            End
        };

        /// <summary>This checks whether the buffer should be appended or replaced, basing on the automata phase</summary>
        private static bool IsAppendState(SingleGameAutomState phase)
        {
            return phase == SingleGameAutomState.ProductDesStart | phase == SingleGameAutomState.CategoryItemLinkEnd |
                phase == SingleGameAutomState.DescriptionTextStart | phase == SingleGameAutomState.GameParamsScaledTag2 |
                phase == SingleGameAutomState.GameParamsTag | phase == SingleGameAutomState.ManufacturerItemTitleYear |
                phase == SingleGameAutomState.ManufacturerItemTitleAuthor | phase == SingleGameAutomState.ManufacturerItemValueStart |
                phase == SingleGameAutomState.ManufacturerItemTitleYear | phase == SingleGameAutomState.ManufacturerItemTitleAuthor |
                phase == SingleGameAutomState.ManufacturerItemTitleOrg;
        }

        /// <summary>Automata phase to string converter</summary>
        private static string ToString(this SingleGameAutomState phase)
        {
            string res = "Undef";
            switch (phase)
            {
                case SingleGameAutomState.Undef:                            res = "Undef"; break;
                case SingleGameAutomState.Start:                            res = "Start"; break;
                case SingleGameAutomState.WebPageSectionStart:              res = "WebPageSectionStart"; break;
                case SingleGameAutomState.WebPageSectionSet:                res = "WebPageSectionSet"; break;
                case SingleGameAutomState.WebPageAddressStart:              res = "WebPageAddressStart"; break;
                case SingleGameAutomState.WebPageAddressEnd:                res = "WebPageAddressEnd"; break;
                case SingleGameAutomState.ProductInfoTag:                   res = "ProductInfoTag"; break;
                case SingleGameAutomState.ProductInfoHead:                  res = "ProductInfoHead"; break;
                case SingleGameAutomState.ProductNameStart:                 res = "ProductNameStart"; break;
                case SingleGameAutomState.ProductNameEnd:                   res = "ProductNameEnd"; break;
                case SingleGameAutomState.ProductDesStart:                  res = "ProductDesStart"; break;
                case SingleGameAutomState.ProductDesEnd:                    res = "ProductDesEnd"; break;
                case SingleGameAutomState.AvailabilityTag:                  res = "AvailabilityTag"; break;
                case SingleGameAutomState.AvailabilityTagToCart:            res = "AvailabilityTagToCart"; break;
                case SingleGameAutomState.AvailabilityTagAbsent:            res = "AvailabilityTagAbsent"; break;
                case SingleGameAutomState.AvailabilityTagAnnounced:         res = "AvailabilityTagAnnounced"; break;
                case SingleGameAutomState.GameParamsTag:                    res = "GameParamsTag"; break;
                case SingleGameAutomState.GameParamsTimeTag:                res = "GameParamsTimeTag"; break;
                case SingleGameAutomState.GameParamsTimeTitleTag:           res = "GameParamsTimeTitleTag"; break;
                case SingleGameAutomState.GameParamsTimeTitleEnd:           res = "GameParamsTimeTitleEnd"; break;
                case SingleGameAutomState.GameParamsPlayersTag1:            res = "GameParamsPlayersTag1"; break;
                case SingleGameAutomState.GameParamsPlayersTag2:            res = "GameParamsPlayersTag2"; break;
                case SingleGameAutomState.GameParamsPlayersTitleTag:        res = "GameParamsPlayersTitleTag"; break;
                case SingleGameAutomState.GameParamsPlayersTitleEnd:        res = "GameParamsPlayersTitleEnd"; break;
                case SingleGameAutomState.GameParamsAgeTag1:                res = "GameParamsAgeTag1"; break;
                case SingleGameAutomState.GameParamsAgeTag2:                res = "GameParamsAgeTag2"; break;
                case SingleGameAutomState.GameParamsAgeTitleTag:            res = "GameParamsAgeTitleTag"; break;
                case SingleGameAutomState.GameParamsAgeTitleEnd:            res = "GameParamsAgeTitleEnd"; break;
                case SingleGameAutomState.GameParamsScaledTag1:             res = "GameParamsScaledTag1"; break;
                case SingleGameAutomState.GameParamsScaledTag2:             res = "GameParamsScaledTag2"; break;
                case SingleGameAutomState.GameParamsScaledComplexityTitle:  res = "GameParamsScaledComplexityTitle"; break;
                case SingleGameAutomState.GameParamsScaledComplexityStart:  res = "GameParamsScaledComplexityStart"; break;
                case SingleGameAutomState.GameParamsScaledComplexityEnd:    res = "GameParamsScaledComplexityEnd"; break;
                case SingleGameAutomState.GameParamsScaledActivityTitle:    res = "GameParamsScaledActivityTitle"; break;
                case SingleGameAutomState.GameParamsScaledActivityStart:    res = "GameParamsScaledActivityStart"; break;
                case SingleGameAutomState.GameParamsScaledActivityEnd:      res = "GameParamsScaledActivityEnd"; break;
                case SingleGameAutomState.GameParamsScaledPlanningTitle:    res = "GameParamsScaledPlanningTitle"; break;
                case SingleGameAutomState.GameParamsScaledPlanningStart:    res = "GameParamsScaledPlanningStart"; break;
                case SingleGameAutomState.GameParamsScaledPlanningEnd:      res = "GameParamsScaledPlanningEnd"; break;
                case SingleGameAutomState.CategoryTagStart1:                res = "CategoryTagStart1"; break;
                case SingleGameAutomState.CategoryTagStart2:                res = "CategoryTagStart2"; break;
                case SingleGameAutomState.CategoryItemStart:                res = "CategoryItemStart"; break;
                case SingleGameAutomState.CategoryItemLinkStart:            res = "CategoryItemLinkStart"; break;
                case SingleGameAutomState.CategoryItemLinkEnd:              res = "CategoryItemLinkEnd"; break;
                case SingleGameAutomState.CategoryItemEnd:                  res = "CategoryItemEnd"; break;
                case SingleGameAutomState.CategoryTagEnd:                   res = "CategoryTagEnd"; break;
                case SingleGameAutomState.DescriptionTextStart:             res = "DescriptionTextStart"; break;
                case SingleGameAutomState.DescriptionTextEnd:               res = "DescriptionTextEnd"; break;
                case SingleGameAutomState.ManufacturersSection:             res = "ManufacturersSection"; break;
                case SingleGameAutomState.ManufacturerItem:                 res = "ManufacturerItem"; break;
                case SingleGameAutomState.ManufacturerItemTitleYear:        res = "ManufacturerItemTitleYear"; break;
                case SingleGameAutomState.ManufacturerItemTitleAuthor:      res = "ManufacturerItemTitleAuthor"; break;
                case SingleGameAutomState.ManufacturerItemTitleOrg:         res = "ManufacturerItemTitleOrg"; break;
                case SingleGameAutomState.ManufacturerItemTitleEnd:         res = "ManufacturerItemTitleEnd"; break;
                case SingleGameAutomState.ManufacturerItemValueStart:       res = "ManufacturerItemValueStart"; break;
                case SingleGameAutomState.ManufacturerItemValueEnd:         res = "ManufacturerItemValueEnd"; break;
                case SingleGameAutomState.NotABoardGame:                    res = "NotABoardGame"; break;
                case SingleGameAutomState.End:                              res = "End"; break;
                default: res = "else"; break;
            }
            return res;
        }

        /// <param name="fName">The source file with html-code</param>
        /// <param name="enc">File encoding</param>
        /// <param name="Game">Result</param>
        /// <param name="GetTextDes">True if game text description is required</param>
        /// <param name="ParamsZeroBasedScale">True if ZeroBasedScale is required</param>
        /// <param name="ManufacturersDict">Dictionary of possible manufacturers</param>
        /// <param name="isNotABoardGame">Whether the automata worked correctly, but the markup showed it's not a board game</param>
        public static void GetGameFromFile_Manually(string fName, System.Text.Encoding enc,
                                                    out GameParams Game, bool GetTextDes, bool ParamsZeroBasedScale,
                                                    ref ManufacturersDictionary ManufacturersDict,
                                                    out bool isNotABoardGame,
                                                    out bool res, out string msg)
        {
            res = File.Exists(fName);
            msg = res ? "" : ("Файл '" + fName + "' не существует.");
            bool b_do = res, b_io = false, b_apd = false;
            FileStream fs = null; StreamReader sr = null;
            string buf = "", bb;
            isNotABoardGame = false;
            Game = new GameParams(ParamsZeroBasedScale);
            SingleGameAutomState phase = SingleGameAutomState.Undef;
            SingleGameAutomState PrevState = phase;
            string sPrevStateDEBUG = PrevState.ToString();

            if (b_do) try
            {
                fs = new FileStream(fName, FileMode.Open);
                fs.Seek(0, SeekOrigin.Begin);
                sr = new StreamReader(fs, enc);
                b_io = true;
                phase = SingleGameAutomState.Start;
                b_do &= !sr.EndOfStream;
            }
            catch (Exception ex)
            {
                b_do = res = false; msg = "Error when starting reading data: " + ex.Message;
            }
            
            while (b_do)
            {
                try
                {
                    bb = sr.ReadLine();
                }
                catch (Exception ex)
                {
                    res = false; msg = "Error when reading line from file: " + ex.Message;
                    bb = "";
                    if (phase != SingleGameAutomState.End)
                        phase = SingleGameAutomState.Undef;
                }
                if (b_apd)
                    buf += ' ' + bb;
                else
                    buf = bb;

                if (res)
                    WorkThroughLine(ref buf, ref phase, ref Game, GetTextDes, ref ManufacturersDict, ref PrevState, out res, out msg);
                //sPrevStateDEBUG = PrevState.ToString();
                //Console.WriteLine(sPrevStateDEBUG);

                b_apd = IsAppendState(phase);
                b_do = (phase != SingleGameAutomState.End) & res;
                if (sr.EndOfStream | !b_do)
                    if ((phase != SingleGameAutomState.End) && (phase != SingleGameAutomState.ManufacturerItemValueEnd))
                    {
                        res = b_do = false;
                        msg = "Unfinished markup, parsing automata stopped in state " + phase.ToString() + "=>Undef";
                        phase = SingleGameAutomState.Undef;
                    }
                if (phase == SingleGameAutomState.NotABoardGame)
                {
                    isNotABoardGame = true;
                    res = false;
                    b_do = false;
                    msg = "The automata stated, that it is NOT a board game.";
                }
            }
            
            if (b_io) try
            {
                if (sr != null) sr.Close();
                if (fs != null) fs.Close();
            }
            catch (Exception ex)
            {
                res = false; msg += " Error when closing reading object(s): " + ex.Message;
            }
        }

        /// <summary>In fact, an automaton to parse the file line by line</summary>
        /// <param name="buf">The string buffer for parsing (not necessarily should be reset, appending is possible as well, see IsAppendState)</param>
        /// <param name="phase">Current phase of the automata</param>
        /// <param name="Game">Result</param>
        /// <param name="GetTextLongDes">True if game text description is required</param>
        /// <param name="ManufacturersDict">Dictionary of possible manufacturers</param>
        /// <param name="PrevState">The previous state - used for logging/locating parsing errors if the automata went Undef</param>
        private static void WorkThroughLine(ref string buf, ref SingleGameAutomState phase,
                                            ref GameParams Game, bool GetTextLongDes, ref ManufacturersDictionary ManufacturersDict,
                                            ref SingleGameAutomState PrevState, out bool res, out string msg)
        {
            res = true;
            msg = "";
            int i, j, k, c;
            bool again = true;
            while (again)
            {
                if ((phase != SingleGameAutomState.Start) && (phase != SingleGameAutomState.Undef))
                    PrevState = phase;
                //string sPrevStateDEBUG = PrevState.ToString();
                again = true;
                switch (phase)
                {
                    case SingleGameAutomState.Undef:
                        again = false;
                        break;
                    case SingleGameAutomState.Start:
                        i = buf.IndexOf(HGNot.WebPageSectionStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.WebPageSectionStart;
                            buf = buf.Remove(0, i + HGNot.WebPageSectionStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.WebPageSectionStart:
                        i = buf.IndexOf(HGNot.WebPageSectionSet);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.WebPageSectionSet;
                            buf = buf.Remove(0, i + HGNot.WebPageSectionSet.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.WebPageSectionSet:
                        i = buf.IndexOf(HGNot.WebPageAddressStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.WebPageAddressStart;
                            buf = buf.Remove(0, i + HGNot.WebPageAddressStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.WebPageAddressStart:
                        i = buf.IndexOf(HGNot.WebPageAddressEnd);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.WebPageAddressEnd;
                            Game.WebPage = buf.Substring(0, i);
                            buf = buf.Remove(0, i + HGNot.WebPageAddressEnd.Length).Trim();
                            if (buf.Length > 0) again = true;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.WebPageAddressEnd:
                        i = buf.IndexOf(HGNot.ProductInfoTag);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.ProductInfoTag;
                            buf = buf.Remove(0, i + HGNot.ProductInfoTag.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.ProductInfoTag:
                        i = buf.IndexOf(HGNot.ProductInfoHead);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.ProductInfoHead;
                            buf = buf.Remove(0, i + HGNot.ProductInfoHead.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.ProductInfoHead:
                        i = buf.IndexOf(HGNot.ProductNameStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.ProductNameStart;
                            buf = buf.Remove(0, i + HGNot.ProductNameStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }                        
                        break;
                    case SingleGameAutomState.ProductNameStart:
                        i = buf.IndexOf(HGNot.ProductNameEnd);
                        if (i >= 0)
                        {
                            Game.Title = buf.Substring(0, i);
                            phase = SingleGameAutomState.ProductNameEnd;
                            buf = buf.Remove(0, i + HGNot.ProductNameEnd.Length);
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            again = false;
                            // else waiting for appending the rest of the text data
                        }
                        break;
                    case SingleGameAutomState.ProductNameEnd:
                        i = buf.IndexOf(HGNot.ProductDesStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.ProductDesStart;
                            buf = buf.Remove(0, i + HGNot.ProductDesStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.ProductDesStart:
                        i = buf.IndexOf(HGNot.ProductDesEnd);
                        if (i >= 0)
                        {
                            Game.DesShortText = buf.Substring(0, i).DeTag().Trim();
                            phase = SingleGameAutomState.ProductDesEnd;
                            buf = buf.Remove(0, i + HGNot.ProductDesEnd.Length);
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            again = false;
                            // else waiting for appending the rest of the text data
                        }
                        break;
                    case SingleGameAutomState.ProductDesEnd:
                        i = buf.IndexOf(HGNot.AvailabilityTag);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.AvailabilityTag;
                            buf = buf.Remove(0, i + HGNot.AvailabilityTag.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.AvailabilityTag:
                        // AvailabilityTag -> {AvailabilityTagToCart, AvailabilityTagAbsent, AvailabilityTagAnnounced} -> DescriptionTextTag
                        i = buf.IndexOf(HGNot.AvailabilityTagToCart);
                        j = buf.IndexOf(HGNot.AvailabilityTagAbsent);
                        k = buf.IndexOf(HGNot.AvailabilityTagAnnounced);
                        if (i >= 0)
                        {
                            Game.Absent = false;
                            phase = SingleGameAutomState.AvailabilityTagToCart;
                            buf = buf.Substring(i + HGNot.AvailabilityTagToCart.Length);
                        }
                        else
                            if (j >= 0)
                            {
                                Game.Absent = true;
                                phase = SingleGameAutomState.AvailabilityTagAbsent;
                                buf = buf.Substring(j + HGNot.AvailabilityTagAbsent.Length);
                            }
                            else
                                if (k >= 0)
                                {
                                    Game.Absent = true;
                                    Game.AbsentButAnnounced = true;
                                    phase = SingleGameAutomState.AvailabilityTagAnnounced;
                                    buf = buf.Substring(j + HGNot.AvailabilityTagAnnounced.Length);
                                }
                                else
                                {
                                    buf = "";
                                    again = false;
                                }
                        break;
                    case SingleGameAutomState.AvailabilityTagToCart:
                        // {AvailabilityTagToCart, AvailabilityTagAbsent, AvailabilityTagAnnounced} -> GameParamsTag
                        i = buf.IndexOf(HGNot.GameParamsTag);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsTag;
                            buf = buf.Remove(0, i + HGNot.GameParamsTag.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.AvailabilityTagAbsent:
                        // {AvailabilityTagToCart, AvailabilityTagAbsent, AvailabilityTagAnnounced} -> GameParamsTag
                        i = buf.IndexOf(HGNot.GameParamsTag);
                        j = buf.IndexOf(HGNot.CategoryTagStart1);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsTag;
                            buf = buf.Remove(0, i + HGNot.GameParamsTag.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.AvailabilityTagAnnounced:
                        // {AvailabilityTagToCart, AvailabilityTagAbsent, AvailabilityTagAnnounced} -> GameParamsTag
                        i = buf.IndexOf(HGNot.GameParamsTag);
                        j = buf.IndexOf(HGNot.CategoryTagStart1);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsTag;
                            buf = buf.Remove(0, i + HGNot.GameParamsTag.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsTag:
                        // GameParamsTag -> { GameParamsTimeTag, GameParamsPlayersTag1, GameParamsAgeTag1, GameParamsScaledTag1 };
                        // GameParamsTimeTag -> GameParamsTimeTitleTag -> GameParamsTimeTitleEnd -> GameParamsTag /λ/;
                        // GameParamsPlayersTag1 -> GameParamsPlayersTag2 -> GameParamsPlayersTitleTag -> GameParamsPlayersTitleEnd -> GameParamsTag /λ/;
                        // GameParamsAgeTag1 -> GameParamsAgeTag2 -> GameParamsAgeTitleTag -> GameParamsAgeTitleEnd -> GameParamsTag /λ/;
                        i = buf.IndexOf(HGNot.GameParamsTimeTag);
                        j = buf.IndexOf(HGNot.GameParamsPlayersTag1);
                        k = buf.IndexOf(HGNot.GameParamsAgeTag1);
                        c = buf.IndexOf(HGNot.GameParamsScaledTag1);
                        if (i >= 0)
                            if (j >= 0)
                                if (k >= 0)
                                    if (c >= 0)
                                        if ((i < j) && (i < k) && (i < c))
                                        {
                                            phase = SingleGameAutomState.GameParamsTimeTag;
                                            buf = buf.Remove(0, i + HGNot.GameParamsTimeTag.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((j < i) && (j < k) && (j < c))
                                            {
                                                phase = SingleGameAutomState.GameParamsPlayersTag1;
                                                buf = buf.Remove(0, j + HGNot.GameParamsPlayersTag1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                                if ((k < i) && (k < j) && (k < c))
                                                {
                                                    phase = SingleGameAutomState.GameParamsAgeTag1;
                                                    buf = buf.Remove(0, k + HGNot.GameParamsAgeTag1.Length).Trim();
                                                    if (buf.Length == 0) again = false;
                                                }
                                                else
                                                {
                                                    phase = SingleGameAutomState.GameParamsScaledTag1;
                                                    buf = buf.Remove(0, c + HGNot.GameParamsScaledTag1.Length).Trim();
                                                    if (buf.Length == 0) again = false;
                                                }
                                    else
                                        if ((i < j) && (i < k))
                                        {
                                            phase = SingleGameAutomState.GameParamsTimeTag;
                                            buf = buf.Remove(0, i + HGNot.GameParamsTimeTag.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((j < i) && (j < k))
                                            {
                                                phase = SingleGameAutomState.GameParamsPlayersTag1;
                                                buf = buf.Remove(0, j + HGNot.GameParamsPlayersTag1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                            {
                                                phase = SingleGameAutomState.GameParamsAgeTag1;
                                                buf = buf.Remove(0, k + HGNot.GameParamsAgeTag1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                else
                                    if (c >= 0)
                                        if ((i < j) && (i < c))
                                        {
                                            phase = SingleGameAutomState.GameParamsTimeTag;
                                            buf = buf.Remove(0, i + HGNot.GameParamsTimeTag.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((j < i) && (j < c))
                                            {
                                                phase = SingleGameAutomState.GameParamsPlayersTag1;
                                                buf = buf.Remove(0, j + HGNot.GameParamsPlayersTag1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                            {
                                                phase = SingleGameAutomState.GameParamsScaledTag1;
                                                buf = buf.Remove(0, c + HGNot.GameParamsScaledTag1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                    else
                                        if (i < j)
                                        {
                                            phase = SingleGameAutomState.GameParamsTimeTag;
                                            buf = buf.Remove(0, i + HGNot.GameParamsTimeTag.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.GameParamsPlayersTag1;
                                            buf = buf.Remove(0, j + HGNot.GameParamsPlayersTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                            else
                                if (k >= 0)
                                    if (c >= 0)
                                        if ((i < k) && (i < c))
                                        {
                                            phase = SingleGameAutomState.GameParamsTimeTag;
                                            buf = buf.Remove(0, i + HGNot.GameParamsTimeTag.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((k < i) && (k < c))
                                            {
                                                phase = SingleGameAutomState.GameParamsAgeTag1;
                                                buf = buf.Remove(0, k + HGNot.GameParamsAgeTag1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                            {
                                                phase = SingleGameAutomState.GameParamsScaledTag1;
                                                buf = buf.Remove(0, c + HGNot.GameParamsScaledTag1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                    else
                                        if (i < k)
                                        {
                                            phase = SingleGameAutomState.GameParamsTimeTag;
                                            buf = buf.Remove(0, i + HGNot.GameParamsTimeTag.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.GameParamsAgeTag1;
                                            buf = buf.Remove(0, k + HGNot.GameParamsAgeTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                else
                                    if (c >= 0)
                                        if (i < c)
                                        {
                                            phase = SingleGameAutomState.GameParamsTimeTag;
                                            buf = buf.Remove(0, i + HGNot.GameParamsTimeTag.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledTag1;
                                            buf = buf.Remove(0, c + HGNot.GameParamsScaledTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                    else
                                    {
                                        phase = SingleGameAutomState.GameParamsTimeTag;
                                        buf = buf.Remove(0, i + HGNot.GameParamsTimeTag.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                        else
                            if (j >= 0)
                                if (k >= 0)
                                    if (c >= 0)
                                        if ((j < k) && (j < c))
                                        {
                                            phase = SingleGameAutomState.GameParamsPlayersTag1;
                                            buf = buf.Remove(0, j + HGNot.GameParamsPlayersTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((k < j) && (k < c))
                                            {
                                                phase = SingleGameAutomState.GameParamsAgeTag1;
                                                buf = buf.Remove(0, k + HGNot.GameParamsAgeTag1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                            {
                                                phase = SingleGameAutomState.GameParamsScaledTag1;
                                                buf = buf.Remove(0, c + HGNot.GameParamsScaledTag1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                    else
                                        if (j < k)
                                        {
                                            phase = SingleGameAutomState.GameParamsPlayersTag1;
                                            buf = buf.Remove(0, j + HGNot.GameParamsPlayersTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.GameParamsAgeTag1;
                                            buf = buf.Remove(0, k + HGNot.GameParamsAgeTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                else
                                    if (c >= 0)
                                        if (j < c)
                                        {
                                            phase = SingleGameAutomState.GameParamsPlayersTag1;
                                            buf = buf.Remove(0, j + HGNot.GameParamsPlayersTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledTag1;
                                            buf = buf.Remove(0, c + HGNot.GameParamsScaledTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                    else
                                    {
                                        phase = SingleGameAutomState.GameParamsPlayersTag1;
                                        buf = buf.Remove(0, j + HGNot.GameParamsPlayersTag1.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                            else
                                if (k >= 0)
                                    if (c >= 0)
                                        if (k < c)
                                        {
                                            phase = SingleGameAutomState.GameParamsAgeTag1;
                                            buf = buf.Remove(0, k + HGNot.GameParamsAgeTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledTag1;
                                            buf = buf.Remove(0, c + HGNot.GameParamsScaledTag1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                    else
                                    {
                                        phase = SingleGameAutomState.GameParamsAgeTag1;
                                        buf = buf.Remove(0, k + HGNot.GameParamsAgeTag1.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                else
                                    if (c >= 0)
                                    {
                                        phase = SingleGameAutomState.GameParamsScaledTag1;
                                        buf = buf.Remove(0, c + HGNot.GameParamsScaledTag1.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                    else
                                    {
                                        buf = "";
                                        again = false;
                                    }
                        break;
                    case SingleGameAutomState.GameParamsTimeTag:
                        i = buf.IndexOf(HGNot.GameParamsTimeTitleTag);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsTimeTitleTag;
                            buf = buf.Remove(0, i + HGNot.GameParamsTimeTitleTag.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsTimeTitleTag:
                        i = buf.IndexOf(HGNot.GameParamsTimeTitleEnd);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsTimeTitleEnd;
                            Game.Timing = new TimeRange(buf.Substring(0, i));
                            buf = buf.Remove(0, i + HGNot.GameParamsTimeTitleEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsTimeTitleEnd:
                        phase = SingleGameAutomState.GameParamsTag;
                        /*i = buf.IndexOf(HGNot.GameParamsPlayersTag1);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsPlayersTag1;
                            buf = buf.Remove(0, i + HGNot.GameParamsPlayersTag1.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }*/
                        break;
                    case SingleGameAutomState.GameParamsPlayersTag1:
                        i = buf.IndexOf(HGNot.GameParamsPlayersTag2);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsPlayersTag2;
                            buf = buf.Remove(0, i + HGNot.GameParamsPlayersTag2.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsPlayersTag2:
                        i = buf.IndexOf(HGNot.GameParamsPlayersTitleTag);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsPlayersTitleTag;
                            buf = buf.Remove(0, i + HGNot.GameParamsPlayersTitleTag.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsPlayersTitleTag:
                        i = buf.IndexOf(HGNot.GameParamsPlayersTitleEnd);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsPlayersTitleEnd;
                            Game.Players = new PlayersRange(buf.Substring(0, i));
                            buf = buf.Remove(0, i + HGNot.GameParamsPlayersTitleEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsPlayersTitleEnd:
                        i = buf.IndexOf(HGNot.GameParamsAgeTag1);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsAgeTag1;
                            buf = buf.Remove(0, i + HGNot.GameParamsAgeTag1.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsAgeTag1:
                        i = buf.IndexOf(HGNot.GameParamsAgeTag2);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsAgeTag2;
                            buf = buf.Remove(0, i + HGNot.GameParamsAgeTag2.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsAgeTag2:
                        i = buf.IndexOf(HGNot.GameParamsAgeTitleTag);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsAgeTitleTag;
                            buf = buf.Remove(0, i + HGNot.GameParamsAgeTitleTag.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsAgeTitleTag:
                        i = buf.IndexOf(HGNot.GameParamsAgeTitleEnd);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsAgeTitleEnd;
                            Game.Age = new AgeRange(buf.Substring(0, i));
                            buf = buf.Remove(0, i + HGNot.GameParamsAgeTitleEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsAgeTitleEnd:
                        i = buf.IndexOf(HGNot.GameParamsScaledTag1);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsScaledTag1;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledTag1.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsScaledTag1:
                        // GameParamsScaledTag1 -> { GameParamsScaledTag2, CategoryTagStart1, NotABoardGame };
                        // GameParamsScaledTag2 -> { GameParamsScaledComplexityTitle, GameParamsScaledActivityTitle, GameParamsScaledPlanningTitle, CategoryTagStart1 };
                        // GameParamsScaledComplexityTitle -> GameParamsScaledComplexityStart -> GameParamsScaledComplexityEnd -> GameParamsScaledTag2 /λ/; 
                        // GameParamsScaledActivityTitle -> GameParamsScaledActivityStart -> GameParamsScaledActivityEnd -> GameParamsScaledTag2 /λ/;
                        // GameParamsScaledPlanningTitle -> GameParamsScaledPlanningStart -> GameParamsScaledPlanningEnd -> GameParamsScaledTag2 /λ/;
                        i = buf.IndexOf(HGNot.GameParamsScaledTag2);
                        j = buf.IndexOf(HGNot.CategoryTagStart1);
                        k = buf.IndexOf(HGNot.CategoryTagStart2);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsScaledTag2;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledTag2.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                            if (j >= 0)
                            {
                                    phase = SingleGameAutomState.CategoryTagStart1;
                                    buf = buf.Remove(0, j + HGNot.CategoryTagStart1.Length).Trim();
                                    if (buf.Length == 0) again = false;
                                }
                            else
                                if (k >= 0)
                                {
                                    phase = SingleGameAutomState.NotABoardGame;
                                    buf = buf.Remove(0, j + HGNot.CategoryTagStart2.Length).Trim();
                                    if (buf.Length == 0) again = false;
                                    again = false;
                                }
                            else
                            {
                                buf = "";
                                again = false;
                            }
                        break;
                    case SingleGameAutomState.GameParamsScaledTag2:
                        i = buf.IndexOf(HGNot.GameParamsScaledComplexityTitle);
                        j = buf.IndexOf(HGNot.GameParamsScaledActivityTitle);
                        k = buf.IndexOf(HGNot.GameParamsScaledPlanningTitle);
                        c = buf.IndexOf(HGNot.CategoryTagStart1);
                        if (i >= 0)
                            if (j >= 0)
                                if (k >= 0)
                                    if (c >= 0)
                                        if ((i < j) && (i < k) && (i < c))
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledComplexityTitle;
                                            buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((j < i) && (j < k) && (j < c))
                                            {
                                                phase = SingleGameAutomState.GameParamsScaledActivityTitle;
                                                buf = buf.Remove(0, j + HGNot.GameParamsScaledActivityTitle.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                                if ((k < i) && (k < j) && (k < c))
                                                {
                                                    phase = SingleGameAutomState.GameParamsScaledPlanningTitle;
                                                    buf = buf.Remove(0, k + HGNot.GameParamsScaledPlanningTitle.Length).Trim();
                                                    if (buf.Length == 0) again = false;
                                                }
                                                else
                                                {
                                                    phase = SingleGameAutomState.CategoryTagStart1;
                                                    buf = buf.Remove(0, c + HGNot.CategoryTagStart1.Length).Trim();
                                                    if (buf.Length == 0) again = false;
                                                }
                                    else
                                        if ((i < j) && (i < k))
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledComplexityTitle;
                                            buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((j < i) && (j < k))
                                            {
                                                phase = SingleGameAutomState.GameParamsScaledActivityTitle;
                                                buf = buf.Remove(0, j + HGNot.GameParamsScaledActivityTitle.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                            {
                                                phase = SingleGameAutomState.GameParamsScaledPlanningTitle;
                                                buf = buf.Remove(0, k + HGNot.GameParamsScaledPlanningTitle.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                else
                                    if (c >= 0)
                                        if ((i < j) && (i < c))
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledComplexityTitle;
                                            buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((j < i) && (j < c))
                                            {
                                                phase = SingleGameAutomState.GameParamsScaledActivityTitle;
                                                buf = buf.Remove(0, j + HGNot.GameParamsScaledActivityTitle.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                            {
                                                phase = SingleGameAutomState.CategoryTagStart1;
                                                buf = buf.Remove(0, c + HGNot.CategoryTagStart1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                    else
                                        if (i < j)
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledComplexityTitle;
                                            buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledActivityTitle;
                                            buf = buf.Remove(0, j + HGNot.GameParamsScaledActivityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                            else
                                if (k >= 0)
                                    if (c >= 0)
                                        if ((i < k) && (i < c))
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledComplexityTitle;
                                            buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((k < i) && (k < c))
                                            {
                                                phase = SingleGameAutomState.GameParamsScaledPlanningTitle;
                                                buf = buf.Remove(0, k + HGNot.GameParamsScaledPlanningTitle.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                            {
                                                phase = SingleGameAutomState.CategoryTagStart1;
                                                buf = buf.Remove(0, c + HGNot.CategoryTagStart1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                    else
                                        if (i < k)
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledComplexityTitle;
                                            buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledPlanningTitle;
                                            buf = buf.Remove(0, k + HGNot.GameParamsScaledPlanningTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                else
                                    if (c >= 0)
                                        if (i < c)
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledComplexityTitle;
                                            buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.CategoryTagStart1;
                                            buf = buf.Remove(0, c + HGNot.CategoryTagStart1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                    else
                                    {
                                        phase = SingleGameAutomState.GameParamsScaledComplexityTitle;
                                        buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityTitle.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                        else
                            if (j >= 0)
                                if (k >= 0)
                                    if (c >= 0)
                                        if ((j < k) && (j < c))
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledActivityTitle;
                                            buf = buf.Remove(0, j + HGNot.GameParamsScaledActivityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                            if ((k < j) && (k < c))
                                            {
                                                phase = SingleGameAutomState.GameParamsScaledPlanningTitle;
                                                buf = buf.Remove(0, k + HGNot.GameParamsScaledPlanningTitle.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                            else
                                            {
                                                phase = SingleGameAutomState.CategoryTagStart1;
                                                buf = buf.Remove(0, c + HGNot.CategoryTagStart1.Length).Trim();
                                                if (buf.Length == 0) again = false;
                                            }
                                    else
                                        if (j < k)
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledActivityTitle;
                                            buf = buf.Remove(0, j + HGNot.GameParamsScaledActivityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledPlanningTitle;
                                            buf = buf.Remove(0, k + HGNot.GameParamsScaledPlanningTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                else
                                    if (c >= 0)
                                        if (j < c)
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledActivityTitle;
                                            buf = buf.Remove(0, j + HGNot.GameParamsScaledActivityTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.CategoryTagStart1;
                                            buf = buf.Remove(0, c + HGNot.CategoryTagStart1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                    else
                                    {
                                        phase = SingleGameAutomState.GameParamsScaledActivityTitle;
                                        buf = buf.Remove(0, j + HGNot.GameParamsScaledActivityTitle.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                            else
                                if (k >= 0)
                                    if (c >= 0)
                                        if (k < c)
                                        {
                                            phase = SingleGameAutomState.GameParamsScaledPlanningTitle;
                                            buf = buf.Remove(0, k + HGNot.GameParamsScaledPlanningTitle.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.CategoryTagStart1;
                                            buf = buf.Remove(0, c + HGNot.CategoryTagStart1.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                    else
                                    {
                                        phase = SingleGameAutomState.GameParamsScaledPlanningTitle;
                                        buf = buf.Remove(0, k + HGNot.GameParamsScaledPlanningTitle.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                else
                                    if (c >= 0)
                                    {
                                        phase = SingleGameAutomState.CategoryTagStart1;
                                        buf = buf.Remove(0, c + HGNot.CategoryTagStart1.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                    else
                                    {
                                        buf = "";
                                        again = false;
                                    }
                        break;
                    case SingleGameAutomState.GameParamsScaledComplexityTitle:
                        i = buf.IndexOf(HGNot.GameParamsScaledComplexityStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsScaledComplexityStart;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsScaledComplexityStart:
                        i = buf.IndexOf(HGNot.GameParamsScaledComplexityEnd);
                        if (i >= 0)
                        {
                            string s = buf.Substring(0, i).Trim();
                            Game.Complexity = s.ToScaledParam();
                            phase = SingleGameAutomState.GameParamsScaledComplexityEnd;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledComplexityEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsScaledComplexityEnd:
                        phase = SingleGameAutomState.GameParamsScaledTag2;
                        /*i = buf.IndexOf(HGNot.GameParamsScaledActivityTitle);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsScaledActivityTitle;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledActivityTitle.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }*/
                        break;
                    case SingleGameAutomState.GameParamsScaledActivityTitle:
                        i = buf.IndexOf(HGNot.GameParamsScaledActivityStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsScaledActivityStart;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledActivityStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsScaledActivityStart:
                        i = buf.IndexOf(HGNot.GameParamsScaledActivityEnd);
                        if (i >= 0)
                        {
                            string s = buf.Substring(0, i).Trim();
                            Game.Activity = s.ToScaledParam();
                            phase = SingleGameAutomState.GameParamsScaledActivityEnd;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledActivityEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsScaledActivityEnd:
                        phase = SingleGameAutomState.GameParamsScaledTag2;
                        /*i = buf.IndexOf(HGNot.GameParamsScaledPlanningTitle);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsScaledPlanningTitle;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledPlanningTitle.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }*/
                        break;
                    case SingleGameAutomState.GameParamsScaledPlanningTitle:
                        i = buf.IndexOf(HGNot.GameParamsScaledPlanningStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.GameParamsScaledPlanningStart;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledPlanningStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsScaledPlanningStart:
                        i = buf.IndexOf(HGNot.GameParamsScaledPlanningEnd);
                        if (i >= 0)
                        {
                            string s = buf.Substring(0, i).Trim();
                            Game.Planning = s.ToScaledParam();
                            phase = SingleGameAutomState.GameParamsScaledPlanningEnd;
                            buf = buf.Remove(0, i + HGNot.GameParamsScaledPlanningEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.GameParamsScaledPlanningEnd:
                        phase = SingleGameAutomState.GameParamsScaledTag2;
                        /*i = buf.IndexOf(HGNot.CategoryTagStart1);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.CategoryTagStart1;
                            buf = buf.Remove(0, i + HGNot.CategoryTagStart1.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }*/
                        break;
                    case SingleGameAutomState.CategoryTagStart1:
                        i = buf.IndexOf(HGNot.CategoryTagStart2);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.CategoryTagStart2;
                            buf = buf.Remove(0, i + HGNot.CategoryTagStart2.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.CategoryTagStart2:
                        // CategoryTagStart2 -> {CategoryItemStart, CategoryTagEnd };
                        // CategoryItemStart -> CategoryItemLinkStart -> CategoryItemLinkEnd -> CategoryItemEnd -> { CategoryItemStart, CategoryTagEnd };
                        i = buf.IndexOf(HGNot.CategoryItemStart);
                        j = buf.IndexOf(HGNot.CategoryTagEnd);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.CategoryItemStart;
                            buf = buf.Remove(0, i + HGNot.CategoryItemStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                            if (j >= 0)
                            {
                                phase = SingleGameAutomState.CategoryTagEnd;
                                buf = buf.Remove(0, j + HGNot.CategoryTagEnd.Length).Trim();
                                if (buf.Length == 0) again = false;
                            }
                            else
                            {
                                buf = "";
                                again = false;
                            }
                        break;
                    case SingleGameAutomState.CategoryItemStart:
                        i = buf.IndexOf(HGNot.CategoryItemLinkStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.CategoryItemLinkStart;
                            buf = buf.Remove(0, i + HGNot.CategoryItemLinkStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.CategoryItemLinkStart:
                        i = buf.IndexOf(HGNot.CategoryItemLinkEnd);
                        if (i >= 0)
                        {
                            string s = buf.Substring(0, i).Trim();
                            Game.TagLinks.Add(s);
                            phase = SingleGameAutomState.CategoryItemLinkEnd;
                            buf = buf.Remove(0, i + HGNot.CategoryItemLinkEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.CategoryItemLinkEnd:
                        i = buf.IndexOf(HGNot.CategoryItemEnd);
                        if (i >= 0)
                        {
                            Game.Tags.Add(buf.Substring(0, i).DeTag().Trim());
                            phase = SingleGameAutomState.CategoryItemEnd;
                            buf = buf.Remove(0, i + HGNot.CategoryItemEnd.Length);
                            again = true;
                        }
                        else
                        {
                            again = false;
                            // else waiting for appending the rest of the text data, see b_apd
                        }
                        break;
                    case SingleGameAutomState.CategoryItemEnd:
                        // CategoryItemEnd -> { CategoryItemStart, CategoryTagEnd }
                        i = buf.IndexOf(HGNot.CategoryItemStart);
                        j = buf.IndexOf(HGNot.CategoryTagEnd);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.CategoryItemStart;
                            buf = buf.Remove(0, i + HGNot.CategoryItemStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                            if (j >= 0)
                            {
                                phase = SingleGameAutomState.CategoryTagEnd;
                                buf = buf.Remove(0, j + HGNot.CategoryTagEnd.Length).Trim();
                                if (buf.Length == 0) again = false;
                            }
                            else
                            {
                                buf = "";
                                again = false;
                            }
                        break;
                    case SingleGameAutomState.CategoryTagEnd:
                        if (GetTextLongDes)
                        {
                            i = buf.IndexOf(HGNot.DescriptionTextStart);
                            if (i >= 0)
                            {
                                Game.DesLongText = "";
                                phase = SingleGameAutomState.DescriptionTextStart;
                                buf = buf.Remove(0, i + HGNot.DescriptionTextStart.Length).Trim();
                                if (buf.Length == 0) again = false;
                            }
                            else
                            {
                                buf = "";
                                again = false;
                            }
                        }
                        else
                        {
                            phase = SingleGameAutomState.DescriptionTextEnd;
                        }
                        break;
                    case SingleGameAutomState.DescriptionTextStart:
                        i = buf.IndexOf(HGNot.DescriptionTextEnd);
                        if (i >= 0)
                        {
                            Game.DesLongText = buf.Substring(0, i).DeTag();
                            phase = SingleGameAutomState.DescriptionTextEnd;
                            buf = buf.Remove(0, i + HGNot.DescriptionTextEnd.Length);
                            again = true;
                        }
                        else
                        {
                            again = false;
                            // else waiting for appending the rest of the text data, see b_apd
                        }
                        break;
                    case SingleGameAutomState.DescriptionTextEnd:
                        i = buf.IndexOf(HGNot.ManufacturersSection);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.ManufacturersSection;
                            buf = buf.Remove(0, i + HGNot.ManufacturersSection.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }                        
                        break;
                    case SingleGameAutomState.ManufacturersSection:
                        // ManufacturersSection -> ManufacturerItem;
                        // ManufacturerItem -> { ManufacturerItemTitleYear, ManufacturerItemTitleAuthor, ManufacturerItemTitleOrg };
                        // ManufacturerItemTitleYear -> ManufacturerItemTitleEnd -> ManufacturerItemValueStart -> ManufacturerItemValueEnd;
                        // ManufacturerItemTitleAuthor -> ManufacturerItemTitleEnd -> ManufacturerItemValueStart -> ManufacturerItemValueEnd;
                        // ManufacturerItemTitleOrg -> ManufacturerItemTitleEnd -> ManufacturerItemValueEnd;
                        // ManufacturerItemValueEnd -> { ManufacturerItem, End };
                        i = buf.IndexOf(HGNot.ManufacturerItem);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.ManufacturerItem;
                            buf = buf.Remove(0, i + HGNot.ManufacturerItem.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.ManufacturerItem:
                        i = buf.IndexOf(HGNot.ManufacturerItemTitleYear);
                        j = buf.IndexOf(HGNot.ManufacturerItemTitleAuthor);
                        k = buf.IndexOf(HGNot.ManufacturerItemTitleOrg);
                        if (i >= 0)
                            if (j >= 0)
                                if (k >= 0)
                                    if ((i < j) && (i < k))
                                    {
                                        phase = SingleGameAutomState.ManufacturerItemTitleYear;
                                        buf = buf.Remove(0, i + HGNot.ManufacturerItemTitleYear.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                    else
                                        if ((j < i) && (j < k))
                                        {
                                            phase = SingleGameAutomState.ManufacturerItemTitleAuthor;
                                            buf = buf.Remove(0, j + HGNot.ManufacturerItemTitleAuthor.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                        else
                                        {
                                            phase = SingleGameAutomState.ManufacturerItemTitleOrg;
                                            buf = buf.Remove(0, k + HGNot.ManufacturerItemTitleOrg.Length).Trim();
                                            if (buf.Length == 0) again = false;
                                        }
                                else
                                    if (i < j)
                                    {
                                        phase = SingleGameAutomState.ManufacturerItemTitleYear;
                                        buf = buf.Remove(0, i + HGNot.ManufacturerItemTitleYear.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                    else
                                    {
                                        phase = SingleGameAutomState.ManufacturerItemTitleAuthor;
                                        buf = buf.Remove(0, j + HGNot.ManufacturerItemTitleAuthor.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                            else
                                if (k >= 0)
                                    if (i < k)
                                    {
                                        phase = SingleGameAutomState.ManufacturerItemTitleYear;
                                        buf = buf.Remove(0, i + HGNot.ManufacturerItemTitleYear.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                    else
                                    {
                                        phase = SingleGameAutomState.ManufacturerItemTitleOrg;
                                        buf = buf.Remove(0, k + HGNot.ManufacturerItemTitleOrg.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                else
                                {
                                    phase = SingleGameAutomState.ManufacturerItemTitleYear;
                                    buf = buf.Remove(0, i + HGNot.ManufacturerItemTitleYear.Length).Trim();
                                    if (buf.Length == 0) again = false;
                                }
                        else
                            if (j >= 0)
                                if (k >= 0)
                                    if (j < k)
                                    {
                                        phase = SingleGameAutomState.ManufacturerItemTitleAuthor;
                                        buf = buf.Remove(0, j + HGNot.ManufacturerItemTitleAuthor.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                    else
                                    {
                                        phase = SingleGameAutomState.ManufacturerItemTitleOrg;
                                        buf = buf.Remove(0, k + HGNot.ManufacturerItemTitleOrg.Length).Trim();
                                        if (buf.Length == 0) again = false;
                                    }
                                else
                                {
                                    phase = SingleGameAutomState.ManufacturerItemTitleAuthor;
                                    buf = buf.Remove(0, j + HGNot.ManufacturerItemTitleAuthor.Length).Trim();
                                    if (buf.Length == 0) again = false;
                                }
                            else
                                if (k >= 0)
                                {
                                    phase = SingleGameAutomState.ManufacturerItemTitleOrg;
                                    buf = buf.Remove(0, k + HGNot.ManufacturerItemTitleOrg.Length).Trim();
                                    if (buf.Length == 0) again = false;
                                }
                                else
                                {
                                    buf = "";
                                    again = false;
                                }
                        break;
                    case SingleGameAutomState.ManufacturerItemTitleYear:
                        i = buf.IndexOf(HGNot.ManufacturerItemValueStart);
                        j = buf.IndexOf(HGNot.ManufacturerItemValueEnd, i + 1);
                        if ((i >= 0) && (j > 0))
                        {
                            string s = buf.Substring(0, j).Substring(i + HGNot.ManufacturerItemValueStart.Length).DeTag().Trim();
                            try
                            {
                                Game.YearOfIssue = Convert.ToInt32(s);
                            }
                            catch
                            {
                                Game.YearOfIssue = 0;
                            }
                            phase = SingleGameAutomState.ManufacturerItemValueEnd;
                            buf = buf.Remove(0, j + HGNot.ManufacturerItemValueEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            again = false;
                            // waiting for the rest of the data
                        }
                        break;
                    case SingleGameAutomState.ManufacturerItemTitleAuthor:
                        i = buf.IndexOf(HGNot.ManufacturerItemValueStart);
                        j = buf.IndexOf(HGNot.ManufacturerItemValueEnd, i + 1);
                        if ((i >= 0) && (j > 0))
                        {
                            Game.Author = buf.Substring(0, j).Substring(i + HGNot.ManufacturerItemValueStart.Length).DeTag().Trim();
                            phase = SingleGameAutomState.ManufacturerItemValueEnd;
                            buf = buf.Remove(0, j + HGNot.ManufacturerItemValueEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            again = false;
                            // waiting for the rest of the data
                        }
                        break;
                    case SingleGameAutomState.ManufacturerItemTitleOrg:
                        i = buf.IndexOf(HGNot.ManufacturerItemValueStart);
                        j = buf.IndexOf(HGNot.ManufacturerItemValueEnd, i + 1);
                        if ((i >= 0) && (j > 0))
                        {
                            Game.ManufacturerName = buf.Substring(0, j).Substring(i + HGNot.ManufacturerItemValueStart.Length).DeTag().Trim();
                            Game.SetManufacturerID(ref ManufacturersDict);
                            phase = SingleGameAutomState.ManufacturerItemValueEnd;
                            buf = buf.Remove(0, j + HGNot.ManufacturerItemValueEnd.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            again = false;
                            // waiting for the rest of the data
                        }
                        break;
                    case SingleGameAutomState.ManufacturerItemTitleEnd:
                        i = buf.IndexOf(HGNot.CategoryItemLinkStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.CategoryItemLinkStart;
                            buf = buf.Remove(0, i + HGNot.CategoryItemLinkStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.ManufacturerItemValueStart:
                        i = buf.IndexOf(HGNot.CategoryItemLinkStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.CategoryItemLinkStart;
                            buf = buf.Remove(0, i + HGNot.CategoryItemLinkStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        break;
                    case SingleGameAutomState.ManufacturerItemValueEnd:
                        i = buf.IndexOf(HGNot.CategoryItemLinkStart);
                        if (i >= 0)
                        {
                            phase = SingleGameAutomState.CategoryItemLinkStart;
                            buf = buf.Remove(0, i + HGNot.CategoryItemLinkStart.Length).Trim();
                            if (buf.Length == 0) again = false;
                        }
                        else
                        {
                            buf = "";
                            again = false;
                        }
                        phase = SingleGameAutomState.End;
                        break;
                    case SingleGameAutomState.End:
                        again = false;
                        break;
                }
            }
        }

        /// <summary>Getting all of the games as GameParams instances from html dumped to the HD</summary>
        /// <param name="fNameDir">The path to the extracted games data</param>
        /// <param name="encDir">The encoding of the file with list of file names of the games directory</param>
        /// <param name="encHtml">The encoding of the files with games html</param>
        /// <param name="doGetGameTextDes">This flag indicates whether to extract (long) text description of a game or not</param>
        /// <param name="ParamsZeroBasedScale">True if ZeroBasedScale is required</param>
        /// <param name="doExcludeErroneousGames">True if resulting list should not contain games, which threw errors while parsing</param>
        /// <param name="ErroneousGamesIndices">Indices of erroneous games in the list, or if doExcludeErroneousGames then in the Dir</param>
        /// <param name="NotBoardGamesIndices">Indices of items in the Dir which showed to be not board games</param>
        /// <param name="ManufacturersDict">Dictionary of possible manufacturers</param>
        /// <param name="res">success flag</param>
        /// <param name="msg">the number of successfully procesed games and errors description in case of error(s), if any</param>
        /// <returns>A list of games as GameParams instances</returns>
        public static List<GameParams> GetGamesFromHtml(string fNameDir, System.Text.Encoding encDir, System.Text.Encoding encHtml,
                                                        bool doGetGameTextDes, bool ParamsZeroBasedScale,
                                                        bool doExcludeErroneousGames, out List<int> ErroneousGamesIndices,
                                                        out List<int> NotBoardGamesIndices,
                                                        ref ManufacturersDictionary ManufacturersDict,
                                                        out bool res, out List<string> msg)
        {
            List<GameParams> GameList = new List<GameParams>();
            GameParams AGame;
            string fName;
            res = File.Exists(fNameDir);
            string curmsg = res ? "" : ("Файл '" + fNameDir + "' не существует.");
            ErroneousGamesIndices = new List<int>();
            NotBoardGamesIndices = new List<int>();
            bool NotABoardGame;
            msg = new List<string>();
            msg.Add(curmsg);
            bool curres, b_do = res, b_io = false;
            FileStream fs = null; StreamReader sr = null;
            
            if (b_do) try
            {
                fs = new FileStream(fNameDir, FileMode.Open);
                fs.Seek(0, SeekOrigin.Begin);
                sr = new StreamReader(fs, encDir);
                b_do &= !sr.EndOfStream;
                b_io = true;
            }
            catch (Exception ex)
            {
                b_do = res = false;
                curmsg = "Error when starting reading data: " + ex.Message;
                msg.Add(curmsg);
            }
            int iDirGame = 0, NGames = 0, NDirErrors = 0, NParseErrors = 0;
            while (b_do)
            {
                try
                {
                    fName = sr.ReadLine();
                    #region DEBUG
                    if (fName.Contains("Game30731.txt"))
                        b_do = true;    // DEBUG
                    #endregion DEBUG
                    GetGameFromFile_Manually(fName, encHtml, out AGame, doGetGameTextDes, ParamsZeroBasedScale,
                                            ref ManufacturersDict, out NotABoardGame, out curres, out curmsg);
                    if (curres)
                    {
                        NGames++;
                        GameList.Add(AGame);
                    }
                    else
                        if (NotABoardGame)
                        {
                            curmsg = "Not a board game found when parsing game[" + iDirGame.ToString() + "] from dir file '" + fName + "': " + curmsg;
                            msg.Add(curmsg);
                            NotBoardGamesIndices.Add(iDirGame);
                        }
                        else
                        {
                            NParseErrors++;
                            curmsg = "Error when parsing game[" + iDirGame.ToString() + "] from dir file '" + fName + "': " + curmsg;
                            msg.Add(curmsg);
                            if (doExcludeErroneousGames)
                            {
                                ErroneousGamesIndices.Add(iDirGame);
                            }
                            else
                            {
                                ErroneousGamesIndices.Add(NGames);
                                NGames++;
                                GameList.Add(AGame);
                            }
                        }
                }
                catch (Exception ex)
                {
                    NDirErrors++;
                    curres = false;
                    curmsg = "Error when reading file name [" + iDirGame.ToString() + "] from dir file: " + ex.Message;
                    msg.Add(curmsg);
                }
                b_do &= !sr.EndOfStream;
                iDirGame++;
            }

            if (b_io) try
            {
                if (sr != null) sr.Close();
                if (fs != null) fs.Close();
            }
            catch (Exception ex)
            {
                curres = false;
                curmsg = " Error when closing reading object(s): " + ex.Message;
                msg.Add(curmsg);
            }

            curmsg = "Total errors:\t" + (NDirErrors + NParseErrors).ToString();
            msg.Insert(0, curmsg);
            curmsg = "Errors while parsing game html files:\t" + NParseErrors.ToString();
            msg.Insert(0, curmsg);
            curmsg = "Errors while reading dir file:\t" + NDirErrors.ToString();
            msg.Insert(0, curmsg);
            curmsg = "Games_extracted_successfully:\t" + NGames.ToString(); 
            msg.Insert(0, curmsg);

            return GameList;
        }

        /// <summary>A method for removing tags from a string s</summary>
        public static string DeTag(this string s)
        {
            int j, i = s.IndexOf('<');
            while (i >= 0)
            {
                j = s.IndexOf('>', i + 1);
                s = s.Remove(i, j - i + 1);
                s = s.Insert(i, " ");
                i = s.IndexOf('<', i);
            }
            return s;
        }
    }
}
