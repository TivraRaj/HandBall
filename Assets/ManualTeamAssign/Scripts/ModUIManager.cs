using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using TomoClub.Core;
using UnityEngine;
using UnityEngine.Diagnostics;

[RequireComponent(typeof(PhotonView))]
public class ModUIManager : MonoBehaviourPun
{
    [Header("Panels")]
    [SerializeField] private GameObject modPanel;

    [Header("Player List")]
    [SerializeField] private TMP_Text arenaLabel;
    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;
    [SerializeField] private Transform arenaPlayerList;

    [Header("Team Selection")]
    [SerializeField] private TMP_Text arenaName;
    [SerializeField] private Transform redTeamPlayerList;
    [SerializeField] private Transform blueTeamPlayerList;
    [SerializeField] private TeamSelectionUI redTeamSelection;
    [SerializeField] private TeamSelectionUI blueTeamSelection;

    [Header("Prefabs")]
    [SerializeField] private PlayerListingUI playerListPrefab;
    [SerializeField] private TeamListingUI teamListPrefab;

    private int currentArena = 0;
    private int arenaCount => MultiplayerManager.Instance.occupiedArenas;

    private PlayerListingUI currentPlayerListing;

    private List<PlayerListingUIList> playerListingUILists = new();
    private List<ArenaTeamsListingUIList> arenaTeamsListingUILists = new();

    [SerializeField] private PlayerUIManager playerUIManager;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        modPanel.SetActive(true);

        redTeamSelection.OnTeamClicked += TeamSelected;
        blueTeamSelection.OnTeamClicked += TeamSelected;

        InitPlayerArenaList();
        InitArenaTeamList();

        currentPlayerListing = null;
        currentArena = 0;

        if (arenaCount < 2)
        {
            leftButton.SetActive(false);
            rightButton.SetActive(false);
        }

