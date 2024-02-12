using Photon.Realtime;

namespace TomoClub.Core
{
	public static class SessionData
	{
		//Game Details
		public static string GameName { get; set; }
		public static bool connectionEstablished = false;
		public static BuildType BuildType;
		public static Regions BuildRegion;
		public static bool IsRegionLocked;
		public static MainMenu.Builds Build;
		public static AuthenticationType AuthenticationType;
		public static bool TestMode;

		public static GameType GameType;
		public static TypedLobby gameLobby;

		public static string webGLLink;

		//Session Details
		public static bool showSplashScreen;
		public static float splashScreenTTL;

		//Game States
		public static GameStates previousGameState = GameStates.Null;
		public static GameStates currentGameState = GameStates.MainMenu;

		public static bool ForceReconnect = false;

		public static void ResetArenaTeamLists()
		{
			for(int i = 0; i < MultiplayerManager.Instance.occupiedArenas; i++)
			{
				MultiplayerManager.Instance.arenaTeamLists[i].redTeamPlayers.Clear();
                MultiplayerManager.Instance.arenaTeamLists[i].blueTeamPlayers.Clear();
            }
		}

		public static void ResetTeamData()
		{
			LocalPlayer.Instance.teamNo = -1;
			LocalPlayer.Instance.teamName = TeamName.None;
		}

	} 
}