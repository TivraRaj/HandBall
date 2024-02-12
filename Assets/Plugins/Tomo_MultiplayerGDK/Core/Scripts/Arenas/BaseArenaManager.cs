using UnityEngine;
using TomoClub.Core;

namespace TomoClub.Arenas
{
	public enum ArenaState { Paused, Running, Completed }

	public abstract class BaseArenaManager<T> : MonoBehaviour where T: BaseArena
	{
		[Header("Arenas")]
		[SerializeField] Vector3[] arenaPositions;
		[SerializeField] GameObject arenaPrefab;

		protected T[] arenas;
		protected int currentArenaNo = -1;
		protected int sessionGameTime;

		protected ArenaButtonFunctions arenaButtonFunctions;

		protected virtual void Awake()
		{
			arenaButtonFunctions = GetComponentInChildren<ArenaButtonFunctions>();

		}

		protected virtual void Start()
		{
			InstantiateArenas();

			if (SessionData.showSplashScreen)
			{
				PersistantUI.Instance.UpdateSplashScreen(true);
				if (LocalPlayer.Instance.isMasterClient)
					Invoke(nameof(OnGameSceneLoaded), SessionData.splashScreenTTL);

			}
			else
				StartArenas();
		}

		protected virtual void OnEnable()
		{
			UtilEvents.SetAndStartTimer += SwitchTimerControl;

			arenaButtonFunctions.OnPlayPauseAll += OnPlayPauseAllArenas;
			arenaButtonFunctions.OnPlayPauseArena += OnPlayPauseArena;
			arenaButtonFunctions.OnSetCurrentArenaLookAt += SetCurrentArenaToLookAt;
			arenaButtonFunctions.OnSpectate += SpectateGame;
		}

		protected virtual void OnDisable()
		{
			UtilEvents.SetAndStartTimer -= SwitchTimerControl;

			arenaButtonFunctions.OnPlayPauseAll -= OnPlayPauseAllArenas;
			arenaButtonFunctions.OnPlayPauseArena -= OnPlayPauseArena;
			arenaButtonFunctions.OnSetCurrentArenaLookAt -= SetCurrentArenaToLookAt;
			arenaButtonFunctions.OnSpectate -= SpectateGame;
		}

		protected virtual void InstantiateArenas()
		{
			Init();
			GenerateArenas();
		}

		protected abstract void OnGameSceneLoaded();
		
		protected virtual void Init()
		{
			arenas = new T[MultiplayerManager.Instance.occupiedArenas];


			ArenaUIManager.Instance.Init();

			//Game Settings
			sessionGameTime = MultiplayerManager.Instance.GameSessionTime;
		}

		protected virtual void GenerateArenas()
		{
			//New Arenas Object
			var arenaParentObject = new GameObject("Arenas");
			arenaParentObject.transform.position = Vector3.zero;
			arenaParentObject.transform.rotation = Quaternion.identity;

			if(arenaPrefab == null)
            {
				Debug.LogError("Cannot Spawn Arenas as no arena object reference found");
				return;
            }

			for (int i = 0; i < MultiplayerManager.Instance.occupiedArenas; i++)
			{
				if(arenaPositions[i] == null)
                {
					Debug.LogError($"Cannot Spawn Arena {i} as no arena position reference found");
					return;
				}

				var arenaObject = Instantiate(arenaPrefab, arenaPositions[i], Quaternion.identity, arenaParentObject.transform);
				arenaObject.gameObject.name = "Arena " + (i + 1);
				arenas[i] = arenaObject.GetComponent<T>();
				arenas[i].InitializeArena(i + 1, sessionGameTime);
			}
		}

		public virtual void StartArenas()
		{
			UpdatePlayerTypeState(LocalPlayer.Instance.defaultPlayerType);

			for (int i = 0; i < MultiplayerManager.Instance.occupiedArenas; i++)
			{
				arenas[i].StartArena();
			}

			UtilEvents.ShowToastMessage("Game Started!");
		}