        SwitchToArena(currentArena);
    }

    private void OnDestroy()
    {
        redTeamSelection.OnTeamClicked -= TeamSelected;
        blueTeamSelection.OnTeamClicked -= TeamSelected;

        foreach (PlayerListingUIList playerListingUIList in playerListingUILists)
        {
            foreach (PlayerListingUI playerListingUI in playerListingUIList.playerListingUIs)
            {
                playerListingUI.OnPlayerListingClicked -= HandlePlayerListingClick;
                playerListingUI.OnPlayerUnassignTeamClicked -= HandlePlayerUnassignTeamClick;
            }
        }
    }

    private void InitPlayerArenaList()
    {
        for (int i = 0; i < arenaCount; i++)
        {
            PlayerListingUIList newPlayerListingUIList = new();
            ArenaTeamsListingUIList arenaTeamsListingUIList = new();

            foreach (Player player in MultiplayerManager.Instance.arenaLists[i].arenaPlayers)
            {
                PlayerListingUI playerListingUI = GameObject.Instantiate<PlayerListingUI>(playerListPrefab);
                playerListingUI.InitPlayerDetail(player);
                playerListingUI.OnPlayerListingClicked += HandlePlayerListingClick;
                playerListingUI.OnPlayerUnassignTeamClicked += HandlePlayerUnassignTeamClick;
                playerListingUI.Init();

                if (MultiplayerManager.Instance.arenaTeamLists[i].redTeamPlayers.Contains(player) || MultiplayerManager.Instance.arenaTeamLists[i].blueTeamPlayers.Contains(player))
                {
                    playerListingUI.TeamAssigned();
                }

                newPlayerListingUIList.playerListingUIs.Add(playerListingUI);
                playerListingUI.transform.SetParent(arenaPlayerList, false);
            }

            playerListingUILists.Add(newPlayerListingUIList);
            arenaTeamsListingUILists.Add(arenaTeamsListingUIList);
        }
    }

    private void InitArenaTeamList()
    {
        for (int i = 0; i < MultiplayerManager.Instance.occupiedArenas; i++)
        {
            foreach (Player player in MultiplayerManager.Instance.arenaTeamLists[i].redTeamPlayers)
            {
                currentPlayerListing = playerListingUILists[i].playerListingUIs.Find(playerListingUI => playerListingUI.playerNameString == player.NickName);
                currentPlayerListing.TeamAssigned();
                CreateTeamPlayerListing(player, i, Team.Red);
                currentPlayerListing = null;
            }
            foreach (Player player in MultiplayerManager.Instance.arenaTeamLists[i].blueTeamPlayers)
            {
                currentPlayerListing = playerListingUILists[i].playerListingUIs.Find(playerListingUI => playerListingUI.playerNameString == player.NickName);
                currentPlayerListing.TeamAssigned();
                CreateTeamPlayerListing(player, i, Team.Blue);
                currentPlayerListing = null;
            }
        }
    }

    private void HideAllPlayers()
    {
        foreach (PlayerListingUIList playerListingUIList in playerListingUILists)
        {
            foreach (PlayerListingUI playerListingUI in playerListingUIList.playerListingUIs)
            {
                playerListingUI.gameObject.SetActive(false);
            }
        }
    }

    private void ShowArenaPlayers(int arenaNo)
    {
        foreach (PlayerListingUI playerListingUI in playerListingUILists[arenaNo].playerListingUIs)
        {
            playerListingUI.gameObject.SetActive(true);
        }
    }

    private void HideAllTeamPlayers()
    {
        foreach (ArenaTeamsListingUIList arenaTeamsListingUIList in arenaTeamsListingUILists)
        {
            foreach (TeamListingUI teamListingUI in arenaTeamsListingUIList.redTeamListingUIs)
            {
                teamListingUI.gameObject.SetActive(false);
            }

            foreach (TeamListingUI teamListingUI in arenaTeamsListingUIList.blueTeamListingUIs)
            {
                teamListingUI.gameObject.SetActive(false);
            }
        }
    }

    private void ShowArenaTeamPlayers(int arenaNo)
    {
        foreach (TeamListingUI teamListingUI in arenaTeamsListingUILists[arenaNo].redTeamListingUIs)
        {
            teamListingUI.gameObject.SetActive(true);
        }
        foreach (TeamListingUI teamListingUI in arenaTeamsListingUILists[arenaNo].blueTeamListingUIs)
        {
            teamListingUI.gameObject.SetActive(true);
        }
    }

    private void SwitchToArena(int arenaNo)
    {
        HideAllPlayers();
        HideAllTeamPlayers();
        UpdateArenaLabel();
        ShowArenaPlayers(arenaNo);
        ShowArenaTeamPlayers(arenaNo);
    }

    public void NextArena()
    {
        if (currentArena == arenaCount - 1)
            currentArena = 0;
        else
            currentArena++;

        if (currentPlayerListing)
        {
            currentPlayerListing.Deselect();
            DisableTeamSelection();
        }
        currentPlayerListing = null;

        SwitchToArena(currentArena);
    }

    public void PrevArena()
    {
        if (currentArena == 0)
            currentArena = arenaCount - 1;
        else
            currentArena--;

        if (currentPlayerListing)
        {
            currentPlayerListing.Deselect();
            DisableTeamSelection();
        }
        currentPlayerListing = null;

        SwitchToArena(currentArena);
    }

    private void UpdateArenaLabel()
    {
        arenaLabel.text = $"Arena {currentArena + 1}";
        arenaName.text = $"Arena {currentArena + 1}";
    }

    private void HandlePlayerListingClick(PlayerListingUI playerListingUI)
    {
        // Deselect on clicking a selected player list
        if (currentPlayerListing == playerListingUI)
        {
            currentPlayerListing.Deselect();
            currentPlayerListing = null;
            DisableTeamSelection();
            return;
        }

        // Deselect selected player when another player is clicked on
        if (currentPlayerListing)
        {
            currentPlayerListing.Deselect();
            DisableTeamSelection();
        }

        currentPlayerListing = playerListingUI;
        currentPlayerListing.Select();

        EnableTeamSelection();
    }

    private void EnableTeamSelection()
    {
        redTeamSelection.Highlight();
        blueTeamSelection.Highlight();
    }

    private void DisableTeamSelection()
    {
        redTeamSelection.DisableHighlight();
        blueTeamSelection.DisableHighlight();
    }

    private void HandlePlayerUnassignTeamClick(PlayerListingUI playerListingUI)
    {
        Player player = MultiplayerManager.Instance.arenaLists[currentArena].arenaPlayers.Find(player => player.NickName == playerListingUI.playerNameString);

        RemovePlayerFromTeam(player);

        playerListingUI.TeamRemoved();
    }

    private void TeamSelected(Team team)
    {
        AddSelectedPlayerToTeam(team);

        currentPlayerListing = null;
        DisableTeamSelection();
    }

    private void AddSelectedPlayerToTeam(Team team)
    {
        int maxPlayers = MultiplayerManager.Instance.arenaLists[currentArena].arenaPlayers.Count / 2;

        Player player = MultiplayerManager.Instance.arenaLists[currentArena].arenaPlayers.Find(player => player.NickName == currentPlayerListing.playerNameString);

        // Add player to list

        switch (team)
        {
            case Team.Red:
                if (MultiplayerManager.Instance.arenaTeamLists[currentArena].redTeamPlayers.Count < maxPlayers)
                {
                    MultiplayerManager.Instance.arenaTeamLists[currentArena].redTeamPlayers.Add(player);
                    break;
                }
                else
                {
                    currentPlayerListing.Deselect();
                    UtilEvents.ShowToastMessage?.Invoke($"Arena {currentArena + 1} Red Team is maxed out!");
                    return;
                }
            case Team.Blue:
                if (MultiplayerManager.Instance.arenaTeamLists[currentArena].blueTeamPlayers.Count < maxPlayers)
                {
                    MultiplayerManager.Instance.arenaTeamLists[currentArena].blueTeamPlayers.Add(player);
                    break;
                }
                else
                {
                    currentPlayerListing.Deselect();
                    UtilEvents.ShowToastMessage?.Invoke($"Arena {currentArena + 1} Blue Team is maxed out!");
                    return;
                }
        }

        // Set Player Team data on master client

        if (player == LocalPlayer.Instance.player && PhotonNetwork.IsMasterClient)
        {
            LocalPlayer.Instance.teamNo = (int)team;
            LocalPlayer.Instance.teamName = (TeamName)team;
        }

        // Create and add team listing

        CreateTeamPlayerListing(player, currentArena, team);

        // Mark player listing as team assigned

        currentPlayerListing.TeamAssigned();

        // RPC to player to set team data

        AddTeamDataOnNetwork(player, currentArena, (int)team);
    }

    private void AddTeamDataOnNetwork(Player player, int arenaNo, int team)
    {
        this.photonView.RPC(nameof(AddTeamDataOnPlayer), RpcTarget.Others, player, arenaNo, team);
    }

    private void RemoveTeamDataOnNetwork(Player player, int arenaNo, int team)
    {
        this.photonView.RPC(nameof(RemoveTeamDataOnPlayer), RpcTarget.Others, player, arenaNo, team);
    }

    [PunRPC]
    private void AddTeamDataOnPlayer(Player player, int arenaNo, int team)
    {
        if (player == LocalPlayer.Instance.player)
        {
            LocalPlayer.Instance.teamNo = team;
            LocalPlayer.Instance.teamName = (TeamName)team;

            playerUIManager.TeamAssigned((Team)team);
        }

        if ((Team)team == Team.Red)
            MultiplayerManager.Instance.arenaTeamLists[arenaNo].redTeamPlayers.Add(player);
        else
            MultiplayerManager.Instance.arenaTeamLists[arenaNo].blueTeamPlayers.Add(player);
    }

    [PunRPC]
    private void RemoveTeamDataOnPlayer(Player player, int arenaNo, int team)
    {
        if (player == LocalPlayer.Instance.player)
        {
            LocalPlayer.Instance.teamNo = -1;
            LocalPlayer.Instance.teamName = TeamName.None;

            playerUIManager.TeamRemoved();
        }

        if ((Team)team == Team.Red)
            MultiplayerManager.Instance.arenaTeamLists[arenaNo].redTeamPlayers.Remove(player);
        else
            MultiplayerManager.Instance.arenaTeamLists[arenaNo].blueTeamPlayers.Remove(player);
    }

    private void CreateTeamPlayerListing(Player player, int arenaNo, Team team)
    {
        TeamListingUI teamListingUI = GameObject.Instantiate<TeamListingUI>(teamListPrefab);
        teamListingUI.InitPlayerDetail(player);

        switch (team)
        {
            case Team.Red:
                arenaTeamsListingUILists[arenaNo].redTeamListingUIs.Add(teamListingUI);
                teamListingUI.SetPlayerTeam(team);
                teamListingUI.transform.SetParent(redTeamPlayerList, false);
                break;

            case Team.Blue:
                arenaTeamsListingUILists[arenaNo].blueTeamListingUIs.Add(teamListingUI);
                teamListingUI.SetPlayerTeam(team);
                teamListingUI.transform.SetParent(blueTeamPlayerList, false);
                break;
        }
    }

    private void RemovePlayerFromTeam(Player player)
    {
        if (MultiplayerManager.Instance.arenaTeamLists[currentArena].redTeamPlayers.Contains(player))
        {
            MultiplayerManager.Instance.arenaTeamLists[currentArena].redTeamPlayers.Remove(player);
            RemoveTeamPlayerListing(player, Team.Red);

            RemoveTeamDataOnNetwork(player, currentArena, (int)Team.Red);
        }
        else
        {
            MultiplayerManager.Instance.arenaTeamLists[currentArena].blueTeamPlayers.Remove(player);
            RemoveTeamPlayerListing(player, Team.Blue);

            RemoveTeamDataOnNetwork(player, currentArena, (int)Team.Blue);
        }
    }

    private void RemoveTeamPlayerListing(Player player, Team team)
    {
        TeamListingUI teamListingUI;
        switch (team)
        {
            case Team.Red:
                teamListingUI = arenaTeamsListingUILists[currentArena].redTeamListingUIs.Find(teamListing => teamListing.playerNameString == player.NickName);

                arenaTeamsListingUILists[currentArena].redTeamListingUIs.Remove(teamListingUI);
                Destroy(teamListingUI.gameObject);

                break;

            case Team.Blue:
                teamListingUI = arenaTeamsListingUILists[currentArena].blueTeamListingUIs.Find(teamListing => teamListing.playerNameString == player.NickName);

                arenaTeamsListingUILists[currentArena].blueTeamListingUIs.Remove(teamListingUI);
                Destroy(teamListingUI.gameObject);

                break;
        }
    }

    public void BackToLobby()
    {
        MultiplayerManager.Instance.ResetRoom();
    }

    public void Play()
    {
        for (int i = 0; i < MultiplayerManager.Instance.occupiedArenas; i++)
        {
            if (MultiplayerManager.Instance.arenaLists[i].arenaPlayers.Count != MultiplayerManager.Instance.arenaTeamLists[i].redTeamPlayers.Count + MultiplayerManager.Instance.arenaTeamLists[i].blueTeamPlayers.Count)
            {
                UtilEvents.ShowToastMessage?.Invoke($"All players in Arena {i + 1} have not been assigned teams");
                return;
            }
        }

        PhotonNetwork.LoadLevel(Constants.GameScene);
    }

    public void SettingsButton()
    {
        if (PersistantUI.Instance != null)
            PersistantUI.Instance.ShowSettingsPopup();
    }
}

public class PlayerListingUIList
{
    public List<PlayerListingUI> playerListingUIs = new();
}

public class ArenaTeamPlayersList
{
    public List<Player> redTeamPlayers = new();
    public List<Player> blueTeamPlayers = new();
}

public class ArenaTeamsListingUIList
{
    public List<TeamListingUI> redTeamListingUIs = new();
    public List<TeamListingUI> blueTeamListingUIs = new();
}
