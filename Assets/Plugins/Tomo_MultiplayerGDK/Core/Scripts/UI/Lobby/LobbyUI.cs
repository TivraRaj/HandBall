using TMPro;
using UnityEngine;
using TomoClub.Core;
using Photon.Realtime;
using System.Collections.Generic;

public class LobbyUI : MonoBehaviour
{
    [Header("Lobby UI")]
    [SerializeField] TextMeshProUGUI Text_CurrentRoomName;
    [SerializeField] GameObject Popup_PlayerLobbyList;

    [Header("Lobby List")]
    [SerializeField] TextMeshProUGUI Text_LobbyPlayerListTitle;
    [SerializeField] GameObject[] LobbyPlayers;

    private TextMeshProUGUI[] playerLobbyListText;

    private void Awake()
    {
        Text_CurrentRoomName.text = MultiplayerManager.Instance.currentRoomName;

        Popup_PlayerLobbyList.SetActive(false);

        //Player Lobby List Init
        playerLobbyListText = new TextMeshProUGUI[LobbyPlayers.Length];

        for (int i = 0; i < LobbyPlayers.Length; i++)
        {
            playerLobbyListText[i] = LobbyPlayers[i].GetComponentInChildren<TextMeshProUGUI>();
            LobbyPlayers[i].SetActive(false);
        }

    }

	private void OnEnable()
	{
        MultiplayerMesseges.OnUpdateLobbyList += UpdatePlayerLobbyList;
    }

	private void OnDisable()
	{
        MultiplayerMesseges.OnUpdateLobbyList -= UpdatePlayerLobbyList;
    }

	private void UpdatePlayerLobbyList(List<Player> allPlayers)
    {
        Text_LobbyPlayerListTitle.text = $"Lobby <size=40><#6A6A6A> Total Players: {allPlayers.Count}";
        for (int i = 0; i < LobbyPlayers.Length; i++)
        {
            if (i < allPlayers.Count)
            {
                LobbyPlayers[i].SetActive(true);
                if (allPlayers[i].IsMasterClient)
                {
                    playerLobbyListText[i].text = allPlayers[i].NickName + " (Mod)";

                }
                else
                {
                    bool isSpectator = allPlayers[i].CustomProperties[Constants.Player.ArenaNo] != null && (int)allPlayers[i].CustomProperties[Constants.Player.ArenaNo] == 0;
                    playerLobbyListText[i].text = isSpectator ? allPlayers[i].NickName + " (Spec)" : allPlayers[i].NickName;
                }

            }
            else
                LobbyPlayers[i].SetActive(false);
        }

    }

    public void CopyPlayerGameLink()
    {
        string gameLink = SessionData.webGLLink;
        GUIUtility.systemCopyBuffer = gameLink + $" Room ID: {MultiplayerManager.Instance.currentRoomName}";
    }

    //Open Common Settings Menu on click
    public void SettingsButton()
    {
        if (PersistantUI.Instance != null)
            PersistantUI.Instance.ShowSettingsPopup();
    }

    //Open/Close LobbyPlayerList
    public void TogglePlayerLobbyList(bool status) => Popup_PlayerLobbyList.SetActive(status);

}
