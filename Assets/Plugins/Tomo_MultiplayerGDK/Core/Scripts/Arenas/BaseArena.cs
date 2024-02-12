using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;

using TomoClub.Core;
using TomoClub.Util;
using System;

namespace TomoClub.Arenas
{

	public abstract class BaseArena : MonoBehaviour
	{
		[SerializeField] protected ArenaUIController uiController;

		protected int arenaNo;
		protected List<Player> arenaPlayers => MultiplayerManager.Instance.arenaLists[arenaNo - 1].arenaPlayers;

		protected ArenaState arenaState = ArenaState.Paused;
		public ArenaState ArenaState => arenaState;

		protected int arenaTimeLeft;
		protected CounterDown arenaTimer = new CounterDown();

		public Action<bool> OnGameplayPaused;
		public Action OnGameplayEnd;

		protected virtual void Awake()
		{
			//Empty but Arena can use 
		}

		protected virtual void Start()
		{
			//Empty but Arena can use 
		}

		protected virtual void OnEnable()
		{
			arenaTimer.CounterCompleted += OnTimerCompleted;
			arenaTimer.CounterUpdatePerSecond += OnTimerUpdate;
		}

		protected virtual void OnDisable()
		{
			arenaTimer.CounterCompleted -= OnTimerCompleted;
			arenaTimer.CounterUpdatePerSecond -= OnTimerUpdate;
		}

		/// <summary>
		/// This methods run every time the arena updates
		/// </summary>
		protected abstract void OnTimerUpdate(int currentTime);

		/// <summary>
		/// This method run every time the arena timer completes
		/// </summary>
		protected abstract void OnTimerCompleted();

		protected virtual void Update()
		{
			// Run this arena's timer only on the masterClient
			if (LocalPlayer.Instance.isMasterClient) arenaTimer.UpdateCounter();
		}

		protected abstract void InitializeCameraSystem();
		protected abstract void InitializeCanvasSystem();

		public virtual void InitializeArena(int arenaNo, int sessionGameTime)
		{

			//Set This Arena's data
			this.arenaNo = arenaNo;
			uiController.arenaDataText.text = $"Arena {arenaNo}: {arenaState}";
			uiController.arenaPlayersText.text = "";
			for (int i = 0; i < arenaPlayers.Count; i++)
			{
				uiController.arenaPlayersText.text += string.IsNullOrEmpty(uiController.arenaPlayersText.text) ? arenaPlayers[i].NickName : $", {arenaPlayers[i].NickName}";
			}

			//Set Arena Timer
			UpdateTimerData(sessionGameTime);
			arenaTimer.SetCounter(sessionGameTime);

			uiController.arenaCanvas.gameObject.SetActive(false);
			InitializeCameraSystem();
			InitializeCanvasSystem();

		}

		public virtual void UpdateTimerData(int currentTime)
		{
			arenaTimeLeft = currentTime;
			uiController.arenaTimerText.text = Utilities.CovertTimeToString(arenaTimeLeft);
		}

		/// <summary>
		/// Start this arena
		/// </summary>
		public virtual void StartArena()
		{
			if (LocalPlayer.Instance.isMasterClient) arenaTimer.StartCounter();
			UpdateArenaStateData(ArenaState.Running);
		}

		public virtual void PauseTimer() => arenaTimer.PauseCounter();

		public virtual void PlayTimer() => arenaTimer.PlayCounter();

		public virtual void StopTimer()
		{
			if (arenaTimer.IsRunning()) arenaTimer.PauseCounter();
		}

		public virtual void ContinueTimer() => arenaTimer.SetAndStartCounter(arenaTimeLeft);

		protected abstract void ActivateCamera();
		protected abstract void ActivateCanvas();

		/// <summary>
		/// Assign the player this arena Camera
		/// </summary>
		public virtual void ActivateArena()
		{
			uiController.arenaCanvas.gameObject.SetActive(true);
			ActivateCamera();
			ActivateCanvas();

		}

		protected abstract void DeactivateCamera();
		protected abstract void DeactivateCanvas();


		/// <summary>
		/// Deassign this arena camera for this player
		/// </summary>
		public virtual void DeactivateArena()
		{
			uiController.arenaCanvas.gameObject.SetActive(false);
			DeactivateCanvas();
            DeactivateCamera();
		}

		public virtual void UpdateArenaStateData(ArenaState arenaState)
		{
			this.arenaState = arenaState;
			uiController.arenaDataText.text = $"Arena {arenaNo}: {arenaState}";

			switch (arenaState)
			{
				case ArenaState.Paused:
					OnGameplayPaused?.Invoke(true);
					break;

				case ArenaState.Running:
					OnGameplayPaused?.Invoke(false);
					break;

				case ArenaState.Completed:
					OnGameplayEnd?.Invoke();
					break;

				default:
					break;
			}
		}


        } 
}
