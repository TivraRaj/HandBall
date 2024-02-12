using TomoClub.Arenas;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class Arena : BaseArena
{
    public int ArenaNo => arenaNo;
    public List<Player> ArenaPlayers => arenaPlayers;

    [SerializeField] private Camera arenaCamera;
    [SerializeField] private Canvas arenaCanvas;
    [SerializeField] private GameManager gameManager;

    public GameManager GameManager {  get { return gameManager; } }

    #region CAMERA SYSTEM

    protected override void Start()
    {
        base.Start();
        gameManager.SetArena(this);
    }

    protected override void InitializeCameraSystem()
    {
        arenaCamera.gameObject.SetActive(false);
    }

    protected override void ActivateCamera()
    {
        arenaCamera.gameObject.SetActive(true);
    }

    protected override void DeactivateCamera()
    {
        arenaCamera.gameObject.SetActive(false);
    }


    #endregion

    #region CANVAS SYSTEM
    protected override void InitializeCanvasSystem()
    {
        arenaCanvas.gameObject.SetActive(false);
    }

    protected override void ActivateCanvas()
    {
        arenaCanvas.gameObject.SetActive(true);
    }
    protected override void DeactivateCanvas()
    {
        arenaCanvas.gameObject.SetActive(false);
    }

    #endregion

    #region TIMER SYSTEM

    protected override void OnTimerCompleted()
	{
		RemoteProcedureCalls.Instance.UpdateArenaTimerCompletedOnNetwork(arenaNo);
	}

	protected override void OnTimerUpdate(int currentTime)
	{
		RemoteProcedureCalls.Instance.UpdateArenaTimerOnNetwork(arenaNo, currentTime);
	}

    #endregion

    public int ArenaTimeLeft => arenaTimeLeft; 
}