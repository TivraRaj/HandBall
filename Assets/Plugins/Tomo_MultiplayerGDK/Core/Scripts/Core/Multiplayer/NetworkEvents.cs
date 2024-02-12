using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using TomoClub.Util;

namespace TomoClub.Core
{
	public class NetworkEvents : MonoBehaviour
	{
		public static NetworkEvents Instance;

		private void Awake()
		{
			if (Instance == null) Instance = this;
		}

		private void OnEnable()
		{
			PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
			ServerMesseges.OnLeaveRoom += GoBackToMainMenu;
			ServerMesseges.OnPlayerJoinedRoom += SyncGameSettingsForPlayer;
		}

		private void OnDisable()
		{
			PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
			ServerMesseges.OnLeaveRoom -= GoBackToMainMenu;


			ServerMesseges.OnPlayerJoinedRoom -= SyncGameSettingsForPlayer;
		}

		//Recieved Network Event 
		private void NetworkingClient_EventReceived(EventData obj)
		{
			//Moderator closed room event
			if (obj.Code == Constants.NetworkEvents.CloseRoomForEveryone) PlayerLeaveRoom(false); //Closed Room -> kickPlayer = false

			//Kick out player event
			if (obj.Code == Constants.NetworkEvents.CloseRoomForPlayer)
			{
				LocalPlayer.Instance.timedOut = true;
				PlayerLeaveRoom(true); //Kick Player -> kickPlayer = true
			}

			//Recive Player
			if (obj.Code == Constants.NetworkEvents.PlayerIsReady)
			{
				ReadyUpOnNetwork((object[])obj.CustomData);
			}

			//Sync Game Settings On Client
			if (obj.Code == Constants.NetworkEvents.SyncGameSettings)
			{
				SetGameSettingsDataOnClient((object[])obj.CustomData);
			}
		}

		public void CloseRoomForPlayer(Player playerToKickOut)
		{
			MultiplayerManager.Instance.playerProperties[Constants.Player.ArenaNo] = -1;
			playerToKickOut.SetCustomProperties(MultiplayerManager.Instance.playerProperties);

			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.TargetActors = new int[1] { playerToKickOut.ActorNumber };
			object[] obj = new object[] { };

			PhotonNetwork.RaiseEvent(Constants.NetworkEvents.CloseRoomForPlayer, obj, raiseEventOptions, SendOptions.SendReliable);
		}

		//Player leaves room when moderator closes the room
		private void PlayerLeaveRoom(bool kicked)
		{

			if (kicked)
			{
				UtilEvents.ShowToastMessage?.Invoke("Kicked out of room");
			}
			else
			{
				UtilEvents.ShowToastMessage?.Invoke("Moderator closed room.");
			}

			PhotonNetwork.LeaveRoom(false);

		}

		public void Moderator_CloseRoom()
		{
			StartCoroutine(CloseCurrentRoom());
		}

		IEnumerator CloseCurrentRoom()
		{
			//Reset arenaNos on the network
			UtilEvents.ShowToastMessage?.Invoke("Closing Room: " + PhotonNetwork.CurrentRoom.Name);
			for (int i = 0; i < MultiplayerManager.Instance.lobbyPlayers.Count; i++)
			{
				if((int)MultiplayerManager.Instance.lobbyPlayers[i].CustomProperties[Constants.Player.ArenaNo] > 1)
				{
					MultiplayerManager.Instance.playerProperties[Constants.Player.ArenaNo] = -1;
					MultiplayerManager.Instance.lobbyPlayers[i].SetCustomProperties(MultiplayerManager.Instance.playerProperties);
					yield return null;
				}

				yield return new WaitForSeconds(0.5f / MultiplayerManager.Instance.lobbyPlayers.Count);
			}

			if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
			{
				PhotonNetwork.RaiseEvent(Constants.NetworkEvents.CloseRoomForEveryone, new object[] { }, RaiseEventOptions.Default, SendOptions.SendReliable);
			}

			UtilEvents.ShowToastMessage?.Invoke("Exiting to Main Menu...");
			yield return new WaitForSeconds(0.5f);
			PhotonNetwork.LeaveRoom(false);

		}

		//On Leave Room go back to main menu
		private void GoBackToMainMenu()
		{
			if(SessionData.currentGameState != GameStates.MainMenu)
				SceneManager.LoadScene(Constants.MainMenuScene);
		}

		public void PlayerReadyUp()
		{
			object[] data = new object[] { LocalPlayer.Instance.player };
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.Receivers = ReceiverGroup.All;
			PhotonNetwork.RaiseEvent(Constants.NetworkEvents.PlayerIsReady, data, raiseEventOptions, SendOptions.SendReliable);
		}

		private void ReadyUpOnNetwork(object[] data)
		{
			Player sentPlayer = (Player)data[0];
			int arenaNo = (int)sentPlayer.CustomProperties[Constants.Player.ArenaNo];
			int teamNo = (int)sentPlayer.CustomProperties[Constants.Player.TeamNo];

			int playerIndexInTeamList = (TeamName)teamNo == TeamName.Red ?
				MultiplayerManager.Instance.arenaTeamLists[arenaNo - 1].redTeamPlayers.IndexOf(sentPlayer) : MultiplayerManager.Instance.arenaTeamLists[arenaNo - 1].blueTeamPlayers.IndexOf(sentPlayer);

			UserEvents.UpdatePlayerReadyUp?.Invoke(arenaNo, teamNo, playerIndexInTeamList);

		}

		public void SyncGameSettingsOnNetwork(int gameTime)
		{
			object[] data = new object[] { gameTime };
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.Receivers = ReceiverGroup.All;
			PhotonNetwork.RaiseEvent(Constants.NetworkEvents.SyncGameSettings, data, raiseEventOptions, SendOptions.SendReliable);
		}

		public void SyncGameSettingsForPlayer(Player player)
		{
			if (!LocalPlayer.Instance.isMasterClient) return;

			object[] data = new object[] { MultiplayerManager.Instance.GameSessionTime };
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
			raiseEventOptions.TargetActors = new int[] { player.ActorNumber };
			PhotonNetwork.RaiseEvent(Constants.NetworkEvents.SyncGameSettings, data, raiseEventOptions, SendOptions.SendReliable);
		}

		private void SetGameSettingsDataOnClient(object[] gameSettings)
		{
			MultiplayerManager.Instance.GameSessionTime = (int)gameSettings[0];
		}
	} 
}
