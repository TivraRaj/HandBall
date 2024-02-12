using UnityEngine;
using Photon.Pun;
using System.Collections;
using TomoClub.Core;
using System.Collections.Generic;
using Photon.Realtime;

public class ObjectSpawner : MonoBehaviourPun
{
    [SerializeField] private GameObject redPlayerPrefab;
    [SerializeField] private GameObject bluePlayerPrefab;
    [SerializeField] private GameObject BallPrefab;

    [SerializeField] private Transform[] redPlayerPosition;
    [SerializeField] private Transform[] bluePlayerPosition;
    [SerializeField] private Transform ballPosition;

    private GameManager gameManager;

    private List<Player> RedTeamList;
    private List<Player> BlueTeamList;

    public void Init(int arenaNo)
    {
        RedTeamList = MultiplayerManager.Instance.arenaTeamLists[arenaNo - 1].redTeamPlayers;
        BlueTeamList = MultiplayerManager.Instance.arenaTeamLists[arenaNo - 1].blueTeamPlayers;

        if(PhotonNetwork.IsMasterClient)
        {
            for(int i = 0; i < RedTeamList.Count; i++)
            {
                RemoteProcedureCalls.Instance.SpawnPlayerOnNetwork(RedTeamList[i], arenaNo, redPlayerPrefab.name, redPlayerPosition[i].position);
            }

            for (int i = 0; i < BlueTeamList.Count; i++)
            {
                RemoteProcedureCalls.Instance.SpawnPlayerOnNetwork(BlueTeamList[i], arenaNo, bluePlayerPrefab.name, bluePlayerPosition[i].position);
            }

            SpawnBall(arenaNo);
        }
    }

    public void SetGameManager(GameManager gameManager) => this.gameManager = gameManager;

    public void SpawnPlayer(int arenaNo, string playerName, Vector3 playerPosition)
    {
        object[] data = new object[] { arenaNo };
        PlayerController newPlayer = PhotonNetwork.Instantiate(playerName, playerPosition, Quaternion.identity, 0, data).GetComponent<PlayerController>();
        newPlayer.SetObjectSpawner(this);
    }

    private void SpawnBall(int arenaNo)
    {
        object[] data = new object[]{ arenaNo };
        PhotonNetwork.Instantiate(BallPrefab.name, ballPosition.position, Quaternion.identity, 0, data);
    }

    public void ActivateGoalText(TextColorEnum color)
    {
        RemoteProcedureCalls.Instance.SetGoalTextOnNetwork(gameManager.GetArena().ArenaNo, true, color);
        StartCoroutine(DeactivateGoalText(color));
    }

    private IEnumerator DeactivateGoalText(TextColorEnum color)
    {
        yield return new WaitForSeconds(2f);
        RemoteProcedureCalls.Instance.SetGoalTextOnNetwork(gameManager.GetArena().ArenaNo, false, color);
    }
}