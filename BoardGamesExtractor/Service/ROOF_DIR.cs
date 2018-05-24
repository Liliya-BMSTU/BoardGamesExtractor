using System;

namespace BoardGamesExtractor
{
    /// <summary>Directories for experiments on different target computers</summary>
    class RoofDir
    {
        /// <summary>Path to BoardGamesExtractor containing data and project folders (ending with "BoardGamesExtractor\").</summary>
        public const string ROOT_PATH_BASE =
            @"C:\Lil'Jaguar DELL\Coding materials\C#\BoardGamesExtractor\"; // DELL
            //@"D:\YourPath\BoardGamesExtractor\";   // Fill this in, otherwise the stuff won't work
        
        /// <summary>Path to HobbyGames input data ("HobbyGamesIN\").</summary>
        public const string ROOT_HOBBYGAMES_IN = ROOT_PATH_BASE + @"HobbyGamesIN\";
        /// <summary>Path to HobbyGames search data - games list ("HobbyGamesIN\Search\").</summary>
        public const string ROOT_HOBBYGAMES_IN_SEARCH = ROOT_HOBBYGAMES_IN + @"Search\";
        /// <summary>Path to HobbyGames games data ("HobbyGamesIN\Games\").</summary>
        public const string ROOT_HOBBYGAMES_IN_GAMES = ROOT_HOBBYGAMES_IN + @"Games\";

        /// <summary>Path for dumping results for HobbyGames ("HobbyGamesOUT\").</summary>
        public const string ROOT_HOBBYGAMES_OUT = ROOT_PATH_BASE + @"HobbyGamesOUT\";
    }
}
