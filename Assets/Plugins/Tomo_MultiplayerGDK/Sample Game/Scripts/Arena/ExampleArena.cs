using TomoClub.Arenas;
using TomoClub.Core;
using UnityEngine;

namespace TomoClub.SampleGame
{
	/// <summary>
	/// This is an extension to base arena to override any of its functionality
	/// </summary>
	public class ExampleArena : BaseArena
	{
		[SerializeField] Camera m_camera;
		[SerializeField] Canvas m_canvas;

		private ArenaScore arenaScore;
		public ArenaScore ArenaScore => arenaScore;

		private ModeratorCameraMovement moderatorCameraMovement;
		public ModeratorCameraMovement ModeratorCameraMovement => moderatorCameraMovement;

		protected override void Awake()
		{
			base.Awake();
			arenaScore = GetComponent<ArenaScore>();
			moderatorCameraMovement = GetComponentInChildren<ModeratorCameraMovement>();

		}
		protected override void OnTimerUpdate(int currentTime)
		{
			ExampleRemoteProcedureCalls.Instance.UpdateArenaTimerOnNetwork(arenaNo, currentTime);
		}

		protected override void OnTimerCompleted()
		{
			ExampleRemoteProcedureCalls.Instance.UpdateArenaTimerCompletedOnNetwork(arenaNo);
		}

		public override void ActivateArena()
		{
			base.ActivateArena();
			moderatorCameraMovement.UpdateCameraFollow(true && LocalPlayer.Instance.isInGameSpec);
		}

		public override void DeactivateArena()
		{
			base.DeactivateArena();
			moderatorCameraMovement.UpdateCameraFollow(true && LocalPlayer.Instance.isInGameSpec);
			moderatorCameraMovement.UpdateCameraFollow(false);
		}

        protected override void InitializeCameraSystem()
        {
			m_camera.depth = -1;
			m_camera.gameObject.SetActive(false);
        }

        protected override void InitializeCanvasSystem()
        {
			m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			m_canvas.gameObject.SetActive(false);
        }

        protected override void ActivateCamera()
        {
			m_camera.depth = 1;
			m_camera.gameObject.SetActive(true);
		}

        protected override void ActivateCanvas()
        {
			m_canvas.gameObject.SetActive(true);
		}

        protected override void DeactivateCamera()
        {
			m_camera.depth = -1;
			m_camera.gameObject.SetActive(false);
		}

        protected override void DeactivateCanvas()
        {
			m_canvas.gameObject.SetActive(false);
		}
    } 
}

