using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerListingUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerName;
    [SerializeField] private Image image;
    [SerializeField] private Button button;
    [SerializeField] private GameObject unassignButton;
    [SerializeField] private GameObject unassignText;

    [Header("Colors")]
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color assignedColor;

    public UnityAction<PlayerListingUI> OnPlayerListingClicked;
    public UnityAction<PlayerListingUI> OnPlayerUnassignTeamClicked;

    public string playerNameString { get { return playerName.text; } private set { } }

    public void Init()
    {
        unassignButton.SetActive(false);
    }

    public void InitPlayerDetail(Player player)
    {
        playerName.text = player.NickName;
    }

    public void OnClick()
    {
        OnPlayerListingClicked?.Invoke(this);
    }

    public void OnUnassignTeamClick()
    {
        OnPlayerUnassignTeamClicked?.Invoke(this);
    }

    public void Select()
    {
        image.color = Color.green;
    }

    public void Deselect()
    {
        image.color = defaultColor;
    }

    public void TeamAssigned()
    {
        image.color = assignedColor;
        button.enabled = false;

        unassignButton.SetActive(true);
    }

    public void TeamRemoved()
    {
        image.color = defaultColor;
        button.enabled = true;

        unassignText.SetActive(false);
        unassignButton.SetActive(false);
    }
}
