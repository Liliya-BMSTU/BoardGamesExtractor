using System;
using System.Collections.Generic;

namespace BoardGamesExtractor
{
    class Program
    {
        /// <summary>Entry points for key methods</summary>
        static void Main(string[] args)
        {
            //////////////////////////////////////////////////
            /// WARNING! Check the ROOF_DIR.cs file:       ///
            /// you should set up the ROOT_PATH_BASE const ///
            //////////////////////////////////////////////////



            // This code extracts all of the search pages with game IDs and links (see inside About.txt)

            // GettingHGPages.GetHGSearchPages();         // done 20180505
            /*int Amount;
            GettingHGPages.GetSearchPagesAmount(out Amount);    // done 20180509
            Console.WriteLine("Search pages: " + Amount.ToString());
            Console.ReadKey();
            */

            // This code forms a List of all available manufacturers
            //GettingHGPages.GetHGManufList();           // done 20180508

            // This code extracts all of the games
            //GettingHGPages.GetHGGamesList();            // done 20180509, redone 20180510
            //GettingHGPages.GetHGGamePages();            // done 20180509, but sizes mismatch /// see BGE_VersionInfo // correctly @20180510
            // This code checks whether all of the games from search are present (and states whether there were duplicates)
            //GettingHGPages.CheckGamesDir();             // done 20180910, redone 20180510
            
            // This code forms a List of all games (List<GameParams>) inside
            bool doConsoleLog = true;
            bool doFormTagsDictionary = true;
            bool doCheckDropOut = false;             // true; done 20180523
            GettingHGPages.GetGameInstances(doConsoleLog, doFormTagsDictionary, doCheckDropOut);  // done 20180520
        }
    }
}
