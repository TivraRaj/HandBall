using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;
using UnityEngine;
using Random = System.Random;
using UnityEngine.SceneManagement;

using TomoClub.Util;

namespace TomoClub.Core
{
	#region Global Multiplayer Data and Events
	public static class MultiplayerMesseges
	{
		public static Action<List<Player>> OnUpdateMyArenaList;
		public static Action<ArenaList[]> OnUpdateArenaLists;
		public static Action OnUpdateArenaTeamLists;
		public static Action<List<Player>> OnUpdateLobbyList;
		public static Action<List<Player>> OnUpdateGamePlayerList;
		public static Action<int, int> OnUpdatePlayerArena;
		public static Action<int> OnUpdatePing;
	}

	[Serializable]
	public class ArenaList
	{
		public List<Player> arenaPlayers = new List<Player>();
	}

	[Serializable]
	public class ArenaTeamList
	{
		public List<Player> redTeamPlayers = new List<Player>();
		public List<Player> blueTeamPlayers = new List<Player>();
	}

	#endregion


	public class MultiplayerManager : MonoBehaviour
	{
		[Header("Game Settings")]
		[Tooltip("Settings for the current game, make sure to change them according to design specs")]
		public GameSettings gameSettings;

		[Tooltip("Settings for current game's room")]
		public RoomSettings roomSettings;

		/// <summary>
		/// All the players in the lobby
		/// </summary>
		[Header("Current Room Player Lists - For debugging purposes only")]
		public List<Player> lobbyPlayers = new List<Player>();

		/// <summary>
		/// All the players in the lobby who can play the game (Player list excluding the mod)
		/// </summary>
		public List<Player> gamePlayers = new List<Player>();

		/// <summary>
		/// These are list of players for each arena
		/// </summary>
		public ArenaList[] arenaLists;

		/// <summary>
		/// These are list of players for red and blue team in each arena
		/// </summary>
		public ArenaTeamList[] arenaTeamLists;

		[Header("Current Room Data - For debugging purposes only")]

		/// <summary>
		///No of available arenas in the room that can be filled
		/// </summary>
		public int availableArenas = 0;

		/// <summary>
		/// No of arenas in the room that have been occupied. Use this for any gameplay logic
		/// </summary>
		public int occupiedArenas = 0;

		/// <summary>
		/// Current room's state
		/// </summary>
		public RoomState currentRoomState = RoomState.Null;

		/// <summary> 
		/// Max players that this current room can have, '-1' if room doesn't exist
		/// </summary>
		public int currentRoomMaxPlayers => PhotonNetwork.CurrentRoom == null ? -1 : PhotonNetwork.CurrentRoom.MaxPlayers;

		/// <summary>
		/// Current room name, empty if room doesn't exist
		/// </summary>
		public string currentRoomName => PhotonNetwork.CurrentRoom == null ? "" : PhotonNetwork.CurrentRoom.Name;

		/// <summary>
		/// Each room can have players between playersPerRoomRange.x and playersPerRoomRange.y. Both inclusive
		/// </summary>
		public Vector2Int playersPerRoomRange => roomSettings.totalPlayersRange;

		/// <summary>
		/// Each room can have arenas between arenasPerRoomRange.x and arenasPerRoomRange.y. Both inclusive
		/// </summary>
		public Vector2Int arenasPerRoomRange => roomSettings.totalArenaRange;

		/// <summary>
		/// Each arena can have players between playersPerArenaRange.x and playersPerArenaRange.y. Both inclusive
		/// </summary>
		public Vector2Int playersPerArenaRange => roomSettings.perArenaPlayersRange;


		//Global Hashtables for custom properties
		public Hashtable playerProperties;
		public Hashtable roomProperties;

		public Room currentRoom => PhotonNetwork.CurrentRoom;

		public int Specators { get; set; }
		public int MaxPlayers => currentRoomMaxPlayers - Specators;