		#region Arena To Look At
		//Set Current Arena To look at
		protected virtual void SetCurrentArenaToLookAt(int arenaNo)
		{
			if (arenaNo < 1 || arenaNo > arenas.Length)
			{
				Debug.LogError($"Can only set arenaNo between 0 and {arenas.Length + 1}");
				return;
			}

			if (currentArenaNo == arenaNo) return;


			if (currentArenaNo > 0)
			{
				ArenaUIManager.Instance.SetSpectorButtonColor(currentArenaNo, Color.red);
				arenas[currentArenaNo - 1].DeactivateArena();
			}

			ArenaUIManager.Instance.SetSpectorButtonColor(arenaNo, Color.green);
			arenas[arenaNo - 1].ActivateArena();
			currentArenaNo = arenaNo;

		}

		private int ActiveArenaToLookAt()
		{
			for (int i = 0; i < arenas.Length; i++)
			{
				int arenaNo = i + 1;
				if (arenas[i].ArenaState != ArenaState.Completed) return arenaNo;
			}

			return 1;
		}

		#endregion

		#region Arena Timers

		public virtual void UpdateArenaTimerOnClient(int arenaNo, int currentTime)
		{
			arenas[arenaNo - 1].UpdateTimerData(currentTime);
		}

		//Only master client gets this option
		protected virtual void OnPlayPauseArena(int arenaNo)
		{
			ArenaState currentArenaState = arenas[arenaNo - 1].ArenaState;
			if (currentArenaState == ArenaState.Completed) return;

			//Toggle State of Arena
			int updatedState = 1 - (int)currentArenaState;
			//Update the arena timer on the master client
			UpdateTimerState(arenaNo, (ArenaState)updatedState);

			//Send Updated ArenaState to all players
			UpdateArenaPauseStateOnNetwork(arenaNo, (ArenaState)updatedState);
		}

		protected abstract void UpdateArenaPauseStateOnNetwork(int arenaNo, ArenaState updatedState);

		protected virtual void OnPlayPauseAllArenas()
		{
			int updatedState = AllArenasPaused() ? 1 : 0;
			int currentArenaNo;
			for (int i = 0; i < arenas.Length; i++)
			{
				currentArenaNo = i + 1;
				if (arenas[i].ArenaState == ArenaState.Completed) continue;
				UpdateTimerState(currentArenaNo, (ArenaState)updatedState);

				//Send Updated ArenaState to all players
				UpdateArenaPauseStateOnNetwork(currentArenaNo, (ArenaState)updatedState);

			}

		}

		public virtual void UpdatePlayerPauseOnClient(int arenaNo, int updatedState)
		{
			ArenaState stateToChangeTo = (ArenaState)updatedState;
			arenas[arenaNo - 1].UpdateArenaStateData(stateToChangeTo);

			bool areAllArenasPaused = AllArenasPaused();
			ArenaUIManager.Instance.UpdatePauseButtonSprite(areAllArenasPaused, arenaNo, updatedState);

			//If its your arenaNo then pause/play the game
			if (arenaNo == LocalPlayer.Instance.arenaNo) UpdatePlayerArenaState((ArenaState)updatedState);
		}

		private bool AllArenasPaused()
		{
			for (int i = 0; i < arenas.Length; i++)
			{
				if (arenas[i].ArenaState == ArenaState.Running) return false;
			}
			return true;
		}

		//Switches timer control to new master client
		protected virtual void SwitchTimerControl()
		{
			foreach (var arena in arenas) arena.ContinueTimer();
		}

		#endregion


		#region On Arena Complete
		public virtual void UpdateArenaToCompletedState(int arenaNo)
		{
			arenas[arenaNo - 1].UpdateArenaStateData(ArenaState.Completed);
			ArenaUIManager.Instance.UpdatePauseButtonState(arenaNo, false);
			arenas[arenaNo - 1].StopTimer();

			if (arenaNo == LocalPlayer.Instance.arenaNo) UpdatePlayerArenaState(ArenaState.Completed);

			if (AllArenasCompleted())
			{
				ArenaUIManager.Instance.UpdateWaitForPanelCompletionState(false);
				UpdateLeaderboardBasedOnPlayerTypeState(LocalPlayer.Instance.inGamePlayerType);
			}
			else
			{
				if (arenaNo == LocalPlayer.Instance.arenaNo)
					ArenaUIManager.Instance.UpdateWaitForPanelCompletionState(true);
			}

		}

