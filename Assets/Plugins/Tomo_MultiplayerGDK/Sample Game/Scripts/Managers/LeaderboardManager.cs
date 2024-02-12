using TMPro;
using UnityEngine;
using Photon.Realtime;
using System.Collections.Generic;
using TomoClub.Core;

namespace TomoClub.SampleGame
{
	public class LeaderboardManager : MonoBehaviour
	{

		[Header("Arena Results")]
		[SerializeField] GameObject[] Panel_ArenaWiseResults;
		[SerializeField] TextMeshProUGUI[] arenaWinnerTeamsTitle;
		[SerializeField] TextMeshProUGUI[] arenaWiseTeamAPlayers;
		[SerializeField] TextMeshProUGUI[] arenaWiseTeamBPlayers;
		[SerializeField] TextMeshProUGUI[] arenaWiseTeamAScores;
		[SerializeField] TextMeshProUGUI[] arenaWiseTeamBScores;
		[SerializeField] GameObject[] arenaWiseTeamACrowns;
		[SerializeField] GameObject[] arenaWiseTeamBCrowns;




		private void Awake()
		{
			ArenaRankings_Init();
		}

		private void OnEnable()
		{
			ScoringEvents.UpdateLeaderBoard += UpdateArenaResults;
		}

		private void OnDisable()
		{
			ScoringEvents.UpdateLeaderBoard -= UpdateArenaResults;

		}


		private void ArenaRankings_Init()
		{
			//Initialize Arena Rankings
			for (int i = 0; i < Panel_ArenaWiseResults.Length; i++)
			{
				Panel_ArenaWiseResults[i].SetActive(i < MultiplayerManager.Instance.occupiedArenas);
			}

		}



		public void CalculateWinnerOnTimerEnd(int arenaNo)
		{
			int teamAScore = ScoreManager.Instance.GetTeamScore(arenaNo, 0);
			int teamBScore = ScoreManager.Instance.GetTeamScore(arenaNo, 1);

			if (teamAScore == teamBScore) UpdateArenaResults(arenaNo, -1);
			else if (teamAScore > teamBScore) UpdateArenaResults(arenaNo, 0);
			else if (teamBScore > teamAScore) UpdateArenaResults(arenaNo, 1);

		}



		private void UpdateArenaResults(int arenaNo, int team)
		{
			//Result
			if (team == 0 || team == 1) arenaWinnerTeamsTitle[arenaNo - 1].text = $"TEAM {(Teams)team} Won!";
			else arenaWinnerTeamsTitle[arenaNo - 1].text = $"It's A Draw";

			//Team Wise Result Data
			arenaWiseTeamACrowns[arenaNo - 1].SetActive(team == (int)Teams.A);
			arenaWiseTeamBCrowns[arenaNo - 1].SetActive(team == (int)Teams.B);
			arenaWiseTeamAPlayers[arenaNo - 1].text = UpdatePlayerNamesOnResultBoard(arenaNo, 0);
			arenaWiseTeamBPlayers[arenaNo - 1].text = UpdatePlayerNamesOnResultBoard(arenaNo, 1);
			arenaWiseTeamAScores[arenaNo - 1].text = ScoreManager.Instance.GetTeamScore(arenaNo, 0).ToString();
			arenaWiseTeamBScores[arenaNo - 1].text = ScoreManager.Instance.GetTeamScore(arenaNo, 1).ToString();

		}

		private string UpdatePlayerNamesOnResultBoard(int arenaNo, int team)
		{
			string winnerPlayerList = "";

			List<Player> players = team == 0 ? MultiplayerManager.Instance.arenaTeamLists[arenaNo - 1].redTeamPlayers : MultiplayerManager.Instance.arenaTeamLists[arenaNo - 1].blueTeamPlayers;

			for (int i = 0; i < players.Count; i++)
			{
				winnerPlayerList += string.IsNullOrEmpty(winnerPlayerList) ? players[i].NickName : $", {players[i].NickName}";
			}

			return winnerPlayerList;

		}






	} 
}