		private int m_gameSessionTime = 0;
		//Game Session Data
		public int GameSessionTime
		{
			get => SessionData.TestMode ? m_gameSessionTime : PlayerPrefs.GetInt(Constants.Room.GameTime, gameSettings.sessionGameTime);
			set
			{
				if (SessionData.TestMode)
				{
					m_gameSessionTime = value;
				}
				else
				{
					PlayerPrefs.GetInt(Constants.Room.GameTime, value);
				}
			}

		}

		private bool onGoingArenaAssignment = false;
		private string playerNotAssignedMessage;

		private CounterDown pingCheckTimer;

		public static MultiplayerManager Instance;

		private void Awake()
		{
			if(Instance == null)
			{
				DontDestroyOnLoad(gameObject);
				Instance = this;
				
			}

			Init();

		}

		private void Update()
		{
			pingCheckTimer.UpdateCounter();
		}

		private void Init()
		{
			pingCheckTimer = new CounterDown(5);
			pingCheckTimer.StartCounter();

			m_gameSessionTime = gameSettings.sessionGameTime;

			playerProperties = new Hashtable();
			playerProperties.Add(Constants.Player.ArenaNo, -1);
			playerProperties.Add(Constants.Player.TeamNo, -1);
			playerProperties.Add(Constants.Player.Ping, 0);

			roomProperties = new Hashtable();
			roomProperties.Add(Constants.Room.AvailableArenas, availableArenas);
			roomProperties.Add(Constants.Room.OccupiedArenas, occupiedArenas);
			roomProperties.Add(Constants.Room.RoomState, (int)currentRoomState);

			arenaLists = new ArenaList[arenasPerRoomRange.y];
			arenaTeamLists = new ArenaTeamList[arenasPerRoomRange.y];

			for (int i = 0; i < arenaLists.Length; i++)
			{
				arenaLists[i] = new ArenaList();
			}

			for (int i = 0; i < arenaTeamLists.Length; i++)
			{
				arenaTeamLists[i] = new ArenaTeamList();
			}

		}

		private void OnEnable()
		{
			ServerMesseges.OnRoomPropertiesUpdated += UpdateRoomProperties;
			ServerMesseges.OnPlayerPropertiesUpdated += OnPlayerPropertiesUpdated;
			ServerMesseges.OnPlayerJoinedRoom += UpdateAllLists;
			ServerMesseges.OnPlayerLeftRoom += UpdateAllLists;

			ServerMesseges.OnJoinRoomSuccessful += SetPlayerProperties;

			SceneManager.sceneLoaded += OnSceneLoaded;

			pingCheckTimer.CounterCompleted += UpdatePing;
		}

		private void OnDisable()
		{
			ServerMesseges.OnRoomPropertiesUpdated -= UpdateRoomProperties;
			ServerMesseges.OnPlayerPropertiesUpdated -= OnPlayerPropertiesUpdated;
			ServerMesseges.OnPlayerJoinedRoom -= UpdateAllLists;
			ServerMesseges.OnPlayerLeftRoom -= UpdateAllLists;

			SceneManager.sceneLoaded -= OnSceneLoaded;

			ServerMesseges.OnJoinRoomSuccessful -= SetPlayerProperties;

			pingCheckTimer.CounterCompleted -= UpdatePing;
		}

		private void UpdatePing()
		{
			if(SessionData.connectionEstablished && SessionData.currentGameState != GameStates.InGame)
			{
				int ping = PhotonNetwork.GetPing();
				Hashtable hashtable = new Hashtable();
				hashtable.Add(Constants.Player.Ping, ping);
				LocalPlayer.Instance.player.SetCustomProperties(hashtable);

				MultiplayerMesseges.OnUpdatePing?.Invoke(ping);
			}

			pingCheckTimer.RestartCounter();
		}


