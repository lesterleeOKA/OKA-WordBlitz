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
    public bool hasCheckedAnswers = false;
    public bool checkBattleIdling = false;
    public float idlingCounts = 10f;
    public float count = 0f;
    public AnswerStatus answerStatus = AnswerStatus.waiting;

    protected override void Awake()
    {
        if (Instance == null) Instance = this;
        base.Awake();
        this.resetIdling();
    }

    protected override void Start()
    {
        base.Start();
    }

    public enum AnswerStatus
    {
        waiting,
        bothAnswered,
        oneCorrected,
        bothWrong,
        finished
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
                    this.playerControllers[i].lineDrawer.Init(this.playersColor[i]);
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

        this.handleBattleCheckCase();
    }


    private IEnumerator DelayedNextQuestion(float delay)
    {
        this.answerStatus = AnswerStatus.waiting;
        yield return new WaitForSeconds(delay);
        this.UpdateNextQuestion();
    }

    void handleBattleCheckCase()
    {
        if (this.playerNumber > 1) // Battle Mode
        {
            switch (this.answerStatus)
            {
                case AnswerStatus.waiting:
                    bool bothAnswered = true;
                    for (int i = 0; i < this.playerControllers.Count; i++)
                    {
                        if (this.playerControllers[i] == null || !this.playerControllers[i].IsAnswered)
                        {
                            bothAnswered = false;
                            break;
                        }
                    }
                    if (bothAnswered)
                    {
                        int answersChecked = 0; // To track how many answers have been checked
                        for (int i = 0; i < this.playerControllers.Count; i++)
                        {
                            int currentTime = Mathf.FloorToInt(((this.gameTimer.gameDuration - this.gameTimer.currentTime) / this.gameTimer.gameDuration) * 100);
                            this.playerControllers[i].checkAnswer(currentTime, () =>
                            {
                                answersChecked++;
                                if (answersChecked == this.playerControllers.Count)
                                {
                                    this.answerStatus = AnswerStatus.bothAnswered; // Move to next status
                                }
                            });
                        }
                    }
                    break;
                case AnswerStatus.bothAnswered:
                    bool anyPlayerCorrect = false;
                    for (int i = 0; i < this.playerControllers.Count; i++)
                    {
                        if (this.playerControllers[i] != null && this.playerControllers[i].IsCorrect)
                        {
                            anyPlayerCorrect = true;
                            break; 
                        }
                    }
                    if (anyPlayerCorrect)
                    {
                        this.answerStatus = AnswerStatus.finished;
                    }
                    else
                    {
                        this.answerStatus = AnswerStatus.bothWrong;
                    }
                    break;
                case AnswerStatus.bothWrong:
                    bool anyPlayerFinishedRetry = false;
                    for (int i = 0; i < this.playerControllers.Count; i++)
                    {
                        if (this.playerControllers[i] != null && this.playerControllers[i].Retry == 0)
                        {
                            anyPlayerFinishedRetry = true;
                            break; 
                        }
                    }

                    if (anyPlayerFinishedRetry)
                    {
                        this.answerStatus = AnswerStatus.finished;
                    }
                    else
                    {
                        this.answerStatus = AnswerStatus.waiting;
                    }
                    break;
                case AnswerStatus.finished:
                    StartCoroutine(this.DelayedNextQuestion(2.5f));
                    break;

            }

            
           
            if (this.checkBattleIdling)
            {
                if (this.count > 0f)
                {
                    for (int i = 0; i < this.playerControllers.Count; i++)
                    {
                        if (this.playerControllers[i] != null && this.playerControllers[i].IsAnswered)
                        {
                            this.playerControllers[i].setCountDown(this.count);
                        }
                    }
                    this.count -= Time.deltaTime;
                }
                else
                {
                    this.hasCheckedAnswers = true;
                    int answersChecked = 0; // To track how many answers have been checked
                    for (int i = 0; i < this.playerControllers.Count; i++)
                    {
                        int currentTime = Mathf.FloorToInt(((this.gameTimer.gameDuration - this.gameTimer.currentTime) / this.gameTimer.gameDuration) * 100);
                        this.playerControllers[i].checkAnswer(currentTime, () =>
                        {
                            answersChecked++;
                            if (answersChecked == this.playerControllers.Count)
                            {
                                this.answerStatus = AnswerStatus.bothAnswered; // Move to next status
                            }
                        });
                    }
                    this.resetIdling();
                    this.checkBattleIdling = false;
                }
            }
            else
            {
                for (int i = 0; i < this.playerControllers.Count; i++)
                {
                    if (this.playerControllers[i] != null)
                    {
                        this.playerControllers[i].countDownText.text = "";
                    }
                }
            }
        }
    }


    public void resetIdling()
    {
        this.count = this.idlingCounts;
        for (int i = 0; i < this.playerControllers.Count; i++)
        {
            if (this.playerControllers[i] != null && this.playerControllers[i].IsAnswered)
            {
                this.playerControllers[i].setCountDown(this.count);
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
