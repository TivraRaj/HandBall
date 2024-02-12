using Photon.Pun;
using UnityEngine;

public class BallController : MonoBehaviourPunCallbacks, IPunInstantiateMagicCallback
{
    private PhotonView ballPhotonView;

    private void Start()
    {
        ballPhotonView = GetComponent<PhotonView>();
    }

    public PhotonView GetBallPhotonView()
    {
        return ballPhotonView;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(ballPhotonView.IsMine)
        {
            PlayerController collidedobject = collision.gameObject.GetComponent<PlayerController>();

            if (collidedobject != null)
            {
                ballPhotonView.RPC(nameof(UpdateBallStatus), RpcTarget.All, false);

                PhotonView collidedPhotonView = collision.gameObject.GetComponent<PhotonView>();

                if (collidedPhotonView != null)
                {    
                    collidedPhotonView.RPC(nameof(collidedobject.UpdateBallOnPlayer), RpcTarget.All, true);
                }
            }
        }
    }

    [PunRPC]
    public void UpdateBallStatus(bool isActive)
    {   
        this.gameObject.SetActive(isActive);
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        int arenaNo = (int)info.photonView.InstantiationData[0];
        RemoteProcedureCalls.Instance.SetBallControllerOnGameManager(arenaNo, this);
    }
}