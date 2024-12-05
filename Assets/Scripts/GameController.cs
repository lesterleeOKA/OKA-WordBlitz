using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GameController : GameBaseController
{
    public static GameController Instance = null;
    public GameObject playerPanelPrefab;
    public Transform parent;
    public Color[] playersColor;
    public Sprite[] defaultAnswerBox, defaultFrames;
    public List<PlayerController> playerControllers = new List<PlayerController>();
    private bool showWordHints = false;
    public GridWordFormat gridWordFormat = GridWordFormat.AllUpper;

    protected override void Awake()
    {
        if (Instance == null) Instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    private IEnumerator InitialQuestion()
    {
        QuestionController.Instance?.nextQuestion();
        string word = QuestionController.Instance.currentQuestion.correctAnswer;

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < this.maxPlayers; i++)
        {
            if(i< this.playerNumber)
            {
                var playerController = GameObject.Instantiate(this.playerPanelPrefab, this.parent).GetComponent<PlayerController>();
                playerController.gameObject.name = "Player" + i + "_Panel";
                playerController.UserId = i;
                this.playerControllers.Add(playerController);

                var loader = LoaderConfig.Instance;
                if (loader != null)
                {
                    this.gridWordFormat = loader.gameSetup.gridWordFormat;
                    this.playerControllers[i].Init(word, this.defaultAnswerBox, this.defaultFrames, this.gridWordFormat);

                    if (loader.apiManager.peopleIcon != null && loader.apiManager.IsLogined && i==0)
                    {
                        var _playerName = loader.apiManager.loginName;
                        var icon = SetUI.ConvertTextureToSprite(loader.apiManager.peopleIcon as Texture2D);
                        this.playerControllers[i].UserName = _playerName;
                        this.playerControllers[i].updatePlayerIcon(true, _playerName, icon, this.playersColor[i]);
                    }
                    else
                    {
                        this.playerControllers[i].updatePlayerIcon(true, null, null, this.playersColor[i]);
                    }
                }
            }
            else
            {
                //this.playerControllers[i].updatePlayerIcon(false);
                int notUsedId = i + 1;
                var notUsedPlayerIcon = GameObject.FindGameObjectWithTag("P" + notUsedId + "_Icon");
                if(notUsedPlayerIcon != null) notUsedPlayerIcon.SetActive(false);
            }      
        }
    }


    public override void enterGame()
    {
        base.enterGame();
        StartCoroutine(this.InitialQuestion());
    }

    public override void endGame()
    {
        bool showSuccess = false;
        for (int i = 0; i < this.playerControllers.Count; i++)
        {
            if(i < this.playerNumber)
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
                this.playerControllers[i].NewQuestionWord(word, this.gridWordFormat);
            }
        }
    }

   

    private void Update()
    {
        if(!this.playing) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.UpdateNextQuestion();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            this.showWordHints = !this.showWordHints;
            for (int i = 0; i < this.playerControllers.Count; i++)
            {
                this.playerControllers[i].gridManager.showQuestionWordPosition = this.showWordHints;
                this.playerControllers[i].gridManager.setLetterHint(this.showWordHints, Color.green);
            }
        }

        // Handle mouse input
        if (Input.GetMouseButtonUp(0))
        {
            for (int i = 0; i < this.playerControllers.Count; i++)
            {
                if (this.playerControllers[i] != null && this.playerControllers[i].IsConnectWord)
                {
                    int currentTime = Mathf.FloorToInt(((this.gameTimer.gameDuration - this.gameTimer.currentTime) / this.gameTimer.gameDuration) * 100);
                    this.playerControllers[i].StopConnection(currentTime);
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
                            int currentTime = Mathf.FloorToInt(((this.gameTimer.gameDuration - this.gameTimer.currentTime) / this.gameTimer.gameDuration) * 100);
                            player.StopConnection(currentTime);
                            break;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            for (int i = 0; i < this.playerControllers.Count; i++)
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
        if (touchIndex < playerControllers.Count)
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
