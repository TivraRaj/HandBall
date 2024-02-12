using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;



namespace TomoClub.Core
{ 

public static class ServerMesseges
{
    public static Action EstablishConnectionToServer;
    public static Action OnConnectedToPhoton;
    public static Action OnDissconnectedFromPhoton;
    public static Action OnCreateRoomSuccessful;
    public static Action OnJoinRoomSuccessful;
    public static Action<string> OnCreateRoomFailed;
    public static Action<string> OnJoinRoomFailed;
    public static Action OnLeaveRoom;
    public static Action<List<RoomInfo>> OnRoomListUpdated;
    public static Action<Player> OnPlayerJoinedRoom;
    public static Action<Player> OnPlayerLeftRoom;
    public static Action<Player, Hashtable> OnPlayerPropertiesUpdated;
    public static Action<Hashtable> OnRoomPropertiesUpdated;
    public static Action OnMasterClientSwitched;
}

  
    [RequireComponent(typeof(MultiplayerManager))] 
    //Handles connection to the server and all callbacks from the server
    public class ServerManager : MonoBehaviourPunCallbacks
    {
        private static bool hasBeenInitialized = false;
        private string customLobbyName = "TomoClub_";
        private GameSettings gameSettings;
        

        private void Awake()
        {
            if (hasBeenInitialized) return;

            hasBeenInitialized = true;
            gameSettings = GetComponent<MultiplayerManager>().gameSettings;

            //Clear cache if in test mode
            if (gameSettings.inTestingMode && gameSettings.clearPlayerPrefs) PlayerPrefs.DeleteAll();

            //Application Setup
#if UNITY_WEBGL
            Application.targetFrameRate = gameSettings.targetFrameRate_webGL;
#elif UNITY_STANDALONE
            Application.targetFrameRate = gameSettings.targetFrameRate_standalone;
#endif
            Application.runInBackground = gameSettings.canRunInBackground; 

            //Photon Setup
            PhotonNetwork.AutomaticallySyncScene = gameSettings.automaticSyncing; 
            PhotonNetwork.KeepAliveInBackground = gameSettings.timeAliveInBackground;

            //Set Session Data (Persistant)
            SessionData.GameName = gameSettings.gameName;
            SessionData.GameType = gameSettings.gameType;
            //Set Session Data (Non-Persistant)
            SessionData.TestMode = gameSettings.inTestingMode;
            SessionData.AuthenticationType = gameSettings.authenticationType;

            SessionData.webGLLink = gameSettings.webGLHostLink;
            
            SessionData.showSplashScreen = gameSettings.showSplashScreen;
            SessionData.splashScreenTTL = gameSettings.splashScreenTTL;

            if (SessionData.previousGameState == GameStates.Null)
            {
                SessionData.BuildType = gameSettings.buildType; //For initial build type as some build types change into another
                SessionData.IsRegionLocked = gameSettings.isRegionLocked;
                SessionData.BuildRegion = gameSettings.buildRegion;

            }                


            customLobbyName += SessionData.GameType;          

            ServerMesseges.EstablishConnectionToServer += ConnectToPhotonServer;
        }

    

        private void OnDestroy()
        {
            ServerMesseges.EstablishConnectionToServer -= ConnectToPhotonServer;
        }

        //Trying to establish connection with the server!
        private void ConnectToPhotonServer()
        {
            if (SessionData.connectionEstablished) return; 

            //Connect to PhotonServer
            PhotonNetwork.ConnectUsingSettings();
            UtilEvents.ShowToastMessage?.Invoke("Connecting To Game!...");

        }

        //On Failed to Established Connection or disconnected from the server
        public override void OnDisconnected(DisconnectCause cause)
        {
            UtilEvents.ShowToastMessage?.Invoke($"Disconnected from server; Cause: {cause}");
            SessionData.connectionEstablished = false;
            //Send data to UI based on whether in-game disconnection or just normal disconnection
            ServerMesseges.OnDissconnectedFromPhoton?.Invoke();
            
            if(SessionData.ForceReconnect)
			{
                SessionData.ForceReconnect = false;
                ConnectToPhotonServer();
			}
            else
            {
                PersistantUI.Instance.ShowDisconnectionPopup();
            }
        }

        //On Server Connection Established
        public override void OnConnectedToMaster()
        {
            UtilEvents.ShowToastMessage?.Invoke("Connected To Game!");
            if(!PhotonNetwork.InLobby) Invoke(nameof(JoinCustomLobby), 0.3f);
        }

        //Join a custom defined lobby instead of default lobby
        private void JoinCustomLobby()
        {
            TypedLobby customTypedLobby = new TypedLobby(customLobbyName, LobbyType.Default);
            SessionData.gameLobby = customTypedLobby;
            PhotonNetwork.JoinLobby(customTypedLobby);
            ;
        }

        //On Joining Custom Lobby
        public override void OnJoinedLobby()
        {
            UtilEvents.ShowToastMessage?.Invoke($"Joined Lobby: { PhotonNetwork.CurrentLobby.Name }");
            SessionData.connectionEstablished = true;
            ServerMesseges.OnConnectedToPhoton?.Invoke();
            
        }

        //On Created Room Successful
        public override void OnCreatedRoom()
        {
            UtilEvents.ShowToastMessage?.Invoke("Created Room: " + PhotonNetwork.CurrentRoom);
            //PhotonNetwork.LoadLevel(Identifiers.LobbyScene);
        }

        //On Create Room Failed
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            UtilEvents.ShowToastMessage?.Invoke(message);
            ServerMesseges.OnCreateRoomFailed?.Invoke(message);
        }

        //On Joined Room Successful 
        public override void OnJoinedRoom()
        {
            UtilEvents.ShowToastMessage?.Invoke("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
            ServerMesseges.OnJoinRoomSuccessful?.Invoke();
        }

        public override void OnLeftRoom()
        {
            UtilEvents.ShowToastMessage?.Invoke("Left Current Room");
            ServerMesseges.OnLeaveRoom?.Invoke();
        }

        //On Join Room Failed
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            UtilEvents.ShowToastMessage?.Invoke(message);
            ServerMesseges.OnJoinRoomFailed?.Invoke(message);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ServerMesseges.OnRoomListUpdated?.Invoke(roomList);
            //UtilEvents.ShowToastMessage?.Invoke("Available Room List Refreshed");
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            ServerMesseges.OnPlayerJoinedRoom?.Invoke(newPlayer);//Player Entered
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            ServerMesseges.OnPlayerLeftRoom?.Invoke(otherPlayer);//Player Left
            UtilEvents.ShowToastMessage?.Invoke($"{otherPlayer.NickName} has left the room!");
        }


        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            ServerMesseges.OnPlayerPropertiesUpdated?.Invoke(targetPlayer, changedProps);
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            ServerMesseges.OnRoomPropertiesUpdated?.Invoke(propertiesThatChanged);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if(newMasterClient == PhotonNetwork.LocalPlayer)
            {
                UtilEvents.SetAndStartTimer?.Invoke();

            }

        }

        public void UpdatePhotonServer()
		{
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendAcksOnly();
        }

    }
}

