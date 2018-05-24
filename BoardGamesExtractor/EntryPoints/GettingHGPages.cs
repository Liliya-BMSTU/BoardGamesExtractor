using System;
using System.Collections.Generic;
using HGNot = BoardGamesExtractor.HobbyGames_Notation;

namespace BoardGamesExtractor
{
    public static class GettingHGPages
    {
        // Образец 1й и любой другой страниц поиска:
        // see https://hobbygames.ru/nastolnie?results_per_page=60&sort=date_added&order=ASC&all=1
        // see https://hobbygames.ru/nastolnie?results_per_page=60&page=2&sort=date_added&order=ASC&all=1
        
        /// <summary>Это настольные игры</summary>
        public const string HG_Search_PathBase = "https://hobbygames.ru/nastolnie?results_per_page=60&###sort=date_added&order=ASC&all=1";
        /// <summary>Это всё, что есть в каталоге (не только настольные игры)</summary>
        //const string HG_Search_PathBase = "https://hobbygames.ru/catalog-all?results_per_page=60&###sort=date_added&order=ASC&all=1";

        public const string SubstPage0 = "";
        public const string SubstPagei = "&page=";
        public const string Marker = "###";
        public const string FileIOBase = RoofDir.ROOT_HOBBYGAMES_IN_SEARCH + "Search###.html";
        public const string FNameAbout_Short = "About.txt";
        public const string FNameSearchAbout_Full = RoofDir.ROOT_HOBBYGAMES_IN_SEARCH + FNameAbout_Short;
        public const string FNameDir_Short = "Dir.txt";
        public const string FNameSearchDir_Full = RoofDir.ROOT_HOBBYGAMES_IN_SEARCH + FNameDir_Short;
        public const string FNameSearchRes = RoofDir.ROOT_HOBBYGAMES_OUT + "SearchRes.txt";
        public const string FNameManufacturers = RoofDir.ROOT_HOBBYGAMES_OUT + "Manufacturers.txt";
        public const string FNameTags = RoofDir.ROOT_HOBBYGAMES_OUT + "Tags.txt";
        public const string FNameCategories = RoofDir.ROOT_HOBBYGAMES_OUT + "Categories.txt";
        public const string FNameOptions = RoofDir.ROOT_HOBBYGAMES_OUT + "Options.txt";
        public const string FNameThematic = RoofDir.ROOT_HOBBYGAMES_OUT + "Thematic.txt";
        public const string FNameBlackList = RoofDir.ROOT_HOBBYGAMES_OUT + "BlackList.txt";
        public const string FNameCorruptIndexes = RoofDir.ROOT_HOBBYGAMES_OUT + "CorruptIndexes.txt";
        public const string FNameGameBase = RoofDir.ROOT_HOBBYGAMES_IN_GAMES + "Game###.txt";
        public const string FNameGamesAbout_Full = RoofDir.ROOT_HOBBYGAMES_IN_GAMES + FNameAbout_Short;
        public const string FNameGamesDir_Full = RoofDir.ROOT_HOBBYGAMES_IN_GAMES + FNameDir_Short;

        public const string ParsingLog = RoofDir.ROOT_HOBBYGAMES_OUT + "ParseLog.txt";

        public static System.Text.Encoding EncUTF8 { get { return Encodings.EncodingUTF8; } }

