using UnityEngine;
using TomoClub.Core;


namespace TomoClub.Core
{

	[CreateAssetMenu(menuName = "Settings/Game Settings")]
	public class GameSettings : ScriptableObject
	{
		[Header("Game Settings")]
		public string gameName;
		[Tooltip("For Classroom session, there are three types of builds:\nClassroom_Mod: Gives access to moderator tools; " +
			"\nClassroom_Player: For students joining a moderated session; \nClassroom_Common: Gives the user option to select between Classroom_Mod and Classroom_Player" +
			"\n\nStandard: Normal Build, in which players can create their own rooms and join any room")]
		public BuildType buildType;//Standard builds have not been implemented
		[Tooltip("Should we region lock the build")]
		public bool isRegionLocked = false;
		[Tooltip("The Region you want to lock the build to")]
		public Regions buildRegion = Regions.India; 
		[Tooltip("Their are two game types. \nInter-Arena: Competition between arenas, where each arena is a team" +
			"\nIntra-Arena: Competion within each arena, each area has teams that compete with each other")]
		public GameType gameType;
		[Tooltip("Authentication can be done using the usernames, or a generated key, both of these get stored in playerprefs")]
		public AuthenticationType authenticationType;
		[Tooltip("Should the application run in background, should always be true")]
		public bool canRunInBackground = true; //Keep the application running in background

		[Header("Testing Settings")]
		[Tooltip("Uses non-persistant data, turn this on while testing multiple instances on the same device/browser")]
		public bool inTestingMode = false;
		[Tooltip("Clears all previous cached data")]
		public bool clearPlayerPrefs = true;

		[Header("WebGL Settings")]
		[Tooltip("The target frame rate for this game, -1 means no target frame rate")]
		public int targetFrameRate_webGL = -1;
		[Tooltip("Link of player build on itch.io, use account info@tomoclub.org")]
		public string webGLHostLink;

		[Header("Standalone Settings")]
		public int targetFrameRate_standalone = -1;
			
		[Header("Session Settings")]
		[Tooltip("Show Splash Screen")]
		public bool showSplashScreen = false;
		[Tooltip("How long do you want to show the splash screen before starting the game, shouldn't be less than 1 second")]
		public float splashScreenTTL = 3f;
		[Tooltip("Default game time for the session in seconds")]
		public int sessionGameTime = 600;

		[Header("Photon Connection Settings")]
		[Tooltip("Players joining any room would auto sync to the master client's scene")]
		public bool automaticSyncing = true; //Sync incoming players to the master client's scene
		[Tooltip("Amount of time till the player will stay active and recieve RPC's and network events in the background")]
		public float timeAliveInBackground = 120000; //120000ms = 2 mins

		[Header("Photon Room Settings")]
		[Tooltip("Time a player data will stay alive on the network after becoming inactive, -1 means it'll never be destroyed till the room is destroyed")]
		public int playerTTL = 120000;
		[Tooltip("Time till a room stays alive on the network after all the players have been either inactive or been removed")]
		public int emptyRoomTTL = 60000; // In miliseconds
		[Tooltip("Removes the player events and data when the player disconnects")]
		public bool cleanupCacheOnLeave = false;


	} 
}
