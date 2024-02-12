using Photon.Pun;
using UnityEngine;

namespace TomoClub.SampleGame
{
	public class ExampleRemoteProcedureCalls : MonoBehaviourPun
	{
		public static ExampleRemoteProcedureCalls Instance;

		[Header("Managers")]
		[SerializeField] ExampleArenaManager arenaManager;
		[SerializeField] LeaderboardManager leaderboardManager;

		private void Awake()
		{
			if (Instance == null)
				Instance = this;
		}


		public ExampleArena GetArena(int arenaNo) => arenaManager.GetArena(arenaNo);

		public void StartArenasOnNetwork()
		{
			photonView.RPC(nameof(StartArenasOnClient), RpcTarget.All);
		}

		public void UpdateArenaPauseStateOnNetwork(int arenaNo, int updateState)
		{
			photonView.RPC(nameof(UpdateArenaPauseStatusOnClient), RpcTarget.AllViaServer, arenaNo, updateState);
		}

		public void UpdateArenaTimerOnNetwork(int arenaNo, int currentTime)
		{
			photonView.RPC(nameof(UpdateArenaTimerOnClient), RpcTarget.All, arenaNo, currentTime);
		}

		public void UpdateArenaTimerCompletedOnNetwork(int arenaNo)
		{
			photonView.RPC(nameof(UpdateArenaTimerCompletedOnClient), RpcTarget.AllViaServer, arenaNo);
		}

		[PunRPC]
		private void StartArenasOnClient()
		{
			PersistantUI.Instance.UpdateSplashScreen(false);
			arenaManager.StartArenas();
		}


		[PunRPC]
		private void UpdateArenaTimerCompletedOnClient(int arenaNo)
		{
			leaderboardManager.CalculateWinnerOnTimerEnd(arenaNo);
			arenaManager.UpdateArenaToCompletedState(arenaNo);
		}

        [PunRPC]
		private void UpdateArenaTimerOnClient(int arenaNo, int currentTime)
		{
			arenaManager.UpdateArenaTimerOnClient(arenaNo, currentTime);
		}

		[PunRPC]
		private void UpdateArenaPauseStatusOnClient(int arenaNo, int updateState)
		{
			arenaManager.UpdatePlayerPauseOnClient(arenaNo, updateState);
		}

	} 
}
