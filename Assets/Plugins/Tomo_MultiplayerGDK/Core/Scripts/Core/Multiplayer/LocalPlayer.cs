using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using TomoClub.Util;


namespace TomoClub.Core
{
	//Local Player
	public class LocalPlayer : MonoBehaviour
	{

		public static LocalPlayer Instance;

		[Header("Player Settings")]
		[Tooltip("Initial player types on start of the server")]
		[SerializeField] private PlayerSettings playerSettings;

		[Header("Don't change it in editor - for debug purposes only")]
		public int arenaNo = -1;
		public int teamNo = -1;
		public TeamName teamName = TeamName.None;

		public PlayerType defaultPlayerType;
		public PlayerType inGamePlayerType;

		/// <summary>
		/// Is the Local Player a spectator in the main-menu, lobby, pre-game phase
		/// </summary>
		public bool isdefaultSpec => defaultPlayerType == PlayerType.Master_Spectator || defaultPlayerType == PlayerType.Spectator;

		/// <summary>
		/// Is the Local Player a spectator during the game phase
		/// </summary>
		public bool isInGameSpec => inGamePlayerType == PlayerType.Master_Spectator || inGamePlayerType == PlayerType.Spectator;

		/// <summary>
		/// Is the Local Player the room owner
		/// </summary>
		public bool isRoomOwner => defaultPlayerType == PlayerType.Master_Spectator || defaultPlayerType == PlayerType.Master_Player;


		#region Local Player Cached Data
		private string m_playerName;
		public string playerName
		{
			get => SessionData.TestMode ? m_playerName : PlayerPrefs.GetString(Constants.Player.PlayerName, "");
			set
			{
				if (SessionData.TestMode)
					m_playerName = value;
				else
					PlayerPrefs.SetString(Constants.Player.PlayerName, value);
			}
		}

		private string m_userID;
		public string userID
		{
			get
			{
				return SessionData.TestMode ? m_userID : PlayerPrefs.GetString(Constants.Player.PlayerUserID, "");
			}
			set
			{
				if (SessionData.TestMode)
					m_userID = value;
				else
					PlayerPrefs.SetString(Constants.Player.PlayerUserID, value);

			}
		}

		private bool m_timedOut = false;
		public bool timedOut
		{
			get => SessionData.TestMode ? m_timedOut : PlayerPrefs.GetInt(Constants.Player.PlayerTimeout, 0) == 1;
			set
			{
				if (SessionData.TestMode)
				{
					m_timedOut = value;
				}
				else
				{
					int timedOutVal = value ? 1 : 0;
					PlayerPrefs.SetInt(Constants.Player.PlayerTimeout, timedOutVal);
				}

			}
		}

		#endregion



		#region Local Player Photon Data
		public Player player => PhotonNetwork.LocalPlayer;

		public string nickName => player.NickName;
		public int actorNumber => player.ActorNumber - 1;
		public bool isInactive => player.IsInactive;
		public bool isMasterClient => player.IsMasterClient;
		public bool hasRejoined => player.HasRejoined;
		public string playerId => player.UserId;
		public int serverTeamNo
		{
			get
			{
				if (player.CustomProperties[Constants.Player.TeamNo] == null) return -1;
				else return (int)player.CustomProperties[Constants.Player.TeamNo];
			}

		}
		public int serverArenaNo
		{
			get
			{
				if (player.CustomProperties[Constants.Player.ArenaNo] == null) return -1;
				else return (int)player.CustomProperties[Constants.Player.ArenaNo];

			}
		}

		#endregion

		private CounterDown playerTimeOutTimer;

		private void Awake()
		{
			if (Instance == null) Instance = this;
			else return;

			playerTimeOutTimer = new CounterDown(playerSettings.playerTimeout);

		}

		private void OnEnable()
		{
			playerTimeOutTimer.CounterCompleted += KickOutTimerExpired;
			playerTimeOutTimer.CounterUpdatePerSecond += UpdateLeftOutTime;

		}

		private void OnDisable()
		{
			playerTimeOutTimer.CounterCompleted -= KickOutTimerExpired;
			playerTimeOutTimer.CounterUpdatePerSecond -= UpdateLeftOutTime;

		}

		private void Update()
		{
			playerTimeOutTimer.UpdateCounter();
		}


		/// <summary>
		/// Starts the timer if the player got kicked out of a room
		/// </summary>
		public void StartKickOutTimer()
		{
			if (timedOut) playerTimeOutTimer.StartCounter();
		}

		//On expiry of the kick timer
		private void KickOutTimerExpired()
		{
			timedOut = false;
			playerTimeOutTimer.ResetCounter();
			UtilEvents.OnKickOutOver?.Invoke();
			UtilEvents.ShowToastMessage?.Invoke($"Player Timeout ended, can rejoin rooms now!");
		}

		//Update the UI on how much time is left before user is no longer banned
		private void UpdateLeftOutTime(int timeLeft)
		{
			UtilEvents.ShowToastMessage?.Invoke($"Player in timeout for {timeLeft} seconds");
		}

		/// <summary>
		/// Reset Local Player data {arenaNo, teamNo, teamName}
		/// </summary>
		public void ResetLocalPlayerData()
		{
			arenaNo = -1;
			teamNo = -1;
			teamName = TeamName.None;
		}

	} 
}