        /// <summary>Выгрузка всех html-страниц поиска с сайта</summary>
        public static void GetHGSearchPages()
        {
            System.Text.Encoding enc = EncUTF8;

            bool res; string msg;

            uint PageIndex = 1, MaxPage = 1;
            string HG_Search_Path = HG_Search_PathBase.Replace(Marker, SubstPage0);
            string Html = GetWebPages.GetWebPage(HG_Search_Path, enc, out res, out msg);
            string TargPath = "";
            if (res)
            {
                TargPath = FileIOBase.ReplaceWithIndexOfLength(Marker, PageIndex, 4);
                FileIO.ClearFile(TargPath, out res, out msg);
                FileIO.WriteData(TargPath, Html, out res, out msg);
                if (!res)
                    Console.WriteLine("Error while dumping page to \"" + TargPath + "\": " + msg);
            }
            else
                Console.WriteLine("Error while getting page \"" + HG_Search_Path + "\": " + msg);

            //<div class="paginate"><ul class="pagination"><li class="current selected"><a href="#">1</a></li><li><a href="https://hobbygames.ru/catalog/search?page=2&amp;results_per_page=30">2</a></li><li><a href="https://hobbygames.ru/catalog/search?page=3&amp;results_per_page=30">3</a></li><li><a href="https://hobbygames.ru/catalog/search?page=4&amp;results_per_page=30">4</a></li><li><a href="https://hobbygames.ru/catalog/search?page=5&amp;results_per_page=30">5</a></li><li><a href="https://hobbygames.ru/catalog/search?page=2&amp;results_per_page=30" class="next"><span class="icon icon-arrow-right"></span></a></li><li><a href="https://hobbygames.ru/catalog/search?page=234&amp;results_per_page=30" class="last"><span class="icon icon-arrow-right"></span><span class="icon icon-arrow-right"></span></a></li></ul></div>

            int idx1 = Html.IndexOf(HGNot.PaginationTag), idx2 = Html.IndexOf(HGNot.PaginationEnd, idx1), LN = HGNot.PaginationItemStart.Length, i;
            if (idx1 == 0)
            {
                Console.WriteLine("No pagination section containing the last page was found on the very first search page!");
            }
            else
            {
                string pg, pgs = Html.Substring(idx1 + HGNot.PaginationTag.Length, idx2 - idx1 - HGNot.PaginationTag.Length);
                int idx3, idx4;
                idx2 = 0;
                idx1 = pgs.IndexOf(HGNot.PaginationItemStart, idx2);
                while (idx1 >= 0)
                {
                    idx2 = pgs.IndexOf(HGNot.PaginationItemEnd, idx1 + LN);
                    pg = pgs.Substring(idx1 + LN, idx2 - idx1 - LN);
                    if (pg.Contains(HGNot.PaginationItemLast))
                    {
                        idx3 = pg.IndexOf(HGNot.PaginationPgPrefix);
                        idx4 = pg.IndexOf(HGNot.PaginationPgPostfix);
                        if (idx3 > 0)
                        {
                            pg = pg.Substring(idx3 + HGNot.PaginationPgPrefix.Length, idx4 - idx3 - HGNot.PaginationPgPrefix.Length);
                            try
                            {
                                MaxPage = Convert.ToUInt32(pg);
                            }
                            catch
                            {
                                MaxPage = 1;
                                Console.WriteLine("Pagination section found, but the last page index corrupted -- on the very first search page!");
                            }
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Pagination section found, but the last page -- on the very first search page!");
                            break;
                        }
                    }
                    idx1 = pgs.IndexOf(HGNot.PaginationItemStart, idx2);
                }
            
                for (i = 2; i <= MaxPage; i++)
                {
                    PageIndex++;
                    HG_Search_Path = HG_Search_PathBase.Replace(Marker, SubstPagei + PageIndex.ToString());
                    Html = GetWebPages.GetWebPage(HG_Search_Path, enc, out res, out msg);
                    if (res)
                    {
                        TargPath = FileIOBase.ReplaceWithIndexOfLength(Marker, PageIndex, 4);
                        FileIO.ClearFile(TargPath, out res, out msg);
                        FileIO.WriteData(TargPath, Html, out res, out msg);
                        if (!res)
                            Console.WriteLine("Error while dumping page to \"" + TargPath + "\": " + msg);
                    }
                    else
                        Console.WriteLine("Error while getting page \"" + HG_Search_Path + "\": " + msg);
                }
            }
                
            DateTime dt = DateTime.Now;
            Html = dt.Year.IndexOfLength(4) + dt.Month.IndexOfLength(2) + dt.Day.IndexOfLength(2) + '\t' + PageIndex.ToString() + "\tpages\t60\tperpage";
            FileIO.WriteData(FNameSearchAbout_Full, Html, out res, out msg);
            if (!res)
                Console.WriteLine("Dump log has been dumped with error(s): " + msg);
            Console.WriteLine("Dumped search data @" + Html);
            
            List<string> DirFiles = FileIO.GetDirFiles(RoofDir.ROOT_HOBBYGAMES_IN_SEARCH, true, out res, out msg);
            i = 0;
            int N = DirFiles.Count;
            while (i < N)
            {
                if (DirFiles[i].Contains(FNameAbout_Short))
                {
                    DirFiles.RemoveAt(i);
                    break;
                }
                i++;
            }
            
            FileIO.ClearFile(FNameSearchDir_Full, out res, out msg);
            FileIO.WriteData(FNameSearchDir_Full, DirFiles, out res, out msg);
            if (!res)
                Console.WriteLine("List of directory files has been dumped with error(s): " + msg);
            Console.WriteLine("List of directory files for search data dumped.");

            Console.ReadLine();
        }

        /// <summary>Выгрузка количества всех поисковых страниц из одной страницы</summary>
        public static bool GetSearchPagesAmount(out int Amount)
        {
            bool res = true;
            Amount = 0;
            int N;
            string msg;
            string[] Lines = FileIO.ReadAllLines(FNameSearchAbout_Full, out N, out res, out msg);
            if (res)
                if (N > 0)
                {
                    string s = Lines[N-1];
                    N = s.IndexOf('\t');
                    if (N >= 0)
                    {
                        s = s.Substring(N+1);
                        N = s.IndexOf('\t');
                        if (N >= 0)
                            s = s.Substring(0, N);
                        try
                        {
                            N = Convert.ToInt32(s);
                            Amount = N;
                        }
                        catch
                        {
                            res = false;
                            msg = "Error getting pages amount: error while converting the 2nd field '" + s + "' from the last line of file '" + FNameSearchDir_Full + "'";
                        }
                    }
                    else
                    {
                        res = false;
                        msg = "Error getting pages amount: last line of the file '" + FNameSearchDir_Full + "' doesn't start with: datetime-int \\t amount";
                    }
                }
                else
                {
                    res = false;
                    msg = "Error getting pages amount: empty file '" + FNameSearchDir_Full + "'";
                }
            else
                msg = "Error getting pages amount: error while reading file '" + FNameSearchDir_Full + " + '";
            return res;
        }

        /// <summary>Выгрузка всех html-страниц с играми с сайта на материале поисковых страниц</summary>
        public static void GetHGGamePages()
        {
            System.Text.Encoding enc = Encodings.EncodingUTF8;

            bool res; string msg;
            string[] Games;
            int[] GameIDs;
            int i, N, HG_Game_ID, HG_Game_Price;
            string HG_Game_address, HG_Game_Title, Html, TargPath;

            Games = FileIO.ReadAllLines(FNameSearchRes, out N, out res, out msg);
            GameIDs = new int[Games.Length];
            char[] Splitters = new char[1] { '\t' };
            for (i = 0; i < N; i++)
            {
                GameIDs[i] = HG_Game_ID = Games[i].ExtractFieldInt(0, Splitters);
                HG_Game_Price = Games[i].ExtractFieldInt(1, Splitters);
                HG_Game_address = Games[i].ExtractField(2, Splitters);
                HG_Game_Title = Games[i].ExtractField(3, Splitters);
                Html = GetWebPages.GetWebPage(HG_Game_address, enc, out res, out msg);
                TargPath = "";
                if (res)
                {
                    TargPath = FNameGameBase.Replace(Marker, HG_Game_ID.ToString());
                    FileIO.ClearFile(TargPath, out res, out msg);
                    FileIO.WriteData(TargPath, Html, out res, out msg);
                    if (!res)
                        Console.WriteLine("Error while dumping page to \"" + TargPath + "\": " + msg);
                }
                else
                    Console.WriteLine("Error while getting page \"" + HG_Game_address + "\": " + msg);
            }

            int NUnique = GameIDs.DeDuplicatedAmount();
            DateTime dt = DateTime.Now;
            Html = dt.Year.IndexOfLength(4) + dt.Month.IndexOfLength(2) + dt.Day.IndexOfLength(2) + '\t' + NUnique.ToString() + "\tgames";
            FileIO.WriteData(FNameGamesAbout_Full, Html, out res, out msg);
            if (!res)
                Console.WriteLine("Dump log has been dumped with error(s): " + msg);
            Console.WriteLine("Dumped search data @" + Html);

            List<string> DirFiles = FileIO.GetDirFiles(RoofDir.ROOT_HOBBYGAMES_IN_GAMES, true, out res, out msg);
            i = 0;
            N = DirFiles.Count;
            while (i < N)
            {
                if (DirFiles[i].Contains(FNameAbout_Short))
                {
                    N--;
                    DirFiles.RemoveAt(i);
                    break;
                }
                i++;
            }
            i = 0;
            while (i < N)
            {
                if (DirFiles[i].Contains(FNameDir_Short))
                {
                    N--;
                    DirFiles.RemoveAt(i);
                    break;
                }
                i++;
            }

            FileIO.ClearFile(FNameGamesDir_Full, out res, out msg);
            FileIO.WriteData(FNameGamesDir_Full, DirFiles, out res, out msg);
            if (!res)
                Console.WriteLine("List of directory files has been dumped with error(s): " + msg);
            Console.WriteLine("List of directory files for search data dumped.");

            Console.ReadLine();
        }

