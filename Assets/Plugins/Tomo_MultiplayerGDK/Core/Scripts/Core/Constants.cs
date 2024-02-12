

namespace TomoClub.Core
{
	public static class Constants
	{
		public static readonly string MainMenuScene = "MainMenu";
		public static readonly string LobbyScene = "Lobby";
		public static readonly string TeamAssignScene = "TeamAssign";
		public static readonly string GameScene = "Game";
		public static readonly string TempScene = "Temp";

		public static class Game
		{
			public static readonly string ServerRegion = "ServerRegion";
		}

		public static class Player
		{
			public static readonly string PlayerName = "PlayerName";
			public static readonly string PlayerUserID = "UserID";
			public static readonly string PlayerTimeout = "PlayerTimeOut";
			public static readonly string ArenaNo = "ArenaNo";
			public static readonly string TeamNo = "TeamNo";
			public static readonly string TutorialCompleted = "TutorialDone";
			public static readonly string Ping = "Ping";

		}

		public static class Room
		{
			public static readonly string GameTime = "GameTime";
			public static readonly string AvailableArenas = "NoOfArenas";
			public static readonly string OccupiedArenas = "OccupiedArenas";
			public static readonly string RoomState = "RoomState";
		}


		public static class NetworkEvents
		{
			public static readonly byte CloseRoomForEveryone = 1;
			public static readonly byte CloseRoomForPlayer = 2;
			public static readonly byte PlayerIsReady = 3;
			public static readonly byte SyncGameSettings = 4;
		}


	} 
}




