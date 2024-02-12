using UnityEngine;
using TMPro;
using System.Collections.Generic;
using TomoClub.Core;
using Photon.Realtime;

public class PlayerLobbyUI : MonoBehaviour
{
    [Header("Lobby UI")]
    [SerializeField] GameObject Panel_PlayerLobby;
    [SerializeField] GameObject Popup_PlayerArenaList;

    [Header("Arena List")]
    [SerializeField] GameObject[] ArenaPlayers;
    [SerializeField] GameObject Button_Arena;
    [SerializeField] GameObject GameObject_Spectator;
    [SerializeField] TextMeshProUGUI Text_ArenaPlayersTitle;
    [SerializeField] TextMeshProUGUI Text_ArenaPlayersButtonTitle;


    [Header("Tutorial")]
    [SerializeField] GameObject TutorialPrefab;
    [SerializeField] Transform TutorialParent;

    private TextMeshProUGUI[] playerArenaListText;

    private void Awake() => Init();

    private void OnEnable()
    {
        MultiplayerMesseges.OnUpdateMyArenaList += UpdatePlayerArenaList;
    }

    private void OnDisable()
    {

        MultiplayerMesseges.OnUpdateMyArenaList -= UpdatePlayerArenaList;
    } 
    
    private void Init()
    {

        GameObject_Spectator.SetActive(LocalPlayer.Instance.defaultPlayerType == PlayerType.Spectator);
        //Player Arena List Init
        playerArenaListText = new TextMeshProUGUI[ArenaPlayers.Length];

        for (int i = 0; i < ArenaPlayers.Length; i++)
        {
            playerArenaListText[i] = ArenaPlayers[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            ArenaPlayers[i].SetActive(false);
        }

        Popup_PlayerArenaList.SetActive(false);
        Button_Arena.SetActive(LocalPlayer.Instance.arenaNo > 0);

        //Instantiate Tutorial
        if(TutorialPrefab != null)
            Instantiate(TutorialPrefab, TutorialParent.position, Quaternion.identity, TutorialParent);

        Panel_PlayerLobby.SetActive(!LocalPlayer.Instance.isRoomOwner);
        gameObject.SetActive(!LocalPlayer.Instance.isRoomOwner);

    }

    //Open/Close ArenaPlayerList
    public void TogglePlayerArenaList(bool status) => Popup_PlayerArenaList.SetActive(status);

    private void UpdatePlayerArenaList(List<Player> currentArenaPlayers)
    {

        Button_Arena.SetActive(LocalPlayer.Instance.arenaNo >= 1);

        if (LocalPlayer.Instance.arenaNo < 1) return; 

        for (int i = 1; i < ArenaPlayers.Length; i++)
        {
            ArenaPlayers[i].SetActive(false);
        }

        for (int i = 0; i < currentArenaPlayers.Count; i++)
        {
            ArenaPlayers[i].SetActive(true);
            playerArenaListText[i].text = currentArenaPlayers[i].NickName;
        }

        Text_ArenaPlayersTitle.text = $"Arena {LocalPlayer.Instance.arenaNo} <size=40><#6A6A6A> Total Players: {currentArenaPlayers.Count}";
        Text_ArenaPlayersButtonTitle.text = $"Arena {LocalPlayer.Instance.arenaNo}";

    }


}
