using UnityEngine;
using TomoClub.Core;
using Photon.Pun;
using System;
using DG.Tweening;

namespace TomoClub.Arenas
{
    public class ArenaUIManager : MonoBehaviour
	{ 

		public static ArenaUIManager Instance;

		[Header("Control Panel")]
		[SerializeField] RectTransform Panel_Controls;
		[SerializeField] GameObject Button_EnableControls;
		[SerializeField] Vector2 ControlPanel_SwingRange;

		[Header("Notes")]
		[SerializeField] GameObject Panel_Notes;
		[SerializeField] GameObject Button_Notes;

		[Header("Exit")]
		[SerializeField] private GameObject Button_Restart;
		[SerializeField] private GameObject Button_Lobby;

		[Header("Play/Pause UI")]
		[SerializeField] GameObject Panel_Pause;
		[SerializeField] GameObject PlayPauseButtonHolder;
		[SerializeField] GameObject PlayPauseDropdown;
		[SerializeField] GameObject Popup_PlayPauseArenas;
		[SerializeField] ArenaTogglePauseButton allArenasTogglePauseButton;
		[SerializeField] Sprite[] playPauseSprites;
		[SerializeField] ArenaTogglePauseButton[] arenaTogglePauseButtons;

		[Header("Spectator Tools")]
		[SerializeField] GameObject Panel_Spectator;
		[SerializeField] GameObject Spectator_UI;
		[SerializeField] GameObject Panel_WaitingForResultCompilation;
		[SerializeField] SpectatorArenaButtons[] Buttons_Spectator;

		[Header("End Panel")]
		public GameObject Panel_EndPanel;
		public GameObject Panel_EndMaster;
		public GameObject Panel_EndNormal;


		private void Awake()
		{
			if (Instance == null)
				Instance = this;
		}



		public void Init()
        {
			//Reset Controls Panel Position
			Panel_Controls.anchoredPosition = new Vector2(ControlPanel_SwingRange.y, Panel_Controls.anchoredPosition.y);

			//Pause Panel Init
			for (int i = 0; i < arenaTogglePauseButtons.Length; i++)
			{
				arenaTogglePauseButtons[i].SetSprite(playPauseSprites[0]);
				arenaTogglePauseButtons[i].SetButtonState(i < MultiplayerManager.Instance.occupiedArenas);
				arenaTogglePauseButtons[i].SetButtonHolder(i < MultiplayerManager.Instance.occupiedArenas);
			}

			allArenasTogglePauseButton.SetSprite(playPauseSprites[0]);

			Panel_Pause.SetActive(false);
			Popup_PlayPauseArenas.SetActive(false);
			PlayPauseDropdown.SetActive(MultiplayerManager.Instance.occupiedArenas > 1);
			PlayPauseButtonHolder.SetActive(false);

			//Spectator Panel Init
			for (int i = 0; i < Buttons_Spectator.Length; i++)
			{
				Buttons_Spectator[i].SetSpecButtonColor(Color.red);
				Buttons_Spectator[i].SetButtonHolder(i < MultiplayerManager.Instance.occupiedArenas);
			}

			Panel_WaitingForResultCompilation.SetActive(false);
			Spectator_UI.SetActive(false);
			Panel_Spectator.SetActive(false);

			//Notes Panel
			Panel_Notes.SetActive(false);
			Button_Notes.SetActive(LocalPlayer.Instance.isdefaultSpec);

			//Exit Buttons
			Button_Restart.SetActive(LocalPlayer.Instance.isRoomOwner);
			Button_Lobby.SetActive(LocalPlayer.Instance.isRoomOwner);

			//End Panel
			Panel_EndMaster.SetActive(false);
			Panel_EndNormal.SetActive(false);
			Panel_EndPanel.SetActive(false);

			Button_EnableControls.SetActive(true);
		}

		public void UpdatePanelControls(bool status)
        {
			if(status)
            {	
				Panel_Controls.DOAnchorPosX(ControlPanel_SwingRange.x, 0.4f).SetEase(Ease.OutExpo);
            }
			else
            {
				Panel_Controls.DOAnchorPosX(ControlPanel_SwingRange.y, 0.4f).
					SetEase(Ease.OutExpo);
			}

			Button_EnableControls.SetActive(!status);
		}


		public void UpdateNotesMenu(bool status) => Panel_Notes.SetActive(status);

		public void TogglePausePlayDropdown() => Popup_PlayPauseArenas.SetActive(!Popup_PlayPauseArenas.activeSelf);

		public void UpdatePauseButtonSprite(bool arenasPaused, int arenaNo, int updatedState)
        {
			//Update Images Here (// State 0-> pause state -> sprite should be play sprite, State 1-> play state -> sprite should be pause sprite)
			allArenasTogglePauseButton.SetSprite(arenasPaused ? playPauseSprites[1] : playPauseSprites[0]);
			allArenasTogglePauseButton.SetButtonText(arenasPaused);
			arenaTogglePauseButtons[arenaNo - 1].SetSprite(playPauseSprites[1 - updatedState]);
		}

		public void SetSpectorButtonColor(int arenaNo, Color color)
        {
			Buttons_Spectator[arenaNo - 1].SetSpecButtonColor(color);
		}

		public void UpdatePauseButtonState(int arenaNo, bool state)
        {
			arenaTogglePauseButtons[arenaNo - 1].SetButtonState(state);
		}

		public void UpdateWaitForPanelCompletionState(bool state) => Panel_WaitingForResultCompilation.SetActive(state);

		public void UpdatePausePanelState(bool state) => Panel_Pause.SetActive(state);

		public void UpdateSpectatorPanelState(bool state) => Panel_Spectator.SetActive(state);

		public void UpdateSpectatorUIState(bool state) => Spectator_UI.SetActive(state);

		public void UpdatePausePlayHolderState(bool state) => PlayPauseButtonHolder.SetActive(state);




 
    }



}

