using UnityEngine;
using Photon.Pun;
using TMPro;
using TomoClub.Core;

public class PlayerController : MonoBehaviour, IPunInstantiateMagicCallback
{
    private PhotonView photonView;
    private ObjectSpawner spawner;
    private Rigidbody2D rb;
    private GameManager gameManager;

    [SerializeField] private GameObject spriteBody;
    [SerializeField] private GameObject Ball;  
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private GameObject playerIndicator;

    private bool facingRight;
    private Vector3 playerPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
        Ball.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerName.text = photonView.IsMine?PhotonNetwork.LocalPlayer.NickName : photonView.Owner.NickName;
        playerIndicator.SetActive(photonView.IsMine);
        if (photonView.IsMine)
        {
            playerPosition = this.gameObject.transform.position;
        }
    }

    void Update()
    {
        if(photonView.IsMine)
        {
            if (GameStatus.gameStatus == GameStatusEnum.GameOver || GameStatus.gameStatus == GameStatusEnum.GamePaused) 
            {
                rb.velocity = Vector3.zero;
                return; 
            }

            Movement();
        }
    }

    private void Movement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");     

        Vector2 movement = new Vector2(horizontalInput, verticalInput);
        movement.Normalize();

        rb.velocity = new Vector2(movement.x * moveSpeed, movement.y * moveSpeed);

        Vector3 scale = spriteBody.transform.localScale;
        if (facingRight && horizontalInput < 0)
        {
            scale.x = -1f * Mathf.Abs(scale.x);
            facingRight = false;
        }
        else if (!facingRight && horizontalInput > 0)
        {
            scale.x = Mathf.Abs(scale.x);
            facingRight = true;
        }
        spriteBody.transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine && Ball.gameObject.activeSelf)
        {
            if (collision.gameObject.GetComponent<PlayerController>() != null)
            {
                photonView.RPC(nameof(UpdateBallOnPlayer), RpcTarget.All, false);

                PhotonView collidedPhotonView = collision.gameObject.GetComponent<PhotonView>();

                if (collidedPhotonView != null)
                {
                    collidedPhotonView.RPC(nameof(UpdateBallOnPlayer), RpcTarget.All, true);
                }
            }

            if (collision.gameObject.CompareTag("Goal"))
            {
                photonView.RPC(nameof(UpdateBallOnPlayer), RpcTarget.All, false);    

                gameManager.GetBallController.GetBallPhotonView().RPC(nameof(gameManager.GetBallController.UpdateBallStatus), RpcTarget.All, true);


                if(LocalPlayer.Instance.teamName == TeamName.Red && collision.gameObject.GetComponent<GoalController>().GoalId == GoalPost.BlueTeamGoalPost)
                {
                    ResetPosition(gameManager.GetArena().ArenaNo);
                    spawner.ActivateGoalText(TextColorEnum.Red);
                    RemoteProcedureCalls.Instance.IncreaseRedTeamScoreOnNetwork(gameManager.GetArena().ArenaNo, 1);
                }
                else if (LocalPlayer.Instance.teamName == TeamName.Purple && collision.gameObject.GetComponent<GoalController>().GoalId == GoalPost.RedTeamGoalPost)
                {
                    ResetPosition(gameManager.GetArena().ArenaNo);
                    spawner.ActivateGoalText(TextColorEnum.Blue);
                    RemoteProcedureCalls.Instance.IncreaseBlueTeamScoreOnNetwork(gameManager.GetArena().ArenaNo, 1);
                }
            }
        }
    }

    [PunRPC]
    public void UpdateBallOnPlayer(bool isActive)
    {
        Ball.gameObject.SetActive(isActive);
    }

    public void SetObjectSpawner(ObjectSpawner spawner)
    {
        this.spawner = spawner;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        int arenaNo = (int)info.photonView.InstantiationData[0];
        gameManager =  RemoteProcedureCalls.Instance.GetGameManagerFromNetwork(arenaNo);
        if(photonView.IsMine)
        {
            RemoteProcedureCalls.Instance.SetPlayerControllerOnGameManager(arenaNo, this);
        }
    }

    public void ResetPlayerPosition()
    {
        this.gameObject.transform.position = playerPosition; 
    }

    public void ResetPosition(int arenaNo)
    {
        RemoteProcedureCalls.Instance.SetGameStatusOnNetwork(arenaNo, (int)GameStatusEnum.GamePaused);
        RemoteProcedureCalls.Instance.ResetPlayerPositionOnMasterClient(arenaNo);
        Invoke(nameof(SetGameStatustInProgess), 2f);
    }

    private void SetGameStatustInProgess()
    {
        RemoteProcedureCalls.Instance.SetGameStatusOnNetwork(gameManager.GetArena().ArenaNo, (int)GameStatusEnum.GameInprogress);
    }
}