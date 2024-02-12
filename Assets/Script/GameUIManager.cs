using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private TextMeshProUGUI teamWonText;
    [SerializeField] private TextMeshProUGUI goalText;

    [SerializeField] private TextMeshProUGUI redTeamScore;
    [SerializeField] private TextMeshProUGUI blueTeamScore;

    private void Awake()
    {
        SetGameOverUIActiveStatus(false);
        SetGoalTextInfo(false, TextColorEnum.None);
    }

    public void SetGameOverUIActiveStatus(bool activeStatus)
    {
        gameOverUI.SetActive(activeStatus);
    }

    public void SetTeamWonTextInfo(string text, TextColorEnum color)
    {
        teamWonText.SetText(text);
        teamWonText.color = TextColor.GetColorFromEnum(color);
    }

    public void SetRedTeamScoreText(int redTeamScore)
    {
        this.redTeamScore.SetText(redTeamScore.ToString("0"));
    }

    public void SetBlueTeamScoreText(int blueTeamScore)
    {
        this.blueTeamScore.SetText(blueTeamScore.ToString("0"));
    }

    public void SetGoalTextInfo(bool activeStatus, TextColorEnum color)
    {
        goalText.enabled = activeStatus;
        goalText.color = TextColor.GetColorFromEnum(color);
    }
}