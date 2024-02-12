using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameUIManager gameUIManager;
    [SerializeField] private ScoreController scoreController;
    [SerializeField] private ObjectSpawner objectSpawner;

    private PlayerController localPlayerController;
    private BallController ballController;
    private Arena arena;

    private void Start()
    {
        objectSpawner.SetGameManager(this);
        objectSpawner.Init(arena.ArenaNo);
    }

    private void Update() => GameOver();

    public void SetArena(Arena arena) => this.arena = arena;

    public GameUIManager GetGameUIManager() => gameUIManager;

    public ScoreController GetScoreController() => scoreController;

    public ObjectSpawner GetObjectSpawner() => objectSpawner;

    public PlayerController GetPlayerController => localPlayerController;

    public BallController GetBallController => ballController;

    public Arena GetArena() => arena;

    private void GameOver()
    {
        if (arena.ArenaTimeLeft <= 0)
        {
            RemoteProcedureCalls.Instance.SetGameStatusOnNetwork(arena.ArenaNo, (int)GameStatusEnum.GameOver);

            RemoteProcedureCalls.Instance.SetGameOverUIStatusOnNetwork(arena.ArenaNo, true);

            if (GetScoreController().GetRedTeamScore() > GetScoreController().GetBlueTeamScore())
            {
                RemoteProcedureCalls.Instance.SetTeamWonValueOnNetwork(arena.ArenaNo, "Red Team Won", TextColorEnum.Red);
            }
            else if (GetScoreController().GetRedTeamScore() < GetScoreController().GetBlueTeamScore())
            {
                RemoteProcedureCalls.Instance.SetTeamWonValueOnNetwork(arena.ArenaNo, "Blue Team Won", TextColorEnum.Blue);
            }
            else
            {
                RemoteProcedureCalls.Instance.SetTeamWonValueOnNetwork(arena.ArenaNo, "Game Draw", TextColorEnum.Green);
            }
        }
    }

    public void SetGameStatus(GameStatusEnum status)
    {
        GameStatus.gameStatus = status;
    }

    public void SetBallController(BallController ballController)
    {
        this.ballController = ballController;
    }

    public void SetPlayerController(PlayerController playerController)
    {
        this.localPlayerController = playerController;
    }

}