		private void SetPlayerProperties()
		{
			
			if(LocalPlayer.Instance.defaultPlayerType == PlayerType.Player)
			{
				SetPlayerPropertiesOnNetwork(-1);
			}
			else if(LocalPlayer.Instance.defaultPlayerType == PlayerType.Spectator)
			{
				SetPlayerPropertiesOnNetwork(0);
			}
			
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if (scene.name == Constants.LobbyScene)
			{
				SessionData.previousGameState = SessionData.currentGameState;
				SessionData.currentGameState = GameStates.RoomLobby;

				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

				UpdateRoomProperties(currentRoom.CustomProperties);
				LobbyPlayerListUpdated(null);
				UpdateArenaList(null, null);
				UpdateArenaTeamList(null, null);
				SessionData.ResetArenaTeamLists();
				SessionData.ResetTeamData();
			}

			if(scene.name == Constants.TeamAssignScene)
			{
				SessionData.previousGameState = SessionData.currentGameState;
				SessionData.currentGameState = GameStates.TeamAssign;
			}

			if (scene.name == Constants.MainMenuScene)
			{
				SessionData.previousGameState = SessionData.currentGameState;
				SessionData.currentGameState = GameStates.MainMenu;

				Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			}

			if (scene.name == Constants.GameScene)
			{
				SessionData.previousGameState = SessionData.currentGameState;
				SessionData.currentGameState = GameStates.InGame;
			}
		}

		private void UpdateRoomProperties(Hashtable changedRoomProperties)
		{
			availableArenas = changedRoomProperties[Constants.Room.AvailableArenas] != null ? (int)changedRoomProperties[Constants.Room.AvailableArenas] : availableArenas;
			occupiedArenas = changedRoomProperties[Constants.Room.OccupiedArenas] != null ? (int)changedRoomProperties[Constants.Room.OccupiedArenas] : occupiedArenas;
			currentRoomState = changedRoomProperties[Constants.Room.RoomState] != null ? (RoomState)(int)changedRoomProperties[Constants.Room.RoomState] : currentRoomState;

			if (SessionData.currentGameState == GameStates.RoomLobby && currentRoomState == RoomState.Assigned_Arenas)
			{
				OnArenasConfirmed();
			}


			bool goBackToLobby = (SessionData.currentGameState == GameStates.InGame || SessionData.currentGameState == GameStates.TeamAssign) && currentRoomState == RoomState.Unassigned_Arenas;

			if (goBackToLobby && LocalPlayer.Instance.isRoomOwner)
			{
				currentRoom.IsOpen = true;
				currentRoom.IsVisible = true;
				PhotonNetwork.LoadLevel(Constants.LobbyScene);
			}

		}

		private void OnPlayerPropertiesUpdated(Player changedPlayer, Hashtable changedProperties)
		{
			if(changedProperties.ContainsKey(Constants.Player.Ping))
				LobbyPlayerListUpdated(null);

			if (changedProperties.ContainsKey(Constants.Player.ArenaNo) || changedProperties.ContainsKey(Constants.Player.TeamNo))
				UpdateArenaBasedLists(changedPlayer, changedProperties);
		}


		private void ResetAllArenaPlayerLists()
		{
			for (int i = 0; i < arenaLists.Length; i++)
			{
				arenaLists[i].arenaPlayers.Clear();
			}
		}

		private void ResetLobbyPlayerList()
		{
			lobbyPlayers.Clear();
		}

		private void ResetGamePlayerList()
		{
			gamePlayers.Clear();
		}

		private void ResetAllArenaTeamPlayerLists()
		{
			for (int i = 0; i < arenaTeamLists.Length; i++)
			{
				arenaTeamLists[i].redTeamPlayers.Clear();
				arenaTeamLists[i].blueTeamPlayers.Clear();
			}
		}

		private void UpdateAllLists(Player player)
        {
			LobbyPlayerListUpdated(player);
			UpdateArenaBasedLists(null, null);

		}

		private void UpdateArenaBasedLists(Player changedPlayer, Hashtable changedProperties)
		{
			//foreach (var item in changedProperties)
			//{
			//    Debug.Log($"{item.Key}, {item.Value}");
			//}
			UpdateArenaList(changedPlayer, changedProperties);
			UpdateArenaTeamList(changedPlayer, changedProperties);
		}


