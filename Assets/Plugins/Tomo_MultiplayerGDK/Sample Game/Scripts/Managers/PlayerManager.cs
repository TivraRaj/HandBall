using UnityEngine;
using Photon.Pun;
using System.IO;
using TomoClub.Core;

namespace TomoClub.SampleGame
{
	public enum Teams { A, B };

	public class PlayerManager : MonoBehaviour
	{
		private PhotonView photonView;
		private Camera myPlayerCamera;

		public Teams myTeam { get; private set; }

		private void Awake()
		{
			photonView = GetComponent<PhotonView>();
		}

		public void InstantiatePlayerManager(int teamNo)
		{
			if (!photonView.IsMine) return;

			//Set Players Team
			myTeam = (Teams)teamNo;
			Vector3 spawnPoint = AssignSpawnPoint();

			//Instantiate the camera on the network
			var playerCamera = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerCamera),
				new Vector3(spawnPoint.x, spawnPoint.y, -10f), Quaternion.identity);

			//Instantiate the player on the network at given spawnpoint
			var playerController = PhotonNetwork.Instantiate(Path.Combine(Identifiers.PhotonPrefabPath, Identifiers.PlayerController),
				spawnPoint, Quaternion.identity);

			//Give the playerManager reference to the PlayerController
			playerController.GetComponent<PlayerController>().SetPlayerManager(this);

			//Set camera for playerController
			playerCamera.GetComponent<FollowCam>().SetPlayerToFollow(playerController.transform);
			GameManager.Instance.playerCamera = playerCamera.GetComponent<Camera>();

		}

		private Vector3 AssignSpawnPoint()
		{
			switch (myTeam)
			{
				case Teams.A:
					return GameManager.Instance.teamASpawnPoint[LocalPlayer.Instance.arenaNo - 1].position + Vector3.up * Random.Range(-6, 6);
				case Teams.B:
					return GameManager.Instance.teamBSpawnPoint[LocalPlayer.Instance.arenaNo - 1].position + Vector3.up * Random.Range(-6, 6);
				default: return Vector3.zero;
			}

		}


	} 
}


