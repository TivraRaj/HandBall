using Photon.Pun;
using Photon.Realtime;
using TomoClub.Core;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class RemoteProcedureCalls : MonoBehaviourPun
{
    public static RemoteProcedureCalls Instance;

    [Header("Managers")]
    [SerializeField] ArenaManager arenaManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

    }

    public void StartArenasOnNetwork()
    {
        photonView.RPC(nameof(StartArenasOnClient), RpcTarget.All);
    }

    public void UpdateArenaPauseStateOnNetwork(int arenaNo, int updateState)
    {
        this.photonView.RPC(nameof(UpdateArenaPauseStatusOnClient), RpcTarget.AllViaServer, arenaNo, updateState);
    }

    public void UpdateArenaTimerOnNetwork(int arenaNo, int currentTime)
    {
        this.photonView.RPC(nameof(UpdateArenaTimerOnClient), RpcTarget.All, arenaNo, currentTime);
    }

    public void UpdateArenaTimerCompletedOnNetwork(int arenaNo)
    {
        this.photonView.RPC(nameof(UpdateArenaTimerCompletedOnClient), RpcTarget.AllViaServer, arenaNo);
    }


    [PunRPC]
    public void StartArenasOnClient()
    {
        PersistantUI.Instance.UpdateSplashScreen(false);
        arenaManager.StartArenas();
    }

    /// <summary>
    /// On Arena Timer Completed Logic on this client
    /// </summary>
    /// <param name="arenaNo"></param>
    [PunRPC]
    private void UpdateArenaTimerCompletedOnClient(int arenaNo)
    {
        arenaManager.UpdateArenaToCompletedState(arenaNo);
    }

    /// <summary>
    /// On Update Arena Timer Logic On This Client
    /// </summary>
    [PunRPC]
    private void UpdateArenaTimerOnClient(int arenaNo, int currentTime)
    {
        arenaManager.UpdateArenaTimerOnClient(arenaNo, currentTime);
        
    }

    /// <summary>
    /// On Update Arena Pause Status On This Client
    /// </summary>
    [PunRPC]
    private void UpdateArenaPauseStatusOnClient(int arenaNo, int updateState)
    {
        arenaManager.UpdatePlayerPauseOnClient(arenaNo, updateState);
    }

    #region Gameplay Rpc

    public void SetBallControllerOnGameManager(int arenaNo, BallController ballController)
    {
        arenaManager.GetGameManager(arenaNo).SetBallController(ballController);
    }

    public void SetPlayerControllerOnGameManager(int arenaNo, PlayerController playerController)
    {
        arenaManager.GetGameManager(arenaNo).SetPlayerController(playerController);
    }

    public GameManager GetGameManagerFromNetwork(int arenaNo)
    {
        return arenaManager.GetGameManager(arenaNo);
    }

    #region Game Status
    public void SetGameStatusOnNetwork(int arenaNo, int gameStatus)
    {
        this.photonView.RPC(nameof(SetGameStatusOnClient), RpcTarget.All, arenaNo, gameStatus);
    }

    [PunRPC]
    private void SetGameStatusOnClient(int arenaNo, int gameStatus)
    {
        arenaManager.SetGameStatus(arenaNo, gameStatus);
    }
    #endregion

    #region GameOver UI Status
    public void SetGameOverUIStatusOnNetwork(int arenaNo, bool isActive)
    {
        this.photonView.RPC(nameof(SetGameOverUIStatusOnClient), RpcTarget.All, arenaNo, isActive);
    }

    [PunRPC]
    private void SetGameOverUIStatusOnClient(int arenaNo, bool isActive)
    {
        arenaManager.SetGameOverUIStatus(arenaNo, isActive);
    }
    #endregion

    #region Team Won Value
    public void SetTeamWonValueOnNetwork(int arenaNo, string text, TextColorEnum teamColor)
    {
        this.photonView.RPC(nameof(SetTeamWonValueOnClient), RpcTarget.All, arenaNo, text, teamColor);
    }

    [PunRPC]
    private void SetTeamWonValueOnClient(int arenaNo, string text, TextColorEnum teamColor)
    {
        arenaManager.SetTeamWonValue(arenaNo, text, teamColor);
    }
    #endregion

    #region Goal Text
    public void SetGoalTextOnNetwork(int arenaNo, bool isActive, TextColorEnum color)
    {
        this.photonView.RPC(nameof(SetGoalTextOnClient), RpcTarget.All, arenaNo, isActive, color);
    }

    [PunRPC]
    private void SetGoalTextOnClient(int arenaNo, bool isActive, TextColorEnum color)
    {
        arenaManager.SetGoalText(arenaNo, isActive, color);
    }
    #endregion

    #region Score

    public void IncreaseRedTeamScoreOnNetwork(int arenaNo, int value)
    {
        this.photonView.RPC(nameof(IncreaseRedTeamScoreOnClient), RpcTarget.All, arenaNo, value);
    }

    [PunRPC]
    private void IncreaseRedTeamScoreOnClient(int arenaNo, int value)
    {
        arenaManager.IncreaseRedTeamScore(arenaNo, value);
    }

    public void IncreaseBlueTeamScoreOnNetwork(int arenaNo, int value)
    {
        this.photonView.RPC(nameof(IncreaseBlueTeamScoreOnClient), RpcTarget.All, arenaNo, value);
    }

    [PunRPC]
    private void IncreaseBlueTeamScoreOnClient(int arenaNo, int value)
    {
        arenaManager.IncreaseBlueTeamScore(arenaNo, value);
    }
    #endregion

    #region Spawn Players

    public void SpawnPlayerOnNetwork(Player player, int arenaNo, string playerName, Vector3 spawnPlayerPosition)
    {
        this.photonView.RPC(nameof(SpawnPlayerOnClient), player, arenaNo, playerName, spawnPlayerPosition);
    }

    [PunRPC]
    private void SpawnPlayerOnClient(int arenaNo, string playerName, Vector3 spawnPlayerPosition)
    {
        arenaManager.SpawnPlayer(arenaNo, playerName, spawnPlayerPosition);
    }
    #endregion

    #region Reset player Position
    public void ResetPlayerPositionOnMasterClient(int arenaNo)
    {
        this.photonView.RPC(nameof(ResetPlayerPositionOnNetwork), RpcTarget.MasterClient, arenaNo);
    }

    [PunRPC]
    private void ResetPlayerPositionOnNetwork(int arenaNo)
    {
        for(int i = 0; i < MultiplayerManager.Instance.arenaLists[arenaNo - 1].arenaPlayers.Count; i++)
        {
            this.photonView.RPC(nameof(ResetPlayerPositionOnClient), MultiplayerManager.Instance.arenaLists[arenaNo - 1].arenaPlayers[i], arenaNo);
        }
    }

    [PunRPC]
    private void ResetPlayerPositionOnClient(int arenaNo)
    {
        arenaManager.ResetPlayerPosition(arenaNo);
    }
    #endregion

    #endregion
}