		private void UpdateArenaList(Player changedPlayer, Hashtable changedProperties)
		{
			//Set the arena no for local player
			SetPlayerArenaNo(changedPlayer, changedProperties);

			//Sort all players into arena lists
			SortPlayersInArenaList();

			//Send the local players arena list to the UI
			List<Player> myArenaList = LocalPlayer.Instance.arenaNo >= 1 ? arenaLists[LocalPlayer.Instance.arenaNo - 1].arenaPlayers : new List<Player>();
			MultiplayerMesseges.OnUpdateMyArenaList?.Invoke(myArenaList);

			//Send all the arena lists to the UI
			MultiplayerMesseges.OnUpdateArenaLists?.Invoke(arenaLists);


		}

		private void UpdateArenaTeamList(Player changedPlayer, Hashtable changedProperties)
		{
			SetPlayerTeamNo(changedPlayer, changedProperties);

			SortPlayersInArenaTeamList();

			MultiplayerMesseges.OnUpdateArenaTeamLists?.Invoke();
		}

		public void ResetArenaTeamListOnNetwork()
		{
			for (int i = 0; i < gamePlayers.Count; i++)
			{
				playerProperties[Constants.Player.ArenaNo] = gamePlayers[i].CustomProperties[Constants.Player.ArenaNo];
				playerProperties[Constants.Player.TeamNo] = -1;
				gamePlayers[i].SetCustomProperties(playerProperties);
			}
		}

		private void SetPlayerTeamNo(Player changedPlayer, Hashtable changedProperties)
		{
			if (changedPlayer == LocalPlayer.Instance.player)
			{
				LocalPlayer.Instance.teamNo = (int)changedProperties[Constants.Player.TeamNo];
				if (LocalPlayer.Instance.teamNo < 0) return;

				LocalPlayer.Instance.teamName = (TeamName)LocalPlayer.Instance.teamNo;
				string message = $"You have joined Arena {LocalPlayer.Instance.arenaNo}, Team {LocalPlayer.Instance.teamName}";
				UtilEvents.ShowToastMessage?.Invoke(message);

			}
		}

		private void SortPlayersInArenaTeamList()
		{
			ResetAllArenaTeamPlayerLists();

			for (int i = 0; i < arenaLists.Length; i++)
			{
				for (int j = 0; j < arenaLists[i].arenaPlayers.Count; j++)
				{
					int playerTeamNo = arenaLists[i].arenaPlayers[j].CustomProperties[Constants.Player.TeamNo] == null ? -1 : (int)arenaLists[i].arenaPlayers[j].CustomProperties[Constants.Player.TeamNo];
					if (playerTeamNo >= 0)
					{
						if ((TeamName)playerTeamNo == TeamName.Red)
						{
							arenaTeamLists[i].redTeamPlayers.Add(arenaLists[i].arenaPlayers[j]);
						}

						if ((TeamName)playerTeamNo == TeamName.Purple)
						{
							arenaTeamLists[i].blueTeamPlayers.Add(arenaLists[i].arenaPlayers[j]);
						}
					}


				}

			}
		}

		private void SetPlayerArenaNo(Player changedPlayer, Hashtable changedProperties)
		{
			if (changedPlayer == LocalPlayer.Instance.player)
			{
				LocalPlayer.Instance.arenaNo = (int)changedProperties[Constants.Player.ArenaNo];
				
				if (!LocalPlayer.Instance.isMasterClient)
				{
					string message = "";

					if(LocalPlayer.Instance.arenaNo > 0)
					{
						message = $"You have joined Arena {LocalPlayer.Instance.arenaNo}";
					}
					else
					{
						message = LocalPlayer.Instance.arenaNo == -1 ? "Please wait, moderator will assign you an arena" : "You have joined the room as a spectator";
					}

					UtilEvents.ShowToastMessage?.Invoke(message);
				}
	
			}
		}

		private void SortPlayersInArenaList()
		{
			ResetAllArenaPlayerLists();

			for (int i = 0; i < gamePlayers.Count; i++)
			{
				int playerArenaNo = gamePlayers[i].CustomProperties[Constants.Player.ArenaNo] == null ? -1 : (int)gamePlayers[i].CustomProperties[Constants.Player.ArenaNo];
				if (playerArenaNo >= 1)
				{
					arenaLists[playerArenaNo - 1].arenaPlayers.Add(gamePlayers[i]);
				}
			}
		}

