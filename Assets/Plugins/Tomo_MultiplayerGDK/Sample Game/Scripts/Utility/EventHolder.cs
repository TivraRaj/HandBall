using System;
namespace TomoClub.SampleGame
{

	public static class GameplayEvents
	{
		public static Action<int, Teams> UpdateScoreboard;
	}

	public static class ScoringEvents
	{
		public static Action<int, int> UpdateLeaderBoard;
	} 
}
