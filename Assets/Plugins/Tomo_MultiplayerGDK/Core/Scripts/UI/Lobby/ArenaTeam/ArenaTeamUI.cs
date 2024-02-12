using Photon.Pun;
using UnityEngine;
using System.Collections;
using TMPro;
using TomoClub.Core;

public class ArenaTeamUI : MonoBehaviour
{
    [Header("ArenaTeams_Common")]
    [SerializeField] GameObject Popup_ArenaTeams;
    [SerializeField] GameObject Panel_Common;
    [SerializeField] TextMeshProUGUI Title_ArenaNo;
    [SerializeField] TeamArenaListing[] teamArenaListings;
    

    [Header("ArenaTeams_Moderator")]
    [SerializeField] GameObject Panel_Moderator;
    [SerializeField] GameObject playButton;
    [SerializeField] GameObject toggleArenaButtons;
    
    [Header("ArenaTeams_Player")]
    [SerializeField] GameObject Panel_Player;
    [SerializeField] GameObject Button_ReadyUp;

    public static Sprite[] playerReadyUpSprites;

    private bool[][] redTeamPlayerReadyUpStatus;
    private bool[][] blueTeamPlayerReadyUpStatus;

    private int currentTeamArenaListing = 1;

    private void Awake() => Init();

    private void Init()
    {
        LoadReadyUpSprites();

        Panel_Common.SetActive(true);
        Panel_Moderator.SetActive(LocalPlayer.Instance.isRoomOwner);
        Panel_Player.SetActive(!LocalPlayer.Instance.isdefaultSpec);
        playButton.SetActive(false);
        toggleArenaButtons.SetActive(false);
        Button_ReadyUp.SetActive(true);
        Popup_ArenaTeams.SetActive(false);

        Title_ArenaNo.text = "Arena 1";

    }

    private void LoadReadyUpSprites()
    {
        Object[] readyUpSprites = Resources.LoadAll<Sprite>("Sprites/ReadyUp");
        playerReadyUpSprites = new Sprite[readyUpSprites.Length];
        for (int i = 0; i < readyUpSprites.Length; i++)
        {
            playerReadyUpSprites[i] = (Sprite)readyUpSprites[i];
        }

        //Init the sprite on the listings
        for (int i = 0; i < teamArenaListings.Length; i++)
        {
            teamArenaListings[i].TeamPlayerListingsInit();
        }
    }

    private void OnEnable()
    {
        UserEvents.UpdatePlayerReadyUp += ChangeReadyUpStatusOfPlayer;
        MultiplayerMesseges.OnUpdateArenaTeamLists += UpdateArenaTeamsLists;
    }

    private void OnDisable()
    {
        UserEvents.UpdatePlayerReadyUp -= ChangeReadyUpStatusOfPlayer;
        MultiplayerMesseges.OnUpdateArenaTeamLists -= UpdateArenaTeamsLists;
    }
    
    public void UpdateArenaTeamsLists()
    {
        for (int i = 0; i < MultiplayerManager.Instance.arenaTeamLists.Length; i++)
        {
            for (int j = 0; j < MultiplayerManager.Instance.arenaTeamLists[i].redTeamPlayers.Count; j++)
            {
                string playerName = MultiplayerManager.Instance.arenaTeamLists[i].redTeamPlayers[j].NickName;
                teamArenaListings[i].teamPlayerListings_Red[j].UpdateListing(playerName);
            }

            for (int j = 0; j < MultiplayerManager.Instance.arenaTeamLists[i].blueTeamPlayers.Count; j++)
            {
                string playerName = MultiplayerManager.Instance.arenaTeamLists[i].blueTeamPlayers[j].NickName;
                teamArenaListings[i].teamPlayerListings_Blue[j].UpdateListing(playerName);
            }
        }

        if(AllTeamsHaveBeenUpdated(ref MultiplayerManager.Instance.arenaTeamLists))
        {
            TurnOnArenaTeamsPanel(ref MultiplayerManager.Instance.arenaTeamLists);
        }
    }

    private void UpdateTeamArenaListingObjects(ref ArenaTeamList[] arenaTeamLists)
    {
        for (int i = 0; i < teamArenaListings.Length; i++)
        {
            bool isActiveArena = i < MultiplayerManager.Instance.occupiedArenas;
            if (isActiveArena) teamArenaListings[i].UpdateTeamPlayerListingObjects(arenaTeamLists[i].redTeamPlayers.Count, arenaTeamLists[i].blueTeamPlayers.Count);
            teamArenaListings[i].UpdateTeamListingHolder(false);
        }

        if (!LocalPlayer.Instance.isRoomOwner)
        {
            teamArenaListings[LocalPlayer.Instance.arenaNo - 1].UpdateTeamListingHolder(true);
            Title_ArenaNo.text = $"Arena {LocalPlayer.Instance.arenaNo}";
        }

        if (LocalPlayer.Instance.isRoomOwner)
        {
            teamArenaListings[0].UpdateTeamListingHolder(true);
            Title_ArenaNo.text = $"Arena 1";
            toggleArenaButtons.SetActive(MultiplayerManager.Instance.occupiedArenas > 1);
        }


    }

