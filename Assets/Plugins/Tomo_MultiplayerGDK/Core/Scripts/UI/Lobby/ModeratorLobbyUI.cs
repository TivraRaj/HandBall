using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TomoClub.Core;

//Manages Moderator Lobby UI
public class ModeratorLobbyUI : MonoBehaviour
{
    [Header("Lobby UI")]
    [SerializeField] GameObject Panel_ModeratorSideUI;
    [SerializeField] GameObject Button_RandomizeArenas;
    [SerializeField] GameObject Button_StartButton;

    [Header("Player List")]
    [SerializeField] TextMeshProUGUI Text_PlayerListTitle_Mod;
    [SerializeField] GameObject NoPlayers;
    [SerializeField] PlayerListing[] playerListings;
    
    [Header("Arena List")]
    [SerializeField] GridLayoutGroup arenaGridLayout;
    [SerializeField] Vector2Int baseGridCellSize;
    [SerializeField] GameObject[] arenaPlayerListing;

    [Header("Common Popup")]
    [SerializeField] GameObject Popup_Common;
    [SerializeField] TextMeshProUGUI Popup_Common_Title;
    [SerializeField] TextMeshProUGUI Popup_Common_Text;

    private TextMeshProUGUI[] arenaPlayerListingText;
    private Player playerToKickOut = null;

    private enum PopupType { CloseRoomConfirm, BanPlayerConfirm };
    private PopupType currentPopupType = PopupType.CloseRoomConfirm;
 
    private void Awake() => Init();

    private void OnEnable()
    {
        MultiplayerMesseges.OnUpdateGamePlayerList += UpdatePlayerList;
        MultiplayerMesseges.OnUpdateArenaLists += UpdateArenaLists;
        MultiplayerMesseges.OnUpdatePlayerArena += UpdatePlayerArenaOnListing;
    }

    private void OnDisable()
    {
        MultiplayerMesseges.OnUpdateGamePlayerList -= UpdatePlayerList;
        MultiplayerMesseges.OnUpdateArenaLists -= UpdateArenaLists;
        MultiplayerMesseges.OnUpdatePlayerArena -= UpdatePlayerArenaOnListing;

    }

