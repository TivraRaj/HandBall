using Photon.Pun;


namespace TomoClub.SampleGame
{
	public class ArtifactsManager : MonoBehaviourPun
	{
		public static ArtifactsManager instance;

		private Artifact[] artifacts;
		
		private void Awake()
		{
			if (instance == null)
				instance = this;
			else
				Destroy(this);


			//Initializing artifact array based on child count
			artifacts = new Artifact[transform.childCount];


			//Populating the artifacts
			for (int i = 0; i < transform.childCount; i++)
			{
				artifacts[i] = transform.GetChild(i).GetComponent<Artifact>();
				artifacts[i].artifactID = i;
			}
		}


		public void ArtifactCollected(int artifactNo, int team, int arenaNo)
		{
			photonView.RPC(nameof(UpdateCollectedArtifactOnNetwork), RpcTarget.AllViaServer, artifactNo, team, arenaNo);
		}

		[PunRPC]
		private void UpdateCollectedArtifactOnNetwork(int artifactNo, int team, int arenaNo)
		{
			artifacts[artifactNo].gameObject.SetActive(false);
			GameplayEvents.UpdateScoreboard?.Invoke(arenaNo, (Teams)team);
		}



	} 
}