		private bool AllArenasCompleted()
		{
			for (int i = 0; i < arenas.Length; i++)
			{
				if (arenas[i].ArenaState != ArenaState.Completed) return false;
			}

			return true;
		}

		protected virtual void SpectateGame()
		{
			if (LocalPlayer.Instance.inGamePlayerType == PlayerType.Master_Player) UpdatePlayerTypeState(PlayerType.Master_Spectator);
			else if (LocalPlayer.Instance.inGamePlayerType == PlayerType.Player) UpdatePlayerTypeState(PlayerType.Spectator);
		}

		#endregion

		#region Change Arena State 

		protected virtual void UpdateTimerState(int arenaNo, ArenaState arenaState)
		{
			switch (arenaState)
			{
				case ArenaState.Paused:
					arenas[arenaNo - 1].PauseTimer();
					break;
				case ArenaState.Running:
					arenas[arenaNo - 1].PlayTimer();
					break;
				case ArenaState.Completed:
					break;
				default:
					Debug.LogError("Something went really wrong, arenaState can never be null");
					break;
			}
		}

		protected virtual void UpdatePlayerArenaState(ArenaState arenaState)
		{
			switch (arenaState)
			{
				case ArenaState.Paused:
					ArenaUIManager.Instance.UpdatePausePanelState(true);
					break;
				case ArenaState.Running:
					ArenaUIManager.Instance.UpdatePausePanelState(false);
					break;
				case ArenaState.Completed:
					ArenaUIManager.Instance.UpdateSpectatorPanelState(true);
					break;
				default:
					Debug.LogError("Something went really wrong, arenaState can never be null");
					break;
			}
		}

		protected virtual void UpdatePlayerTypeState(PlayerType changedState)
		{
			LocalPlayer.Instance.inGamePlayerType = changedState;
			switch (changedState)
			{
				case PlayerType.Master_Spectator:
					ArenaUIManager.Instance.UpdateSpectatorPanelState(true);
					ArenaUIManager.Instance.UpdateWaitForPanelCompletionState(false);
					ArenaUIManager.Instance.UpdateSpectatorUIState(MultiplayerManager.Instance.occupiedArenas > 1);
					ArenaUIManager.Instance.UpdatePausePlayHolderState(true);
					SetCurrentArenaToLookAt(ActiveArenaToLookAt());
					break;
				case PlayerType.Master_Player:
					ArenaUIManager.Instance.UpdatePausePlayHolderState(true);
					SetCurrentArenaToLookAt(LocalPlayer.Instance.arenaNo);
					break;
				case PlayerType.Spectator:
					ArenaUIManager.Instance.UpdateSpectatorPanelState(true);
					ArenaUIManager.Instance.UpdateSpectatorUIState(MultiplayerManager.Instance.occupiedArenas > 1);
					SetCurrentArenaToLookAt(ActiveArenaToLookAt());
					ArenaUIManager.Instance.UpdateWaitForPanelCompletionState(false);
					break;
				case PlayerType.Player:
					SetCurrentArenaToLookAt(LocalPlayer.Instance.arenaNo);
					break;
				default:
					Debug.LogError("Not possible, something went terribly wrong");
					break;
			}
		}

		protected virtual void UpdateLeaderboardBasedOnPlayerTypeState(PlayerType playerType)
		{
			ArenaUIManager.Instance.Panel_EndPanel.SetActive(true);
			switch (playerType)
			{
				case PlayerType.Master_Spectator:
					ArenaUIManager.Instance.Panel_EndMaster.SetActive(true);
					break;
				case PlayerType.Master_Player:
					ArenaUIManager.Instance.Panel_EndMaster.SetActive(true);
					break;
				case PlayerType.Spectator:
					ArenaUIManager.Instance.Panel_EndNormal.SetActive(true);
					break;
				case PlayerType.Player:
					ArenaUIManager.Instance.Panel_EndNormal.SetActive(true);
					break;
				default:
					break;
			}

			
		}

		#endregion


	} 
}
