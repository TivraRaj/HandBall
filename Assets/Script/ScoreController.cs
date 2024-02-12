using UnityEngine;

public class ScoreController : MonoBehaviour
{
    private int redTeamScore;
    private int blueTeamScore;

    public void SetRedTeamScore(int value)
    {
        redTeamScore += value;
    }

    public void SetBlueTeamScore(int value)
    {
        blueTeamScore += value;
    }

    public int GetRedTeamScore() => redTeamScore;

    public int GetBlueTeamScore() => blueTeamScore;
}