using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace TomoClub.Core
{
    //Handles Room Creation and Joining
    public class RoomManager : MonoBehaviour
    {
        private int defaultPlayers;
        private int defaultSpectators;
        private int defaultArenas; 

        [Header("Create Room - UI Updations")]
        [SerializeField] TextMeshProUGUI playersText;
        [SerializeField] TextMeshProUGUI arenaPlayersText;
        [SerializeField] TextMeshProUGUI spectatorsText;
        [SerializeField] TextMeshProUGUI createErrorText;

        [Header("Join Room - UI Updations")]
        [SerializeField] GameObject noRoomsAvailable;
        [SerializeField] GameObject[] joinRoomObjects;
        [SerializeField] TextMeshProUGUI[] joinErrorText;
        [SerializeField] TMP_InputField joinRoomId;

        [Header("Audio")]
        [SerializeField] AudioClip errorAudioClip;

        private TextMeshProUGUI[] joinRoomObjectTexts;
        private string[] currentRoomNames;

        private bool canCreateRoom = true;
        private bool canJoinRoom = true;

        private Vector2Int totalPlayersRange;
        private Vector2Int totalArenaRange;
        private Vector2Int perArenaPlayersRange;

        private int modVal => LocalPlayer.Instance.defaultPlayerType == PlayerType.Master_Spectator ? 1 : 0;


        private readonly string roomNoPrefix = "Room No: ";

        
        private void Start()
        {
            UpdateCreateRoomData();
            JoinRoom_Init();
        }

        private void OnEnable()
        {
            ServerMesseges.OnConnectedToPhoton += UpdateCreateRoomDefaults;
            ServerMesseges.OnJoinRoomSuccessful += GoToLobbyMenu;
            ServerMesseges.OnCreateRoomFailed += UpdateCreateRoomUI;
            ServerMesseges.OnJoinRoomFailed += UpdateJoinRoomUI;
            ServerMesseges.OnRoomListUpdated += UpdateRoomListUI;
        }

        private void OnDisable()
        {
            ServerMesseges.OnConnectedToPhoton -= UpdateCreateRoomDefaults;
            ServerMesseges.OnJoinRoomSuccessful -= GoToLobbyMenu;
            ServerMesseges.OnCreateRoomFailed -= UpdateCreateRoomUI;
            ServerMesseges.OnJoinRoomFailed -= UpdateJoinRoomUI;
            ServerMesseges.OnRoomListUpdated -= UpdateRoomListUI;

        }

        private void UpdateCreateRoomDefaults()
        {
            defaultSpectators = Mathf.Clamp(defaultSpectators + modVal, modVal, totalPlayersRange.y - perArenaPlayersRange.x);
            defaultPlayers = Mathf.Clamp(defaultPlayers - modVal, totalPlayersRange.x, totalPlayersRange.y - modVal);
            defaultArenas = Mathf.Clamp(Mathf.CeilToInt(defaultPlayers / perArenaPlayersRange.x), totalArenaRange.x, totalArenaRange.y);

            playersText.text = defaultPlayers.ToString();
            arenaPlayersText.text = defaultArenas.ToString();
            spectatorsText.text = defaultSpectators.ToString();

        }

        //Initial create room setup
        private void UpdateCreateRoomData()
        {
            totalArenaRange = MultiplayerManager.Instance.arenasPerRoomRange;
            totalPlayersRange = MultiplayerManager.Instance.playersPerRoomRange;
            perArenaPlayersRange = MultiplayerManager.Instance.playersPerArenaRange;

            createErrorText.text = "";

        }

        private void JoinRoom_Init()
        {
            joinRoomObjectTexts = new TextMeshProUGUI[joinRoomObjects.Length];
            currentRoomNames = new string[joinRoomObjects.Length];
        }

        //Initial join room setup
        private void UpdateJoinRoomData()
        {
            noRoomsAvailable.SetActive(true);

            for (int i = 0; i < joinRoomObjects.Length; i++)
            {
                joinRoomObjects[i].SetActive(false);
                joinRoomObjectTexts[i] = joinRoomObjects[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            }

            foreach (var errorText in joinErrorText)
            {
                errorText.text = "";
            }
            
        }

        //Update message to show on failing to create room
        private void UpdateCreateRoomUI(string message)
        {
            createErrorText.text = message;
            canCreateRoom = true;
        }

        //Update message to show on failing to join room
        private void UpdateJoinRoomUI(string message)
        {
            foreach (var errorText in joinErrorText)
            {
                errorText.text = message;
            }

            canJoinRoom = true;
        }

        #region CREATE ROOM PANEL BUTTON FUNCTIONS

        public void DecrementTotalPlayers()
        {
            createErrorText.text = "";

            if (defaultPlayers - 1 < totalPlayersRange.x)
            {
                createErrorText.text = $"Can't have less than {totalPlayersRange.x} players in a room";
                SoundMessages.PlaySFX?.Invoke(errorAudioClip);
                return;
            }

            if(defaultPlayers - 1 < defaultArenas * perArenaPlayersRange.x)
            {
                defaultArenas--;
                arenaPlayersText.text = defaultArenas.ToString();

            }

            defaultPlayers--;
            playersText.text = defaultPlayers.ToString();
        }

        public void IncrementTotalPlayers()
        {
            createErrorText.text = "";

            if (defaultPlayers + 1 > totalPlayersRange.y - modVal)
            {
                createErrorText.text = $"Can't have more than {totalPlayersRange.y} players in a room";
                SoundMessages.PlaySFX?.Invoke(errorAudioClip);
                return; 
            }

            if (defaultPlayers + 1 + defaultSpectators > totalPlayersRange.y)
            {
                DecrementSpectators();
            }

            //Total Players Exceeding arena player size case
            if (defaultPlayers + 1 > defaultArenas * perArenaPlayersRange.y)
            {
                defaultArenas++;
                arenaPlayersText.text = defaultArenas.ToString();
            }


            defaultPlayers++;
            playersText.text = defaultPlayers.ToString();
        }

        public void DecrementArenaPlayers()
        {
            createErrorText.text = "";

            if (defaultArenas - 1 < totalArenaRange.x)
            {
                createErrorText.text = $"Can't have less than {totalArenaRange.x} arena";
                SoundMessages.PlaySFX?.Invoke(errorAudioClip);
                return;
            }

            if(defaultArenas - 1 < Mathf.CeilToInt((float)defaultPlayers / perArenaPlayersRange.y))
            {
                defaultPlayers = (defaultArenas - 1) * perArenaPlayersRange.y;
                playersText.text = defaultPlayers.ToString();
            }

            defaultArenas--;
            arenaPlayersText.text = defaultArenas.ToString();
        }

        public void IncrementArenaPlayers()
        {
            createErrorText.text = "";

            if (defaultArenas + 1 > totalArenaRange.y)
            {
                createErrorText.text = $"Can't have more than {totalArenaRange.y} arenas";
                SoundMessages.PlaySFX?.Invoke(errorAudioClip);
                return;
            }

            if ( (defaultArenas + 1) * perArenaPlayersRange.x > defaultPlayers)
            {
                defaultPlayers = (defaultArenas + 1) * perArenaPlayersRange.x;
                playersText.text = defaultPlayers.ToString();

                if (defaultPlayers + defaultSpectators > totalPlayersRange.y)
				{
                    defaultSpectators = totalPlayersRange.y - defaultPlayers;
                    spectatorsText.text = defaultSpectators.ToString();
				}
            }

            defaultArenas++;
            arenaPlayersText.text = defaultArenas.ToString();

        }

        public void IncrementSpectators()
		{

            if (totalPlayersRange.y - (defaultSpectators + 1) < totalPlayersRange.x)
			{
                createErrorText.text = $"You need a minimum of {totalPlayersRange.x} players!";
                SoundMessages.PlaySFX?.Invoke(errorAudioClip);
                return;
            }

            if (defaultSpectators + 1 + defaultPlayers > totalPlayersRange.y)
			{
                DecrementTotalPlayers();
			}

            defaultSpectators++;
            spectatorsText.text = defaultSpectators.ToString();
		}

        public void DecrementSpectators()
		{
            if (defaultSpectators - 1 < modVal)
			{
                createErrorText.text = $"Can't have less than {modVal} spectators";
                SoundMessages.PlaySFX?.Invoke(errorAudioClip);
                return;
            }
            
            defaultSpectators--;
            spectatorsText.text = defaultSpectators.ToString();
        }

        public void CreateRoom()
        {
            if(!canCreateRoom)
            {
                SoundMessages.PlaySFX?.Invoke(errorAudioClip);
                return;
            }

            //Reset func on valid click
            canCreateRoom = false;
            createErrorText.text = "";

            MultiplayerManager.Instance.Specators = defaultSpectators;

            //Room options {Note: these values are used to keep player disconnection to the absolute minimum}
            RoomOptions roomOptions = new RoomOptions();
            int maxPlayers = defaultPlayers + defaultSpectators;
            //Debug.Log(maxPlayers);
            roomOptions.MaxPlayers = (byte)( maxPlayers);
            roomOptions.CleanupCacheOnLeave = MultiplayerManager.Instance.gameSettings.cleanupCacheOnLeave;
            roomOptions.PlayerTtl = MultiplayerManager.Instance.gameSettings.playerTTL;
            roomOptions.EmptyRoomTtl = MultiplayerManager.Instance.gameSettings.playerTTL;
            roomOptions.PublishUserId = true;

            //Add the essential room properties {no. of arenas }
            Hashtable roomProperties = new Hashtable();
            roomProperties.Add(Constants.Room.AvailableArenas, defaultArenas);
            roomProperties.Add(Constants.Room.OccupiedArenas, defaultArenas);
            roomProperties.Add(Constants.Room.RoomState, (int)RoomState.Unassigned_Arenas);
            roomOptions.CustomRoomProperties = roomProperties;

            //Create a room of random name
            string roomName = GenerateRandomRoomName();
            UtilEvents.ShowToastMessage?.Invoke("Creating Room: " + roomName);
            PhotonNetwork.CreateRoom(roomName, roomOptions, SessionData.gameLobby);
        }

        private string GenerateRandomRoomName()
        {
            int randomRoomNo =  Random.Range(1, 1000);
            return roomNoPrefix + randomRoomNo;

        }

        #endregion

        //Update Room UI when a room is created or destroy
        private void UpdateRoomListUI(List<RoomInfo> roomInfoList)
        {
            UpdateJoinRoomData();

            //If no rooms in cache then return
            if (roomInfoList.Count == 0) return;

            //If any room has availability then show that room else return 
            bool availableRooms = false;
            for (int i = 0; i < roomInfoList.Count; i++)
            {

                if (roomInfoList[i].PlayerCount > 0)
                {
                    availableRooms = true;
                    break;
                }

            }

            if (!availableRooms) return;

            noRoomsAvailable.SetActive(false);
            for (int i = 0; i < roomInfoList.Count; i++)
            {
                if (roomInfoList[i].PlayerCount == 0) continue;

                currentRoomNames[i] = roomInfoList[i].Name;
                joinRoomObjects[i].SetActive(true);
                joinRoomObjectTexts[i].text = roomInfoList[i].Name + $"\n<#E56F47><size= 38> Players: " +
                    $"({roomInfoList[i].PlayerCount}/{roomInfoList[i].MaxPlayers})";
                                       
            }
        }

        //Join a room
        public void JoinRoom(int roomNo)
        {
            if(!canJoinRoom)
            {
                SoundMessages.PlaySFX?.Invoke(errorAudioClip);
                return;
            }

            canJoinRoom = false;

            PhotonNetwork.JoinRoom(currentRoomNames[roomNo]);
        }

        public void JoinRoomThroughText()
        {
            if (!canJoinRoom)
            {
                SoundMessages.PlaySFX?.Invoke(errorAudioClip);
                return;
            }

            if(string.IsNullOrEmpty(joinRoomId.text))
            {
                foreach (var errorText in joinErrorText)
                {
                    errorText.text = "No Room Id Detected. Try Again!";
                }
                
                return;
            }
            
            canJoinRoom = false;

            PhotonNetwork.JoinRoom(roomNoPrefix + joinRoomId.text);
        }

        private void GoToLobbyMenu()
        {
            if (LocalPlayer.Instance.isMasterClient)
            {
                if (LocalPlayer.Instance.defaultPlayerType == PlayerType.Master_Spectator)
                    MultiplayerManager.Instance.SetPlayerPropertiesOnNetwork(0);
                else if (LocalPlayer.Instance.defaultPlayerType == PlayerType.Master_Player)
                    MultiplayerManager.Instance.SetPlayerPropertiesOnNetwork(-1);

                UtilEvents.ShowToastMessage?.Invoke("Loading Room...");
                Invoke(nameof(LoadLobby), 1f);
            }
        }

        //Creator of room loads the lobby scene, due to auto scene sync all the other memebers will follow
        private void LoadLobby()
        {
            PhotonNetwork.LoadLevel(Constants.LobbyScene);
        }
    }
}