		private void LobbyPlayerListUpdated(Player player)
		{
			ResetLobbyPlayerList();
			ResetGamePlayerList();

			Player[] players = PhotonNetwork.PlayerList;
			for (int i = 0; i < players.Length; i++)
			{
				lobbyPlayers.Add(players[i]);
				int arenaNo = players[i].CustomProperties[Constants.Player.ArenaNo] == null ? -1 : (int)players[i].CustomProperties[Constants.Player.ArenaNo];
				if (arenaNo == 0) continue;
				gamePlayers.Add(players[i]);
			}

			MultiplayerMesseges.OnUpdateLobbyList?.Invoke(lobbyPlayers);
			MultiplayerMesseges.OnUpdateGamePlayerList?.Invoke(gamePlayers);

		}

		public List<Player> ScrambleList(List<Player> listToScramble)
		{
			List<Player> tempList = new List<Player>();

			//Copy data to the temp list
			for (int i = 0; i < listToScramble.Count; i++)
			{
				tempList.Add(listToScramble[i]);
			}

			var rnd = new Random();
			var randomized = tempList.OrderBy(item => rnd.Next()); //Randomize and return a linq

			return randomized.ToList();
		}

		public void AssignRandomArenas()
		{
			if (onGoingArenaAssignment)
			{
				UtilEvents.ShowToastMessage?.Invoke("Arena assignment is in process.. Wait for it to complete");
				return;
			}

			StartCoroutine(AssignRandomArenasToGamePlayers());
		}

		//Coroutine to ensure that no player assignment gets lost on the network
		private IEnumerator AssignRandomArenasToGamePlayers()
		{
			onGoingArenaAssignment = true;
			UtilEvents.ShowToastMessage?.Invoke("Arena assignment in progress..");

			int currentArenaNo = 1;

			List<Player> randomGamePlayers = ScrambleList(gamePlayers);
			for (int i = 0; i < randomGamePlayers.Count; i++)
			{
				//Set Arena for player
				playerProperties[Constants.Player.ArenaNo] = currentArenaNo;
				randomGamePlayers[i].SetCustomProperties(playerProperties);

				//Set Arena UI on player listing
				int playerNo = gamePlayers.IndexOf(randomGamePlayers[i]);
				MultiplayerMesseges.OnUpdatePlayerArena?.Invoke(playerNo, currentArenaNo);

				//Sequential increment of currentArenaNo
				currentArenaNo = currentArenaNo + 1 > availableArenas ? 1 : currentArenaNo + 1;

				yield return new WaitForSeconds(1f / randomGamePlayers.Count);

			}

			UtilEvents.ShowToastMessage?.Invoke("Arena assingment completed!");
			onGoingArenaAssignment = false;
		}

		public bool CanRandomizeArenas()
		{
			return gamePlayers.Count >= availableArenas * playersPerArenaRange.x;
		}

		public void SetPlayerPropertiesOnNetwork(int arenaNo)
		{
			playerProperties[Constants.Player.ArenaNo] = arenaNo;
			playerProperties[Constants.Player.Ping] = PhotonNetwork.GetPing();
			LocalPlayer.Instance.player.SetCustomProperties(playerProperties);
		}

		public void StartGameSession()
		{
			if (CanStartGameSession())
				SetCurrentRoomPropertiesAndStartGame();

		}

		public bool CanStartGameSession()
		{
			if (!LocalPlayer.Instance.isRoomOwner)
			{
				Debug.LogError("Player is neither a room owner, nor a moderator");
				return false;
			}
			playerNotAssignedMessage = "Assign arena to: ";
			if (!CheckIfAllPlayersHaveAnArena())
			{
				UtilEvents.ShowToastMessage?.Invoke(playerNotAssignedMessage);
				return false;
			}

			if (arenaLists[0].arenaPlayers.Count < playersPerArenaRange.x)
			{
				UtilEvents.ShowToastMessage?.Invoke($"Can't start game as Arena 1 doesn't meet minimum players");
				return false;
			}

			for (int i = 1; i < availableArenas; i++)
			{
				if (arenaLists[i].arenaPlayers.Count > 0 && arenaLists[i].arenaPlayers.Count < playersPerArenaRange.x)
				{
					UtilEvents.ShowToastMessage?.Invoke($"Can't start game as Arena {i + 1} has invalid amount of players");
					return false;
				}
			}

			return true;

		}

