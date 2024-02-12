using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using TomoClub.Core;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject playerPanel;

    [Header("Arena Info")]
    [SerializeField] private GameObject arenaInfo;
    [SerializeField] private TMP_Text arenaLabel;

    [Header("Label UIs")]
    [SerializeField] private TMP_Text waitingStatus;
    [SerializeField] private GameObject assignedText;
    [SerializeField] private GameObject redTeamText;
    [SerializeField] private GameObject blueTeamText;
    [SerializeField] private GameObject noTeamGO;

    [SerializeField] private GameObject spectatorGO;
    [SerializeField] private GameObject arenaGO;


    private const string waitingForAssign = "Waiting for Mod to assign you to a team";
    private const string waitingForGameStart = "Waiting for Mod to start the game";

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        spectatorGO.SetActive(LocalPlayer.Instance.defaultPlayerType == PlayerType.Spectator);
        arenaGO.SetActive(LocalPlayer.Instance.defaultPlayerType == PlayerType.Player);

        playerPanel.SetActive(true);

        waitingStatus.text = LocalPlayer.Instance.defaultPlayerType == PlayerType.Spectator ? waitingForGameStart : waitingForAssign;

        arenaLabel.text = $"Arena {LocalPlayer.Instance.arenaNo}";

        CheckAndAssignTeam();
    }

    private void CheckAndAssignTeam()
    {
        if(LocalPlayer.Instance.teamNo < 0)
            return;

        TeamAssigned((Team) LocalPlayer.Instance.teamNo);
    }

    public void TeamAssigned(Team team)
    {
        waitingStatus.text = waitingForGameStart;

        noTeamGO.SetActive(false);
        assignedText.SetActive(true);
        if (team == Team.Red)
            redTeamText.SetActive(true);
        else
            blueTeamText.SetActive(true);
    }

    public void TeamRemoved()
    {
        waitingStatus.text = waitingForAssign;

        noTeamGO.SetActive(true);
        assignedText.SetActive(false);
        redTeamText.SetActive(false);
        blueTeamText.SetActive(false);
    }
}
