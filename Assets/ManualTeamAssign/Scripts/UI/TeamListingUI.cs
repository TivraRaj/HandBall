using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamListingUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Image image;

    [Header("Colors")]
    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;

    private Team team;

    public string playerNameString { get { return playerName.text; } private set { } }

    public void InitPlayerDetail(Player player)
    {
        playerName.text = player.NickName;
    }

    public void SetPlayerTeam(Team _team)
    {
        team = _team;
        if (team == Team.Red)
            image.color = redColor;
        else if (team == Team.Blue)
            image.color = blueColor;
    }
}
