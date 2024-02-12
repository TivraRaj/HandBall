using Photon.Pun;
using UnityEngine;
using TomoClub.Core;
namespace TomoClub.SampleGame
{

	public class Artifact : MonoBehaviour
	{
		public int artifactID;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.CompareTag(Identifiers.Player) && collision.GetComponent<PhotonView>().IsMine)
				ArtifactsManager.instance.ArtifactCollected(artifactID, LocalPlayer.Instance.teamNo, LocalPlayer.Instance.arenaNo);
		}

	} 
}