        /// <summary>Состояния автомата для парсинга поисковых страниц:
        /// Start -> List -> Item -> ItemID -> ItemID End-> ItemPrice -> ItemPriceEnd -> ItemTitle -> ItemHref -> ItemHrefEnd -> ItemEnd -> { Item, ListEnd };
        /// ListEnd -> // fin</summary>
        private enum GameListAutomataState { Undef = -1, Start, List, Item, ItemID, ItemIDEnd, ItemPrice, ItemPriceEnd, ItemTitle, ItemHref, ItemHrefEnd, ItemEnd, ListEnd }

        /// <summary>Создание файла со списком игр из поисковых данныхб включая адреса веб-страниц, см. SearchRes.txt</summary>
        public static void GetHGGamesList()
        {
            bool res, doLog = true;
            string msg, log, s;
            bool flag = FileIO.FileExists(ParsingLog, out res, out msg);
            if (!res)
            {
                doLog = false;
                Console.WriteLine("GetHGGamePages: No logging available! Error: '" + msg + "'");
                Console.Write("Continue anyway? Press y|anything:");
                msg = "";
                while (msg == "")
                {
                    msg = Console.ReadLine().Trim();
                    if (msg.Length > 0)
                    {
                        res = ((msg[0] == 'y') || (msg[0] == 'Y'));
                        if (!res)
                            Console.WriteLine("Cancelling...");
                    }
                }
            }
            if (res)
            {
                FileIO.ClearFile(FNameSearchRes, out res, out msg);
                if (res)
                {
                    log = "#" + TimeStamping.StringStamp_YYMMDD() + "_" + TimeStamping.StringStamp_HHMMSS() + " GetHGGamePages";
                    if (doLog)
                        FileIO.WriteData(ParsingLog, log, out res, out msg);
                    Console.WriteLine(log);

                    int iPg, j, SearchDirLen, FileLen, pos;
                    string[] SearchDir = FileIO.ReadAllLines(FNameSearchDir_Full, out SearchDirLen, out res, out msg);
                    if (res)
                    {
                        string[] SearchPage;
                        int GameIdxInPage;
                        int GameID;
                        int GamePrice;
                        string GameHref;
                        string GameTitle;
                    
                        for (iPg = 0; iPg < SearchDirLen; iPg++)
                        {
                            GameIdxInPage = -1;
                            SearchPage = FileIO.ReadAllLines(SearchDir[iPg], out FileLen, out res, out msg);
                            if (res)
                            {
                                #region extracting
                                /*
                                SearchGameListStart = "<div class=\"products\">";
                                SearchGameListEnd = "Показано товаров";
                                SearchGameItemStart = "<div class=\"product-item\"";
                                SearchGameItemIDStart = "data-product_id=\"";
                                SearchGameItemIDEnd = "\"";
                                SearchGameItemPriceStart = "data-price=\"";
                                SearchGameItemPriceEnd = "\"";
                                SearchGameItemTitleTag = "<div class=\"name-desc\">";
                                SearchGameItemHrefStart = "<a href=\"";
                                SearchGameItemHrefEnd = "\">";
                                SearchGameItemEnd = "</a><div class=\"desc\">"; 
                                */
                                s = "";
                                j = 0;
                                GameID = -1;
                                GamePrice = 0;
                                GameHref = "";
                                GameTitle = "";
                                GameListAutomataState AutomState = GameListAutomataState.Undef;
                                bool searchforstart = true, processingline = false;
                                while ((j < FileLen) && ((AutomState == GameListAutomataState.Undef) == searchforstart) && (AutomState != GameListAutomataState.ListEnd))
                                {
                                    if (searchforstart)
                                    {
                                        pos = SearchPage[j].IndexOf(HGNot.SearchGameListStart);
                                        if (pos >= 0)
                                        {
                                            searchforstart = false;
                                            AutomState = GameListAutomataState.List;
                                        }
                                    }
                                    if (!searchforstart)  // to pass through from above
                                    {
                                        processingline = true;
                                        while (processingline)      // pos is stored along with s
                                            switch (AutomState)
                                            {
                                                case GameListAutomataState.List:
                                                    pos = SearchPage[j].IndexOf(HGNot.SearchGameItemStart);
                                                    if (pos >= 0)
                                                    {
                                                        AutomState = GameListAutomataState.Item;
                                                    }
                                                    else
                                                    {
                                                        processingline = false;
                                                    }
                                                    break;
                                                case GameListAutomataState.Item:
                                                    GameIdxInPage++;
                                                    GameID = -1;
                                                    GamePrice = 0;
                                                    GameHref = "";
                                                    GameTitle = "";
                                                    pos = SearchPage[j].IndexOf(HGNot.SearchGameItemIDStart);
                                                    if (pos >= 0)
                                                    {
                                                        s = SearchPage[j].Substring(pos + HGNot.SearchGameItemIDStart.Length);
                                                        AutomState = GameListAutomataState.ItemID;
                                                    }
                                                    else
                                                    {
                                                        processingline = false;
                                                    }
                                                    break;
                                                case GameListAutomataState.ItemID:
                                                    // s is kept
                                                    pos = s.IndexOf(HGNot.SearchGameItemIDEnd);
                                                    if (pos >= 0)
                                                    {
                                                        s = s.Substring(0, pos);
                                                        try
                                                        {
                                                            GameID = Convert.ToInt32(s);
                                                            AutomState = GameListAutomataState.ItemIDEnd;
                                                        }
                                                        catch
                                                        {
                                                            GameID = -1;
                                                            processingline = false;
                                                            AutomState = GameListAutomataState.Undef;
                                                            log = "Error while parsing '" + SearchDir[iPg] + "' on line " + j.ToString() +
                                                                    ": game[" + GameIdxInPage.ToString() + "] ID is NaN.";
                                                            if (doLog)
                                                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                                                            Console.WriteLine(log);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        j++;
                                                        if (j < FileLen)
                                                        {
                                                            s += SearchPage[j];
                                                        }
                                                        else
                                                        {
                                                            processingline = false;
                                                            AutomState = GameListAutomataState.Undef;
                                                            log = "Error while parsing '" + SearchDir[iPg] +
                                                                    "': unfinished markup, game[" + GameIdxInPage.ToString() + "] ID is not found.";
                                                            if (doLog)
                                                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                                                            Console.WriteLine(log);
                                                        }
                                                    }
                                                    break;
                                                case GameListAutomataState.ItemIDEnd:
                                                    pos = SearchPage[j].IndexOf(HGNot.SearchGameItemPriceStart);
                                                    if (pos >= 0)
                                                    {
                                                        s = SearchPage[j].Substring(pos + HGNot.SearchGameItemPriceStart.Length);
                                                        AutomState = GameListAutomataState.ItemPrice;
                                                    }
                                                    else
                                                    {
                                                        processingline = false;
                                                    }
                                                    break;
                                                case GameListAutomataState.ItemPrice:
                                                    // s is kept
                                                    pos = s.IndexOf(HGNot.SearchGameItemPriceEnd);
                                                    if (pos >= 0)
                                                    {
                                                        s = s.Substring(0, pos);
                                                        try
                                                        {
                                                            GamePrice = Convert.ToInt32(s);
                                                            AutomState = GameListAutomataState.ItemPriceEnd;
                                                        }
                                                        catch
                                                        {
                                                            GamePrice = 0;
                                                            processingline = false;
                                                            AutomState = GameListAutomataState.Undef;
                                                            log = "Error while parsing '" + SearchDir[iPg] + "' on line " + j.ToString() +
                                                                    ": game[" + GameIdxInPage.ToString() + "] price is NaN.";
                                                            if (doLog)
                                                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                                                            Console.WriteLine(log);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        j++;
                                                        if (j < FileLen)
                                                        {
                                                            s += SearchPage[j];
                                                        }
                                                        else
                                                        {
                                                            processingline = false;
                                                            AutomState = GameListAutomataState.Undef;
                                                            log = "Error while parsing '" + SearchDir[iPg] +
                                                                    "': unfinished markup, game[" + GameIdxInPage.ToString() + "] price is not found.";
                                                            if (doLog)
                                                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                                                            Console.WriteLine(log);
                                                        }
                                                    }
                                                    break;
                                                case GameListAutomataState.ItemPriceEnd:
                                                    pos = SearchPage[j].IndexOf(HGNot.SearchGameItemTitleTag);
                                                    if (pos >= 0)
                                                    {
                                                        s = SearchPage[j].Substring(pos + HGNot.SearchGameItemTitleTag.Length);
                                                        AutomState = GameListAutomataState.ItemTitle;
                                                    }
                                                    else
                                                    {
                                                        processingline = false;
                                                    }
                                                    break;
                                                case GameListAutomataState.ItemTitle:
                                                    // s is kept
                                                    pos = s.IndexOf(HGNot.SearchGameItemHrefStart);
                                                    if (pos >= 0)
                                                    {
                                                        AutomState = GameListAutomataState.ItemHref;
                                                        s = s.Substring(pos + HGNot.SearchGameItemHrefStart.Length);
                                                    }
                                                    else
                                                    {
                                                        pos = s.IndexOf(HGNot.SearchGameItemHrefStart2);
                                                        if (pos >= 0)
                                                        {
                                                            AutomState = GameListAutomataState.ItemHref;
                                                            s = s.Substring(pos + HGNot.SearchGameItemHrefStart2.Length);
                                                        }
                                                        else
                                                        {
                                                            j++;
                                                            if (j < FileLen)
                                                            {
                                                                s += SearchPage[j];
                                                            }
                                                            else
                                                            {
                                                                processingline = false;
                                                                AutomState = GameListAutomataState.Undef;
                                                                log = "Error while parsing '" + SearchDir[iPg] +
                                                                        "': unfinished markup, game[" + GameIdxInPage.ToString() + "] href is not found.";
                                                                if (doLog)
                                                                    FileIO.WriteData(ParsingLog, log, out res, out msg);
                                                                Console.WriteLine(log);
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case GameListAutomataState.ItemHref:
                                                    // s is kept
                                                    pos = s.IndexOf(HGNot.SearchGameItemHrefEnd);
                                                    if (pos >= 0)
                                                    {
                                                        GameHref = s.Substring(0, pos);
                                                        AutomState = GameListAutomataState.ItemHrefEnd;
                                                        s = '<' + s.Substring(pos + HGNot.SearchGameItemHrefEnd.Length);
                                                    }
                                                    else
                                                    {
                                                        j++;
                                                        if (j < FileLen)
                                                        {
                                                            s += SearchPage[j];
                                                        }
                                                        else
                                                        {
                                                            processingline = false;
                                                            AutomState = GameListAutomataState.Undef;
                                                            log = "Error while parsing '" + SearchDir[iPg] +
                                                                    "': unfinished markup, game[" + GameIdxInPage.ToString() + "] href is not found.";
                                                            if (doLog)
                                                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                                                            Console.WriteLine(log);
                                                        }
                                                    }
                                                    break;
                                                case GameListAutomataState.ItemHrefEnd:
                                                    // s is kept
                                                    pos = s.IndexOf(HGNot.SearchGameItemEnd);
                                                    if (pos >= 0)
                                                    {
                                                        GameTitle = s.Substring(0, pos).DeTag().Trim();
                                                        AutomState = GameListAutomataState.ItemEnd;
                                                        s = s.Substring(pos + HGNot.SearchGameItemEnd.Length);
                                                        FileIO.WriteData(FNameSearchRes,
                                                            GameID.ToString() + '\t' + GamePrice.ToString() + '\t' + GameHref + '\t' + GameTitle,
                                                            out res, out msg);
                                                    }
                                                    else
                                                    {
                                                        j++;
                                                        if (j < FileLen)
                                                        {
                                                            s += SearchPage[j];
                                                        }
                                                        else
                                                        {
                                                            processingline = false;
                                                            AutomState = GameListAutomataState.Undef;
                                                            log = "Error while parsing '" + SearchDir[iPg] +
                                                                    "': unfinished markup, game[" + GameIdxInPage.ToString() + "] title is not found.";
                                                            if (doLog)
                                                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                                                            Console.WriteLine(log);
                                                        }
                                                    }
                                                    break;
                                                case GameListAutomataState.ItemEnd:
                                                    // s is kept
                                                    pos = s.IndexOf(HGNot.SearchGameListEnd);
                                                    if (pos >= 0)
                                                    {
                                                        processingline = false;
                                                        AutomState = GameListAutomataState.ListEnd;
                                                    }
                                                    else
                                                    {
                                                        pos = s.IndexOf(HGNot.SearchGameItemStart);
                                                        if (pos >= 0)
                                                        {
                                                            AutomState = GameListAutomataState.Item;
                                                        }
                                                        else
                                                        {
                                                            processingline = false;
                                                            if (j + 1 < FileLen)
                                                            {
                                                                s = SearchPage[j + 1];
                                                            }
                                                            else
                                                            {
                                                                processingline = false;
                                                                AutomState = GameListAutomataState.Undef;
                                                                log = "Error while parsing '" + SearchDir[iPg] +
                                                                        "': unfinished markup, game[" + GameIdxInPage.ToString() + "] list end is not found.";
                                                                if (doLog)
                                                                    FileIO.WriteData(ParsingLog, log, out res, out msg);
                                                                Console.WriteLine(log);
                                                            }
                                                        }
                                                    }
                                                    break;
                                                case GameListAutomataState.ListEnd:
                                                    j = FileLen;
                                                    processingline = false;
                                                    break;
                                            }
                                    }
                                    j++;
                                }
                                #endregion extracting

                                log = "Finished parsing file '" + SearchDir[iPg] + "'";
                                if (doLog)
                                    FileIO.WriteData(ParsingLog, log, out res, out msg);
                                Console.WriteLine(log);
                                res = true;
                            }
                            else
                            {
                                log = "Error in GetHGGamePages: error while reading file '" + SearchDir[iPg] + "'";
                                if (doLog)
                                    FileIO.WriteData(ParsingLog, log, out res, out msg);
                                Console.WriteLine(log);
                            }
                        }

                        log = "GetHGGamePages finished, filled file '" + FNameSearchRes + "'";
                        if (doLog)
                            FileIO.WriteData(ParsingLog, log, out res, out msg);
                        Console.WriteLine(log);
                    }
                    else
                    {
                        log = "Error in GetHGGamePages: error while reading file '" + FNameSearchDir_Full + "'";
                        if (doLog)
                            FileIO.WriteData(ParsingLog, log, out res, out msg);
                        Console.WriteLine(log);
                    }   
                }
                else
                {
                    log = "Error in GetHGGamePages: no out file '" + FNameSearchRes + "' available for writing: '" + msg + "'";
                    if (doLog)
                        FileIO.WriteData(ParsingLog, log, out res, out msg);
                    Console.WriteLine(log);
                }
            }
            Console.ReadLine();
        }

        /// <summary>Извлечение списка всех производителей игр</summary>
        public static void GetHGManufList()
        {
            bool res, doLog = true;
            string msg, log, s, value;
            bool flag = FileIO.FileExists(ParsingLog, out res, out msg);
            if (!res)
            {
                doLog = false;
                Console.WriteLine("GetHGManufList: No logging available! Error: '" + msg + "'");
                Console.Write("Continue anyway? Press y|anything:");
                msg = "";
                while (msg == "")
                {
                    msg = Console.ReadLine().Trim();
                    if (msg.Length > 0)
                    {
                        res = ((msg[0] == 'y') || (msg[0] == 'Y'));
                        if (!res)
                            Console.WriteLine("Cancelling...");
                    }
                }
            }
            if (res)
            {
                FileIO.ClearFile(FNameManufacturers, out res, out msg);
                if (res)
                {
                    log = "#" + TimeStamping.StringStamp_YYMMDD() + "_" + TimeStamping.StringStamp_HHMMSS() + " GetHGManufList";
                    if (doLog)
                        FileIO.WriteData(ParsingLog, log, out res, out msg);
                    Console.WriteLine(log);

                    int i, j, SearchDirLen, FileLen;
                    string[] SearchDir = FileIO.ReadAllLines(FNameSearchDir_Full, out SearchDirLen, out res, out msg);
                    if (res)
                    {
                        string[] SearchPage;
                        List<string> ManufList = new List<string>();

                        SearchPage = FileIO.ReadAllLines(SearchDir[0], out FileLen, out res, out msg);
                        if (res)
                        {
                            #region extracting
                            s = "";
                            j = 0;
                            bool searchforstart = true;
                            while (j < FileLen)
                            {
                                if (searchforstart)
                                {
                                    i = SearchPage[j].IndexOf(HGNot.Search_ManufListStart);
                                    if (i >= 0)
                                    {
                                        searchforstart = false;
                                        s = SearchPage[j].Substring(i + HGNot.Search_ManufListStart.Length);
                                    }
                                }
                                else
                                {
                                    i = SearchPage[j].IndexOf(HGNot.Search_ManufListEnd);
                                    if (i < 0)
                                    {
                                        s += SearchPage[j];
                                        i = s.IndexOf(HGNot.Search_ManufListLiEnd);
                                        if (i >= 0)
                                        {
                                            value = s.Substring(0, i).DeTag().Trim();
                                            FileIO.WriteData(FNameManufacturers, value, out res, out msg);
                                            s = s.Substring(i + HGNot.Search_ManufListLiEnd.Length);
                                        }
                                    }
                                    else
                                    {
                                        s += SearchPage[j].Substring(0, i);
                                        i = s.IndexOf(HGNot.Search_ManufListLiEnd);
                                        if (i >= 0)
                                        {
                                            value = s.Substring(0, i).DeTag().Trim();
                                            FileIO.WriteData(FNameManufacturers, value, out res, out msg);
                                        }
                                        break;
                                    }
                                }
                                j++;
                            }
                            #endregion extracting

                            log = "Successfully filled file '" + FNameManufacturers + "'";
                            if (doLog)
                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                            Console.WriteLine(log);
                            res = true;
                        }
                        else
                        {
                            log = "Error in GetHGManufList: error while reading file '" + SearchDir[0] + "'";
                            if (doLog)
                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                            Console.WriteLine(log);
                        }
                    }
                    else
                    {
                        log = "Error in GetHGManufList: error while reading file '" + FNameSearchDir_Full + "'";
                        if (doLog)
                            FileIO.WriteData(ParsingLog, log, out res, out msg);
                        Console.WriteLine(log);
                    }
                }
                else
                {
                    log = "Error in GetHGManufList: no out file '" + FNameManufacturers + "' available for writing: '" + msg + "'";
                    if (doLog)
                        FileIO.WriteData(ParsingLog, log, out res, out msg);
                    Console.WriteLine(log);
                }
            }
            Console.ReadLine();
        }

        /// <summary>Проверка корректности выгрузки игр (собирается статистика, возможна неполная выгрузка,
        /// проверяются дубликаты (а поисковые страницы, похоже, намеренно показывают дубликаты некоторых игр; вероятно, "реклама, сэр!")</summary>
        public static void CheckGamesDir()
        {
            bool res = true;
            string msg, log, s;
            int NDir, NSearch;
            bool doLog;
            FileIO.WriteData(ParsingLog, "", out doLog, out msg);
            if (!doLog)
            {
                Console.WriteLine("CheckGamesDir: No logging available! Error: '" + msg + "'");
                Console.Write("Continue anyway? Press y|anything:");
                msg = "";
                while (msg == "")
                {
                    msg = Console.ReadLine().Trim();
                    if (msg.Length > 0)
                    {
                        res = ((msg[0] == 'y') || (msg[0] == 'Y'));
                        if (!res)
                            Console.WriteLine("Cancelling...");
                    }
                }
            }
            if (res)
            {
                log = "#" + TimeStamping.StringStamp_YYMMDD() + "_" + TimeStamping.StringStamp_HHMMSS() + " CheckGamesDir";
                if (doLog)
                    FileIO.WriteData(ParsingLog, log, out res, out msg);
                Console.WriteLine(log);

                string[] Dir = FileIO.ReadAllLines(FNameGamesDir_Full, out NDir, out res, out msg);
                if (res)
                {
                    string[] Search = FileIO.ReadAllLines(FNameSearchRes, out NSearch, out res, out msg);
                    if (res)
                    {
                        log = "NDir = " + NDir.ToString() + " / NSearch = " + NSearch.ToString() + (NDir == NSearch ? " / sizes match" : " / sizes don't match. Difference:");
                        if (doLog)
                            FileIO.WriteData(ParsingLog, log, out res, out msg);
                        Console.WriteLine(log);
                        if (NDir != NSearch)
                        {
                            char[] Splitters = new char[1] { '\t' };
                            int[] DirIDs = new int[NDir];
                            int i, j, id, NNotFound = 0;
                            bool found;
                            for (j = 0; j < NDir; j++)
                            {
                                s = FileIO.ShortName(Dir[j]).Substring(4);
                                s = s.Substring(0, s.Length - 4);
                                try
                                {
                                    DirIDs[j] = Convert.ToInt32(s);
                                }
                                catch
                                {
                                    log = "Error in Dir[" + j.ToString() + "] file name - no ID found: '" + FileIO.ShortName(Dir[j]) + "'";
                                    if (doLog)
                                        FileIO.WriteData(ParsingLog, log, out res, out msg);
                                    Console.WriteLine(log);
                                    DirIDs[j] = -1;
                                }
                            }
                            for (i = 0; i < NSearch; i++)
                            {
                                found = false;
                                id = Search[i].ExtractFieldInt(0, Splitters);
                                if (id < 0)
                                {
                                    log = "Error in Search[" + i.ToString() + "]: ID == -1";
                                    if (doLog)
                                        FileIO.WriteData(ParsingLog, log, out res, out msg);
                                    Console.WriteLine(log);
                                }
                                else
                                {
                                    for (j = 0; j < NDir; j++)
                                        if ((DirIDs[j] != -1) && (id == DirIDs[j]))
                                        {
                                            found = true;
                                            break;
                                        }
                                    if (!found)
                                    {
                                        NNotFound++;
                                        log = "Not found Search[" + i.ToString() + "]: ID == " + id.ToString();
                                        if (doLog)
                                            FileIO.WriteData(ParsingLog, log, out res, out msg);
                                        Console.WriteLine(log);
                                    }
                                }
                            }
                            log = "Not found from Search: " + NNotFound.ToString() + " IDs";
                            if (doLog)
                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                            Console.WriteLine(log);

                            NNotFound = 0;
                            for (i = 0; i < NSearch; i++)
                            {
                                id = Search[i].ExtractFieldInt(0, Splitters);
                                for (j = i + 1; j < NSearch; j++)
                                {
                                    if (id == Search[j].ExtractFieldInt(0, Splitters))
                                    {
                                        NNotFound++;
                                        log = "Search[" + i.ToString() + "]  == Search[" + j.ToString() + "] == " + id.ToString();
                                        if (doLog)
                                            FileIO.WriteData(ParsingLog, log, out res, out msg);
                                        Console.WriteLine(log);
                                        log = "Search[" + i.ToString() + "]  : " + Search[i].ExtractField(2, Splitters);
                                        if (doLog)
                                            FileIO.WriteData(ParsingLog, log, out res, out msg);
                                        Console.WriteLine(log);
                                        log = "Search[" + j.ToString() + "]  : " + Search[j].ExtractField(2, Splitters);
                                        if (doLog)
                                            FileIO.WriteData(ParsingLog, log, out res, out msg);
                                        Console.WriteLine(log);
                                        break;
                                    }
                                }
                            }
                            log = "Duplicates found in Search:" + NNotFound.ToString();
                            if (doLog)
                                FileIO.WriteData(ParsingLog, log, out res, out msg);
                            Console.WriteLine(log);
                        }
                    }
                    else
                        Console.WriteLine("Error in CheckGamesDir: error reading SearchRes file '" + FNameSearchRes + "'");
                }
                else
                    Console.WriteLine("Error in CheckGamesDir: error reading Dir file '" + FNameGamesDir_Full + "'");
            }
            Console.ReadLine();
        }

        /// <summary>Извлечение списка всех тэгов игр (включая метки серий)</summary>
        public static void GetHGTagsList(ref List<GameParams> Games)
        {
            bool res, doLog = true;
            string msg, log;
            bool flag = FileIO.FileExists(ParsingLog, out res, out msg);
            if (!res)
            {
                doLog = false;
                Console.WriteLine("GetHGManufList: No logging available! Error: '" + msg + "'");
                Console.Write("Continue anyway? Press y|anything:");
                msg = "";
                while (msg == "")
                {
                    msg = Console.ReadLine().Trim();
                    if (msg.Length > 0)
                    {
                        res = ((msg[0] == 'y') || (msg[0] == 'Y'));
                        if (!res)
                            Console.WriteLine("Cancelling...");
                    }
                }
            }
            if (res)
            {
                FileIO.ClearFile(FNameTags, out res, out msg);
                if (res)
                {
                    log = "#" + TimeStamping.StringStamp_YYMMDD() + "_" + TimeStamping.StringStamp_HHMMSS() + " GetHGTagsList";
                    if (doLog)
                        FileIO.WriteData(ParsingLog, log, out res, out msg);
                    Console.WriteLine(log);

                    int i, j, NTags = 0, curNTags, NGames = Games.Count;
                    List<string> curTags, TagsList = new List<string>();
                    for (i = 0; i < NGames; i++)
                    {
                        curTags = Games[i].Tags;
                        curNTags = curTags.Count;
                        for (j = 0; j < curNTags; j++)
                            if (!TagsList.Contains(curTags[j]))
                            {
                                TagsList.Add(curTags[j]);
                                NTags++;
                            }
                    }
                    TagsList.Sort();
                    FileIO.WriteData(FNameTags, TagsList, out res, out msg);
                                            
                    log = "Successfully filled file '" + FNameTags + "', tags retrieved: " + NTags.ToString();
                    if (doLog)
                        FileIO.WriteData(ParsingLog, log, out res, out msg);
                    Console.WriteLine(log);
                    res = true;
                }
                else
                {
                    log = "Error in GetHGTagsList: no out file '" + FNameTags + "' available for writing: '" + msg + "'";
                    if (doLog)
                        FileIO.WriteData(ParsingLog, log, out res, out msg);
                    Console.WriteLine(log);
                }
            }
            Console.ReadLine();
        }

        ///<summary>Точка входа в метод, который создаёт список игр (оно же образец использования)</summary>
        ///<param name="doConsoleLog">Требуется ли логирование в консоль (логирование в файл доступно, если доступен константный путь к файлу)</param>
        ///<param name="doFormTagsDictionary">Требуется ли создавать и дампить на диск словарь тэгов</param>
        ///<param name="doCheckDropOut">Для запуска проверки, остались ли после фильтрации по тегам объекты, не имевшие игровых параметров</param>
        public static void GetGameInstances(bool doConsoleLog, bool doFormTagsDictionary, bool doCheckDropOut)
        {
            // Кодировка на HobbyGames стоит UTF-8
            System.Text.Encoding enc = EncUTF8;

            bool res; string msg;
            bool doLog;

            // Подготовка к логированию
            FileIO.WriteData(ParsingLog, "", out doLog, out msg);
            if (!doLog)
            {
                if (doConsoleLog)
                {
                    Console.WriteLine("CheckGamesDir: No logging available! Error: '" + msg + "'");
                    Console.Write("Continue anyway? Press y|anything:");
                    msg = "";
                    while (msg == "")
                    {
                        msg = Console.ReadLine().Trim();
                        if (msg.Length > 0)
                        {
                            res = ((msg[0] == 'y') || (msg[0] == 'Y'));
                            if (!res)
                                Console.WriteLine("Cancelling...");
                        }
                    }
                }
            }

            // Загрузка словаря производителей игр из файла
            ManufacturersDictionary ManufacturersDict = new ManufacturersDictionary(FNameManufacturers, out res, out msg);
            if (!res)
                ManufacturersDict = new ManufacturersDictionary();

            List<string> TagsDict = Categories.GetDictionary(FNameTags, out res, out msg);
            if (!res)
                TagsDict = new List<string>();
            List<string> CategoriesDict = Categories.GetDictionary(FNameCategories, out res, out msg);
            if (!res)
                CategoriesDict = new List<string>();
            List<string> OptionalTagsDict = Categories.GetDictionary(FNameOptions, out res, out msg);
            if (!res)
                OptionalTagsDict = new List<string>();
            List<string> ThematicDict = Categories.GetDictionary(FNameThematic, out res, out msg);
            if (!res)
                ThematicDict = new List<string>();
            List<string> BlackListDict = Categories.GetDictionary(FNameBlackList, out res, out msg);
            if (!res)
                BlackListDict = new List<string>();

            // настройки для извлечения игр из Html, читай описание метода GetGamesFromHtml
            System.Text.Encoding encDir = enc;
            System.Text.Encoding encHtml = enc;
            bool doGetGameTextDes = true;
            bool ParamsZeroBasedScale = false;
            bool doExcludeErroneousGames = !doCheckDropOut;
            List<int> ErroneousGamesIndices;
            List<int> NotBoardGamesIndices;
            List<string> msgs;
            List<GameParams> Games = GamesFromHtml.GetGamesFromHtml(FNameGamesDir_Full,
                encDir, encHtml, doGetGameTextDes, ParamsZeroBasedScale, doExcludeErroneousGames, out ErroneousGamesIndices, out NotBoardGamesIndices,
                ref ManufacturersDict, out res, out msgs);

            int ig, NG, NDropped = 0;
            if (doCheckDropOut)
            {
                // This version does controlling as well
                int NCorrupt;
                List<int> UndroppedIndexes, CorruptIndexes = FileIO.ReadFileLinesToListInt(FNameCorruptIndexes, out NCorrupt, out res, out msg);
                if (res)
                {
                    Games.DropBlackList(BlackListDict, out NDropped, CorruptIndexes, out UndroppedIndexes);
                    NG = UndroppedIndexes.Count;
                    msg = "Undropped indexes: " + NG.ToString() + (NG > 0 ? " as follows" : ":");
                    msgs.Add(msg);
                    if (NG > 0)
                    {
                        msg = "";
                        for (ig = 0; ig < NG; ig++)
                            msg += UndroppedIndexes[ig].ToString() + ", ";
                    }
                    else
                        msg = "-";
                    msgs.Add(msg);
                }
                else
                {
                    msg = "Error when DropBlackList (..., out UndroppedIndexes) while ReadFileLinesToListInt: " + msg;
                    msgs.Add(msg);
                }
            }
            else
            {
                // This version works by default
                Games.DropBlackList(BlackListDict, out NDropped);
            }

            NG = Games.Count;
            GameParams Gi;
            for (ig = 0; ig < NG; ig++)
            {
                Gi = Games[ig];
                Gi.Categories = Categories.SelectCategories(Gi.Tags, CategoriesDict);
                Gi.Thematic = Categories.SelectThematic(Gi.Tags, ThematicDict);
                Gi.OptionalTags = Categories.SelectOptionalTags(Gi.Tags, OptionalTagsDict);
                Gi.Series = Categories.SelectSeries(Gi.Tags, CategoriesDict, BlackListDict, OptionalTagsDict, ThematicDict);
                Gi.ExpandThematic();
            }
            // ^ на выходе список игр Games

            // добьём словарь тэгов (включает категории)
            if (doFormTagsDictionary)
                GetHGTagsList(ref Games);

            // дальше вывод лога
            int i, N = msgs.Count;
            if (doConsoleLog)
                for (i = 0; i < N; i++)
                    Console.WriteLine(msgs[i]);
            if (doLog)
                for (i = 0; i < N; i++)
                    FileIO.WriteData(ParsingLog, msgs[i], out res, out msg);
            N = ErroneousGamesIndices.Count;
            if (doExcludeErroneousGames)
            {
                msg = "Indices of erroneous games in the Dir:";
                if (doConsoleLog)
                    Console.WriteLine(msg);
                if (doLog)
                    FileIO.WriteData(ParsingLog, msg, out res, out msg);
            }
            else
            {
                msg = "Indices of erroneous games in the list:";
                if (doConsoleLog)
                    Console.WriteLine(msg);
                if (doLog)
                    FileIO.WriteData(ParsingLog, msg, out res, out msg);
            }
            msg = N > 0 ? "" : "-";
            for (i = 0; i < N; i++)
                msg += ErroneousGamesIndices[i].ToString() + ", ";
            if (doConsoleLog)
                Console.WriteLine(msg);
            if (doLog)
                FileIO.WriteData(ParsingLog, msg, out res, out msg);
            
            N = NotBoardGamesIndices.Count;
            msg = "Indices of items, which showed to be not board games, in the Dir:";
            if (doConsoleLog)
                Console.WriteLine(msg);
            if (doLog)
                FileIO.WriteData(ParsingLog, msg, out res, out msg);
            msg = N > 0 ? "" : "-";
            for (i = 0; i < N; i++)
                msg += NotBoardGamesIndices[i].ToString() + ", ";
            if (doConsoleLog)
                Console.WriteLine(msg);
            if (doLog)
                FileIO.WriteData(ParsingLog, msg, out res, out msg);

            msg = "Items dropped from black list of tags: " + NDropped.ToString();
            if (doConsoleLog)
                Console.WriteLine(msg);
            if (doLog)
                FileIO.WriteData(ParsingLog, msg, out res, out msg);

            if (doConsoleLog)
            {
                Console.Write(msg);
                Console.WriteLine();
                Console.ReadKey();
            }
        }
    }
}
