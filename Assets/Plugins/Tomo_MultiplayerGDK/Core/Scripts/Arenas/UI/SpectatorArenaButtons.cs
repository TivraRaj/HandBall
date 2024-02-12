using UnityEngine.UI;
using UnityEngine;

namespace TomoClub.Arenas
{
	public class SpectatorArenaButtons : MonoBehaviour
	{
		[SerializeField] GameObject buttonHolder;
		[SerializeField] Image spectatorButtonImage;

		public void SetButtonHolder(bool isActive) => buttonHolder.SetActive(isActive);

		public void SetSpecButtonColor(Color color) => spectatorButtonImage.color = color;

	} 
}