    private void Init()
    {
        ResetPlayerListings();
        NoPlayers.SetActive(true);

        arenaPlayerListingText = new TextMeshProUGUI[arenaPlayerListing.Length];

        for (int i = 0; i < arenaPlayerListing.Length; i++)
        {
            arenaPlayerListingText[i] = arenaPlayerListing[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            arenaPlayerListingText[i].text = "No Assigned Players";
            arenaPlayerListing[i].SetActive(false);
        }

        Button_StartButton.SetActive(true);
        Button_RandomizeArenas.SetActive(MultiplayerManager.Instance.CanRandomizeArenas());

        Text_PlayerListTitle_Mod.text = $"0 <#b3bedb>/</color> {MultiplayerManager.Instance.MaxPlayers}";

        Panel_ModeratorSideUI.SetActive(LocalPlayer.Instance.isRoomOwner);
        gameObject.SetActive(LocalPlayer.Instance.isRoomOwner);

    }

    private void Start()
    {
        SetArenaPlayersUI();
    }

    private void SetArenaPlayersUI()
    {
        int availableArenas = MultiplayerManager.Instance.availableArenas;
        int divideFactorX = availableArenas > 1 ? 2 : 1;
        int divideFactorY = availableArenas > 2 ? 2 : 1;
        arenaGridLayout.cellSize = new Vector2(baseGridCellSize.x / divideFactorX, baseGridCellSize.y / divideFactorY);

        for (int i = 0; i < availableArenas; i++)
        {
            arenaPlayerListing[i].SetActive(true);
        }

    }

    private void ResetPlayerListings()
    {
        for (int i = 0; i < playerListings.Length; i++)
        {
            playerListings[i].gameObject.SetActive(false);
        }
    }

    //Update player list when player joins/leaves room
    private void UpdatePlayerList(List<Player> players)
    {
        ResetPlayerListings();

        Button_RandomizeArenas.SetActive(MultiplayerManager.Instance.CanRandomizeArenas());
        NoPlayers.SetActive(players.Count == 0);
        Text_PlayerListTitle_Mod.text = $"{players.Count} <#b3bedb>/</color> {MultiplayerManager.Instance.MaxPlayers}";

        for (int i = 0; i < playerListings.Length; i++)
        {

            if (i < players.Count)
            {
                playerListings[i].gameObject.SetActive(true);
                playerListings[i].UpdateListing(players[i], this);
                int arenaNo = players[i].CustomProperties[Constants.Player.ArenaNo] == null ? -1 : (int)players[i].CustomProperties[Constants.Player.ArenaNo];
                if (arenaNo > 0)
                    UpdatePlayerArenaOnListing(i, arenaNo);

            }
            else
                playerListings[i].gameObject.SetActive(false);
        }
            
    }

    //Update the UI for a moderator when they rejoin the room lobby
    private void UpdateArenaLists(ArenaList[] arenaLists)
    {

        for (int i = 0; i < arenaLists.Length; i++)
        {
            arenaPlayerListingText[i].text = "";

            if (arenaLists[i].arenaPlayers.Count == 0)
            {
                arenaPlayerListingText[i].text = "No Assigned Players";
                continue;
            }

            for (int j = 0; j < arenaLists[i].arenaPlayers.Count; j++)
            {
                arenaPlayerListingText[i].text += string.IsNullOrEmpty(arenaPlayerListingText[i].text) ?
                    arenaLists[i].arenaPlayers[j].NickName : $", {arenaLists[i].arenaPlayers[j].NickName}";

            }
                
        }
    }

    //Update the UI for the player arena manual buttons
    private void UpdatePlayerArenaOnListing(int playerNo, int arenaNo)
    {
        playerListings[playerNo].UpdateArenaOnListing(arenaNo); 
    }

    //Text to show for popup 
    private void ChangePopupTextBasedOnType()
    {
        switch (currentPopupType)
        {
            case PopupType.CloseRoomConfirm:
                Popup_Common_Title.text = "Close Room?";
                Popup_Common_Text.text = $"Are you sure you want to close {PhotonNetwork.CurrentRoom.Name}?";
                break;
            case PopupType.BanPlayerConfirm:
                Popup_Common_Title.text = "Kick Player?";
                Popup_Common_Text.text = $"Are you sure you want to kick Player: {playerToKickOut.NickName}?";
                break;
            default:
                break;
        }
    }

    #region Class Public Functions

    public void SetPlayerToKickOut(Player player)
    {
        playerToKickOut = player;
        currentPopupType = PopupType.BanPlayerConfirm;
        UpdatePopupStatus(true);
    }

    #endregion

    #region Moderator Lobby Button Functions


    //Close current room
    public void OnHomeButtonHit()
    {
        currentPopupType = PopupType.CloseRoomConfirm;
        UpdatePopupStatus(true);
    }

    //Assign Arenas to all the players currently in room
    public void AssignArenasToPlayersInLobby() => MultiplayerManager.Instance.AssignRandomArenas();

    //Open/Close for Kick Player/Close Room
    public void UpdatePopupStatus(bool enable)
    {
        if (enable) ChangePopupTextBasedOnType();

        Popup_Common.SetActive(enable);
    }

    //On popup confirmation
    public void ConfirmPopup()
    {
        switch (currentPopupType)
        {
            case PopupType.CloseRoomConfirm:
                NetworkEvents.Instance.Moderator_CloseRoom();
                break;
            case PopupType.BanPlayerConfirm:
                NetworkEvents.Instance.CloseRoomForPlayer(playerToKickOut);
                break;
            default:
                break;
        }

        Popup_Common.SetActive(false);
    }

    //Play Game
    public void StartGame() => MultiplayerManager.Instance.StartGameSession();


    #endregion


}
