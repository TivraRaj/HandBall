using UnityEngine;
using TomoClub.Arenas;

namespace TomoClub.SampleGame
{
	/// <summary>
	/// Override any base arena manager functionality 
	/// </summary>
	public class ExampleArenaManager : BaseArenaManager<ExampleArena>
	{
		[Header("Moderator Camera Limits")]
		[Tooltip("Arena Wise moderator camera limits, use the arenas as reference positions for this")]
		[SerializeField] Vector2[] modCameraLimits;

        protected override void GenerateArenas()
        {
            base.GenerateArenas();
			
			for (int i = 0; i < arenas.Length; i++)
			{
				arenas[i].ModeratorCameraMovement.limits = modCameraLimits[i];
			}
		}

        protected override void SpectateGame()
		{
			if (GameManager.Instance.playerCamera != null) GameManager.Instance.playerCamera.depth = -10;
			base.SpectateGame();
		}

		protected override void UpdateArenaPauseStateOnNetwork(int arenaNo, ArenaState updatedState)
		{
			ExampleRemoteProcedureCalls.Instance.UpdateArenaPauseStateOnNetwork(arenaNo, (int)updatedState);
		}

		public void UpdateArenaScore(int arenaNo, int team, int score)
		{
			switch ((Teams)team)
			{
				case Teams.A:
					arenas[arenaNo - 1].ArenaScore.UpdateRedTeamScoreUI(score);
					break;
				case Teams.B:
					arenas[arenaNo - 1].ArenaScore.UpdateBlueTeamScoreUI(score);
					break;
				default:
					Debug.LogError("<<<<ERROR>>>> Impossible Team");
					break;
			}
		}

        protected override void OnGameSceneLoaded()
        {
			ExampleRemoteProcedureCalls.Instance.StartArenasOnNetwork();
        }

		public ExampleArena GetArena(int arenaNo) => arenas[arenaNo - 1];
    }  
}
