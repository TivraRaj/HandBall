using TMPro;
using UnityEngine;
namespace TomoClub.SampleGame
{

	public class ArenaScore : MonoBehaviour
	{
		[Header("Game UI")]
		[SerializeField] TextMeshProUGUI redTeamScoreUI;
		[SerializeField] TextMeshProUGUI blueTeamScoreUI;

		public void Awake()
		{
			redTeamScoreUI.text = "Team A: 0";
			blueTeamScoreUI.text = "Team B: 0";
		}

		public void UpdateRedTeamScoreUI(int score)
		{
			redTeamScoreUI.text = redTeamScoreUI.text = "Team A: " + score.ToString();
		}

		public void UpdateBlueTeamScoreUI(int score)
		{
			blueTeamScoreUI.text = blueTeamScoreUI.text = "Team B: " + score.ToString();
		}

	} 
}
