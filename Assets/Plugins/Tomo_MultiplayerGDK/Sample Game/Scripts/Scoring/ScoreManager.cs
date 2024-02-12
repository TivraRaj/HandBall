using Photon.Pun;
using UnityEngine;

namespace TomoClub.SampleGame
{
	public class ScoreManager : MonoBehaviour
	{
		public static ScoreManager Instance;

		[SerializeField] ExampleArenaManager arenaManager;

		private int[] perArena_TeamA_Scores = new int[4];
		private int[] perArena_TeamB_Scores = new int[4];


		private void Awake()
		{
			if (Instance == null) Instance = this;

		}


		private void OnEnable()
		{
			GameplayEvents.UpdateScoreboard += UpdateScoreOnNetwork;
		}

		private void OnDisable()
		{
			GameplayEvents.UpdateScoreboard -= UpdateScoreOnNetwork;
		}


		private void UpdateScoreOnNetwork(int arenaNo, Teams team)
		{

			int teamScore = 0;
			switch (team)
			{
				case Teams.A:
					perArena_TeamA_Scores[arenaNo - 1]++;
					teamScore = perArena_TeamA_Scores[arenaNo - 1];
					break;
				case Teams.B:
					perArena_TeamB_Scores[arenaNo - 1]++;
					teamScore = perArena_TeamB_Scores[arenaNo - 1];
					break;
			}
			arenaManager.UpdateArenaScore(arenaNo, (int)team, teamScore);


			if (perArena_TeamA_Scores[arenaNo - 1] == 1 || perArena_TeamB_Scores[arenaNo - 1] == 1)
			{
				//Everyone on server
				ScoringEvents.UpdateLeaderBoard?.Invoke(arenaNo, (int)team);
				arenaManager.UpdateArenaToCompletedState(arenaNo);

			}


		}




		public int GetTeamScore(int arenaNo, int team)
		{
			Teams teamName = (Teams)team;

			switch (teamName)
			{
				case Teams.A: return perArena_TeamA_Scores[arenaNo - 1];
				case Teams.B: return perArena_TeamB_Scores[arenaNo - 1];
				default: return -1;
			}

		}

	} 
}

