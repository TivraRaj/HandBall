using UnityEngine;
using Photon.Pun;
using System.IO;
using TomoClub.Core;

namespace TomoClub.SampleGame
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager Instance;

		public Transform[] teamASpawnPoint;
		public Transform[] teamBSpawnPoint;

		[HideInInspector]public Camera playerCamera;

		private void Awake()
		{
			if (Instance == null) Instance = this;
		}


		private void Start()
		{
			if(SessionData.showSplashScreen)
			{
				Invoke(nameof(SpawnPlayer), SessionData.splashScreenTTL);
			}
			
		}

		private void SpawnPlayer()
		{
			if (!LocalPlayer.Instance.isdefaultSpec)
			{
				var playerManager = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerManager),
					Vector3.zero, Quaternion.identity).GetComponent<PlayerManager>();
				playerManager.InstantiatePlayerManager(LocalPlayer.Instance.teamNo);
			}

		}

	} 
}




