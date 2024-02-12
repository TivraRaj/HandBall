using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TomoClub.Core;

public class PlayerListing : MonoBehaviour
{
    [Header("Player Listing UI")]
    [SerializeField] GameObject playerTextObject;
    [SerializeField] GameObject playerArenaOptions;
    [SerializeField] GameObject assignAreansText;
    [SerializeField] Image[] assignArenaImages;

    private Player player;
    private ModeratorLobbyUI moderatorLobbyUIManager;
    private TextMeshProUGUI playerText;

    private void Awake() => Init();

    private void Start() => SetInitialArenas();

    private void Init()
    {
        playerTextObject.SetActive(true);
        playerArenaOptions.SetActive(false);
        assignAreansText.SetActive(false);

        playerText = playerTextObject.GetComponent<TextMeshProUGUI>();
    }

    private void SetInitialArenas()
    {
        for (int i = 0; i < assignArenaImages.Length; i++)
        {
            assignArenaImages[i].gameObject.SetActive(i < MultiplayerManager.Instance.availableArenas);
            assignArenaImages[i].color = Color.green;
        }
    }

    private void ResetArenaImages()
    {
        for (int i = 0; i < MultiplayerManager.Instance.availableArenas; i++)
        {
            assignArenaImages[i].color = Color.green;
        }
    }
    
    //Set player listing
    public void UpdateListing(Player targetPlayer, ModeratorLobbyUI modLobbyUIMan)
    {
        moderatorLobbyUIManager = modLobbyUIMan;
        player = targetPlayer;

        if (player.CustomProperties[Constants.Player.Ping] == null)
		{
            playerText.text = player.NickName;
        }
        else
		{
            int ping = (int)player.CustomProperties[Constants.Player.Ping];
            string color = ping > 250 ? "red" : "green";
            playerText.text = $"{player.NickName}\n<size=30><color=\"{color}\">Ping: {ping}</size></color>";

        }
        
        UpdateArenaOnListing();
    }

    //Arena no needs to be (1 indexed)
    public void AssignOrDeassignArenaToPlayer(int arenaNo)
    {
        if (MultiplayerManager.Instance.arenaLists[arenaNo - 1].arenaPlayers.Count == MultiplayerManager.Instance.playersPerArenaRange.y)
        {
            UtilEvents.ShowToastMessage?.Invoke($"Sorry, Arena {arenaNo} is maxed out!");
            return;
        }

        int currentArenaNo = player.CustomProperties[Constants.Player.ArenaNo] == null ? -1 : (int)player.CustomProperties[Constants.Player.ArenaNo];

        //Reset Player to no arena 
        if (currentArenaNo == arenaNo)
        {
            MultiplayerManager.Instance.playerProperties[Constants.Player.ArenaNo] = -1;
            player.SetCustomProperties(MultiplayerManager.Instance.playerProperties);
            assignArenaImages[arenaNo - 1].color = Color.green;
            UtilEvents.ShowToastMessage?.Invoke($"Deassigned Arena for Player: {player.NickName}");
            return;
        }

        //Reassign New Arena to player 
        MultiplayerManager.Instance.playerProperties[Constants.Player.ArenaNo] = arenaNo;
        player.SetCustomProperties(MultiplayerManager.Instance.playerProperties);
        if (currentArenaNo >= 1) assignArenaImages[currentArenaNo - 1].color = Color.green;
        assignArenaImages[arenaNo - 1].color = Color.red;
        UtilEvents.ShowToastMessage?.Invoke($"Player: {player.NickName} is assigned Arena {arenaNo}");

    }

    public void UpdateArenaOnListing()
    {
        ResetArenaImages();
        int playerArenaNo = player.CustomProperties[Constants.Player.ArenaNo] != null ? (int)player.CustomProperties[Constants.Player.ArenaNo] : -1;
        if(playerArenaNo >= 1) assignArenaImages[playerArenaNo - 1].color = Color.red;

    }

    public void UpdateArenaOnListing(int arenaNo)
    {
        ResetArenaImages();
        assignArenaImages[arenaNo - 1].color = Color.red;

    }


    //Set current player to kick
    public void KickPlayer()
    {
        moderatorLobbyUIManager.SetPlayerToKickOut(player);
    }

    public void FlipPlayerListing()
    {
        if(playerArenaOptions.activeSelf)
        {
            playerTextObject.SetActive(true);
            playerArenaOptions.SetActive(false);
            assignAreansText.SetActive(false);
        }
        else if(playerTextObject.activeSelf)
        {
            playerTextObject.SetActive(false);
            playerArenaOptions.SetActive(true);
            assignAreansText.SetActive(true);
        }
    }


}
