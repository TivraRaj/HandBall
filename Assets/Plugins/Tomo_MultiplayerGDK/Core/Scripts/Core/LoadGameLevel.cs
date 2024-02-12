using UnityEngine;


namespace TomoClub.Core
{
	public class LoadGameLevel : MonoBehaviour
	{
		private void Start()
		{
			Invoke(nameof(RestartGame), SessionData.splashScreenTTL / 2f);
		}

		private void RestartGame()
		{
			if (LocalPlayer.Instance.isMasterClient)
				Photon.Pun.PhotonNetwork.LoadLevel(Constants.GameScene);
		}
	}

}