    private void TurnOnArenaTeamsPanel(ref ArenaTeamList[] arenaTeamLists)
    {
        UpdateTeamArenaListingObjects(ref arenaTeamLists);

        redTeamPlayerReadyUpStatus = new bool[MultiplayerManager.Instance.occupiedArenas][];
        blueTeamPlayerReadyUpStatus = new bool[MultiplayerManager.Instance.occupiedArenas][];

        for (int i = 0; i < redTeamPlayerReadyUpStatus.Length; i++)
        {
            redTeamPlayerReadyUpStatus[i] = new bool[arenaTeamLists[i].redTeamPlayers.Count];
        }

        for (int i = 0; i < blueTeamPlayerReadyUpStatus.Length; i++)
        {
            blueTeamPlayerReadyUpStatus[i] = new bool[arenaTeamLists[i].blueTeamPlayers.Count];
        }

        Popup_ArenaTeams.SetActive(true);
    }

    private bool AllTeamsHaveBeenUpdated(ref ArenaTeamList[] arenaTeamLists)
    {
        for (int i = 0; i < MultiplayerManager.Instance.occupiedArenas; i++)
        {
            if (arenaTeamLists[i].blueTeamPlayers.Count == 0 || arenaTeamLists[i].redTeamPlayers.Count == 0) return false;
            if (arenaTeamLists[i].blueTeamPlayers.Count + arenaTeamLists[i].redTeamPlayers.Count != MultiplayerManager.Instance.arenaLists[i].arenaPlayers.Count) return false;
        }

        return true;
    }

    private void ChangeReadyUpStatusOfPlayer(int arenaNo, int teamNo, int playerIndexInTeamList)
    {
        //Change Based on team no
        switch ((TeamName)teamNo)
        {
            case TeamName.Red:
                teamArenaListings[arenaNo - 1].teamPlayerListings_Red[playerIndexInTeamList].UpdateListingReadyStatus(true);
                redTeamPlayerReadyUpStatus[arenaNo - 1][playerIndexInTeamList] = true;
                break;
            case TeamName.Purple:
                teamArenaListings[arenaNo - 1].teamPlayerListings_Blue[playerIndexInTeamList].UpdateListingReadyStatus(true);
                blueTeamPlayerReadyUpStatus[arenaNo - 1][playerIndexInTeamList] = true;
                break;
            case TeamName.None:
                Debug.LogError("Team is None, check logic, something is wrong");
                break;
            default:
                Debug.LogError("Team is Null, not possible, check logic, something is really wrong");
                break;
        }

        bool canPlayGame = LocalPlayer.Instance.isRoomOwner && AllPlayersAreReady();
        playButton.SetActive(canPlayGame);
        
    }

    private bool AllPlayersAreReady()
    {
        for (int i = 0; i < redTeamPlayerReadyUpStatus.Length; i++)
        {
            for (int j = 0; j < redTeamPlayerReadyUpStatus[i].Length; j++)
            {
                if(redTeamPlayerReadyUpStatus[i][j] == false) return false;
            }
        }

        for (int i = 0; i < blueTeamPlayerReadyUpStatus.Length; i++)
        {
            for (int j = 0; j < blueTeamPlayerReadyUpStatus[i].Length; j++)
            {
                if (blueTeamPlayerReadyUpStatus[i][j] == false) return false;
            }
        }

        return true;
    }

    //Button Functions
    public void PlayerReadyUp()
    {
        Button_ReadyUp.SetActive(false);
        NetworkEvents.Instance.PlayerReadyUp();
    }

    public void PlayGame()
    {
       StartGameForIntraArena();
    }

    private void StartGameForIntraArena()
    {
        if (LocalPlayer.Instance.isRoomOwner)
        {
            MultiplayerManager.Instance.currentRoom.IsOpen = false;
            MultiplayerManager.Instance.currentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel(Constants.GameScene);
        }
    }

    public void ToggleArenaTeamListPrev()
    {
        teamArenaListings[currentTeamArenaListing - 1].UpdateTeamListingHolder(false);
        currentTeamArenaListing = currentTeamArenaListing - 1 < 1 ? MultiplayerManager.Instance.occupiedArenas : currentTeamArenaListing - 1;
        teamArenaListings[currentTeamArenaListing - 1].UpdateTeamListingHolder(true);
        Title_ArenaNo.text = $"Arena {currentTeamArenaListing}";

    }

    public void ToggleArenaTeamListNext()
    {
        teamArenaListings[currentTeamArenaListing - 1].UpdateTeamListingHolder(false);
        currentTeamArenaListing = currentTeamArenaListing + 1 > MultiplayerManager.Instance.occupiedArenas ?  1 : currentTeamArenaListing + 1;
        teamArenaListings[currentTeamArenaListing - 1].UpdateTeamListingHolder(true);
        Title_ArenaNo.text = $"Arena {currentTeamArenaListing}";
    }
}
