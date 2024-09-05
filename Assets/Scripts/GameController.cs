using UnityEngine;
using System.Collections;
public class GameController : GameBaseController
{
    public static GameController Instance = null;
    public PlayerController[] playerControllers;
    public int playerNumber = 0;

    protected override void Awake()
    {
        if (Instance == null) Instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        this.playerNumber = LoaderConfig.Instance != null ? LoaderConfig.Instance.PlayerNumbers : 2;
        StartCoroutine(this.InitialQuestion());
       
    }

    private IEnumerator InitialQuestion()
    {
        QuestionController.Instance?.nextQuestion();
        string word = QuestionController.Instance.currentQuestion.correctAnswer;

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < this.playerControllers.Length; i++)
        {
            if (this.playerControllers[i] != null)
            {
                if (i < this.playerNumber)
                {
                    if (i == 0 && LoaderConfig.Instance != null && LoaderConfig.Instance.apiManager.peopleIcon != null)
                    {
                        var _playerName = LoaderConfig.Instance?.apiManager.loginName;
                        var icon = SetUI.ConvertTextureToSprite(LoaderConfig.Instance.apiManager.peopleIcon as Texture2D);
                        this.playerControllers[i].updatePlayerIcon(true, _playerName, icon);
                    }

                    this.playerControllers[i].gameObject.SetActive(true);
                    this.playerControllers[i].Init(word);
                }
                else
                {
                    this.playerControllers[i].gameObject.SetActive(false);
                    this.playerControllers[i].updatePlayerIcon(false);
                }
            }
        }
    }


    public override void enterGame()
    {
        base.enterGame();
    }

    public override void endGame()
    {
        bool showSuccess = false;
        for (int i = 0; i < this.playerControllers.Length; i++)
        {
            var playerController = this.playerControllers[i];
            if (playerController != null)
            {
                if (playerController.Score >= 30)
                {
                    showSuccess = true;
                }
                this.endGamePage.updateFinalScore(i, playerController.Score);
            }
        }
        this.endGamePage.setStatus(true, showSuccess);

        base.endGame();
    }

    public void UpdateNextQuestion()
    {
        LogController.Instance?.debug("Next Question");
        QuestionController.Instance?.nextQuestion();
        string word = QuestionController.Instance.currentQuestion.correctAnswer;

        for (int i = 0; i < this.playerNumber; i++)
        {
            if (this.playerControllers[i] != null)
            {
                this.playerControllers[i].NewQuestionWord(word);
            }
        }
    }

   

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.UpdateNextQuestion();
        }

        // Handle mouse input
        if (Input.GetMouseButtonUp(0))
        {
            for (int i = 0; i < this.playerControllers.Length; i++)
            {
                if (this.playerControllers[i] != null && this.playerControllers[i].IsConnectWord)
                {
                    this.playerControllers[i].StopConnection();
                }
            }
        }

        // Handle touch input
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                PlayerController player = this.GetPlayerByTouchIndex(i);

                if (player != null)
                {
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            player.StartConnection();
                            HandleTouch(touch.position, player);
                            break;
                        case TouchPhase.Moved:
                            if (player.IsConnectWord)
                            {
                                HandleTouch(touch.position, player);
                            }
                            break;
                        case TouchPhase.Ended:
                            player.StopConnection();
                            break;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            for (int i = 0; i < this.playerControllers.Length; i++)
            {
                if (this.playerControllers[i] != null && this.playerControllers[i].IsConnectWord)
                {
                    HandleMouse(this.playerControllers[i]);
                }
            }
        }
    }

    private PlayerController GetPlayerByTouchIndex(int touchIndex)
    {
        // Map touch index to player index (assuming two players)
        if (touchIndex < playerControllers.Length)
        {
            return playerControllers[touchIndex];
        }
        return null;
    }

    private void HandleMouse(PlayerController player)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        CheckCell(ray, player);
    }

    private void HandleTouch(Vector2 touchPosition, PlayerController player)
    {
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        CheckCell(ray, player);
    }

    private void CheckCell(Ray ray, PlayerController player)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Cell cell = hit.collider.GetComponent<Cell>();
            if (cell != null && !cell.isSelected)
            {
                player.SelectCell(cell);
            }
        }
    }
}