		public void SetCurrentRoomPropertiesAndStartGame()
		{
			occupiedArenas = 0;
			for (int i = 0; i < availableArenas; i++)
			{
				if (arenaLists[i].arenaPlayers.Count > 0) occupiedArenas++;
			}

			roomProperties[Constants.Room.AvailableArenas] = availableArenas;
			roomProperties[Constants.Room.OccupiedArenas] = occupiedArenas;
			roomProperties[Constants.Room.RoomState] = (int)RoomState.Assigned_Arenas;
			currentRoom.SetCustomProperties(roomProperties);
		}

		public void ResetRoom()
		{
			ResetArenaTeamListOnNetwork();
			Invoke(nameof(WaitAndResetRoomState), 1f);

		}

		private void WaitAndResetRoomState()
		{
			roomProperties[Constants.Room.AvailableArenas] = availableArenas;
			roomProperties[Constants.Room.OccupiedArenas] = occupiedArenas;
			roomProperties[Constants.Room.RoomState] = (int)RoomState.Unassigned_Arenas;
			currentRoom.SetCustomProperties(roomProperties);
		}

		private bool CheckIfAllPlayersHaveAnArena()
		{
			bool allAssignedArenas = true;
			//Check if all players have been assigned an arena
			for (int i = 0; i < gamePlayers.Count; i++)
			{
				int playerArenaNo = gamePlayers[i].CustomProperties[Constants.Player.ArenaNo] == null ? -1 : (int)gamePlayers[i].CustomProperties[Constants.Player.ArenaNo];

				if (playerArenaNo < 1)
				{
					allAssignedArenas = false;
					playerNotAssignedMessage += playerNotAssignedMessage == "Assign arena to: " ? $"{gamePlayers[i].NickName} "
						: $",{gamePlayers[i].NickName} ";
				}
			}

			return allAssignedArenas;
		}

		private void OnArenasConfirmed()
		{
			switch (SessionData.GameType)
			{
				case GameType.InterArena:
						StartGameForInterArena();
					break;

				case GameType.IntraArena:
					UtilEvents.ShowToastMessage?.Invoke("Generating Teams");
					CreateTeamsForAllArenas();
					break;

				default:
					Debug.LogError("Something went wrong, the Game Type is undefined");
					break;
			}

		}

		private void StartGameForInterArena()
		{
			if (LocalPlayer.Instance.isMasterClient)
			{
				currentRoom.IsOpen = false;
				currentRoom.IsVisible = false;
				PhotonNetwork.LoadLevel(Constants.TeamAssignScene);
			}


		}

		private void CreateTeamsForAllArenas()
		{
			if (LocalPlayer.Instance.isRoomOwner)
				StartCoroutine(CreateTeamsForIntraArena());
		}

		private IEnumerator CreateTeamsForIntraArena()
		{
			int[] currentTeam = new int[occupiedArenas];
			for (int i = 0; i < occupiedArenas; i++)
			{
				List<Player> randomizedArenaPlayers = ScrambleList(arenaLists[i].arenaPlayers);
				currentTeam[i] = (int)TeamName.Red;

				for (int j = 0; j < randomizedArenaPlayers.Count; j++)
				{
					//Sets the team no for the arena player
					playerProperties[Constants.Player.ArenaNo] = randomizedArenaPlayers[j].CustomProperties[Constants.Player.ArenaNo];
					playerProperties[Constants.Player.TeamNo] = currentTeam[i];
					randomizedArenaPlayers[j].SetCustomProperties(playerProperties);
					currentTeam[i] = 1 - currentTeam[i];
					yield return new WaitForSeconds(1f / randomizedArenaPlayers.Count);
				}
			}

		}

	} 
}
