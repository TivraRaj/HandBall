using TomoClub.Arenas;
using UnityEngine;

public class ArenaManager : BaseArenaManager<Arena> 
{
	public Arena GetArena(int arenaNo) => arenas[arenaNo - 1];

	protected override void SpectateGame()
	{
		//Change of camera while spectating game
		base.SpectateGame();
	}

	protected override void UpdateArenaPauseStateOnNetwork(int arenaNo, ArenaState updatedState)
	{
		RemoteProcedureCalls.Instance.UpdateArenaPauseStateOnNetwork(arenaNo, (int)updatedState);
	}

    protected override void OnGameSceneLoaded()
    {
		RemoteProcedureCalls.Instance.StartArenasOnNetwork();
    }

    public GameManager GetGameManager(int arenaNo)
    {
        return GetArena(arenaNo).GameManager;
    }

	public void SetGameStatus(int arenaNo, int status)
	{
        GetGameManager(arenaNo).SetGameStatus((GameStatusEnum)status);
    }

    public void SetGameOverUIStatus(int arenaNo, bool isActive)
    {
        GetGameManager(arenaNo).GetGameUIManager().SetGameOverUIActiveStatus(isActive);
    }

    public void SetTeamWonValue(int arenaNo, string text, TextColorEnum teamColor)
    {
		GetGameManager(arenaNo).GetGameUIManager().SetTeamWonTextInfo(text, teamColor);
    }

    public void SetGoalText(int arenaNo, bool isActive, TextColorEnum color)
    {
        GetGameManager(arenaNo).GetGameUIManager().SetGoalTextInfo(isActive, color);
    }

    public void IncreaseRedTeamScore(int arenaNo, int value)
    {
        GetGameManager(arenaNo).GetScoreController().SetRedTeamScore(value);
        GetGameManager(arenaNo).GetGameUIManager().SetRedTeamScoreText(GetGameManager(arenaNo).GetScoreController().GetRedTeamScore());
    }

    public void IncreaseBlueTeamScore(int arenaNo, int value)
    {
        GetGameManager(arenaNo).GetScoreController().SetBlueTeamScore(value);
        GetGameManager(arenaNo).GetGameUIManager().SetBlueTeamScoreText(GetGameManager(arenaNo).GetScoreController().GetBlueTeamScore());
    }

    public void SpawnPlayer(int arenaNo, string playerName, Vector3 spawnPlayerPosition)
    {
        GetGameManager(arenaNo).GetObjectSpawner().SpawnPlayer(arenaNo, playerName, spawnPlayerPosition);
    }

    public void ResetPlayerPosition(int arenNo)
    {
        GetGameManager(arenNo).GetPlayerController.ResetPlayerPosition();
    }
}
