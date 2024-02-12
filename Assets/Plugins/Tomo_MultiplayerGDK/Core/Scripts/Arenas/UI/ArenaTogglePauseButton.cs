using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace TomoClub.Arenas
{
	public class ArenaTogglePauseButton : MonoBehaviour
	{
		[SerializeField] Button arenaTogglePauseButton;
		[SerializeField] Image arenaToggePauseImage;
		[SerializeField] GameObject buttonHolder;
		[SerializeField] TextMeshProUGUI buttonText;

		private readonly string Pause = "Pause";
		private readonly string Play = "Play";
			

        private void OnEnable()
        {
			if (buttonText != null)
				SetButtonText(false);
        }

        public void SetButtonState(bool isActive) => arenaTogglePauseButton.interactable = isActive;
		public void SetSprite(Sprite sprite)
		{
			arenaToggePauseImage.sprite = sprite;
		}

		public void SetButtonText(bool isPause)
		{
			if (buttonText != null)
				buttonText.text = isPause ? Play : Pause;
		}



		public void SetButtonHolder(bool isActive) => buttonHolder.SetActive(isActive);

	} 
}
