using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : UserData
{
    public Scoring scoring;
    public string answer = string.Empty;
    public GridManager gridManager;
    public LineDrawer lineDrawer;
    public Cell[,] grid;
    private List<Cell> selectedCells = new List<Cell>();
    public bool IsAnswered = false;
    public bool IsCheckedAnswer = false;
    public bool IsConnectWord = false;
    public bool IsShowHintLetter = false;
    public bool IsCorrect = false;
    public CanvasGroup correctAnswerBox, countDownBox;
    public TextMeshProUGUI answerBox, retryTimesUIText;
    public Image answerBoxFrame;
    public Image frame, coverBlank;
    protected Vector2 originalGetScorePos = Vector2.zero;
    public CanvasGroup correctPopup, wrongPopup;
    private LoaderConfig loader = null;
    private Tween timerScaleTween = null;
    public TextMeshProUGUI countDownText = null;

    // Start is called before the first frame update

    public void Init(string _word, Sprite[] defaultAnswerBoxes=null, Sprite[] defaultFrames = null, GridWordFormat gridWordFormat = GridWordFormat.AllUpper)
    {
        this.updateRetryTimes(false);
        this.loader = LoaderConfig.Instance;
        if (this.correctPopup != null) this.originalGetScorePos = this.correctPopup.transform.localPosition;

        if (this.PlayerIcons[0] == null)
        {
            this.PlayerIcons[0] = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Icon").GetComponent<PlayerIcon>();
        }

        if(this.scoring.scoreTxt == null)
        {
            this.scoring.scoreTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Score").GetComponent<TextMeshProUGUI>();
        }

        if (this.scoring.resultScoreTxt == null)
        {
            this.scoring.resultScoreTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_ResultScore").GetComponent<TextMeshProUGUI>();
        }
        this.scoring.init();
        float frame_width = this.GetComponent<RectTransform>().sizeDelta.x;

        Sprite gridTexture = this.loader.gameSetup.gridTexture != null ? 
            SetUI.ConvertTextureToSprite(this.loader.gameSetup.gridTexture as Texture2D) : null;
        this.grid = gridManager.CreateGrid(this.UserId, _word, frame_width, gridTexture, gridWordFormat);


        if(this.frame != null)
        {
            switch (this.UserId)
            {
                case 0:
                    this.answerBoxFrame.sprite = defaultAnswerBoxes[0];
                    if (this.loader.gameSetup.frameTexture_p1 != null)
                    {
                        Sprite frameTexture_p1 = SetUI.ConvertTextureToSprite(this.loader.gameSetup.frameTexture_p1 as Texture2D);
                        this.frame.sprite = frameTexture_p1;
                    }
                    else
                    {
                        this.frame.sprite = defaultFrames[0];
                    }
                    break;
                case 1:
                    this.answerBoxFrame.sprite = defaultAnswerBoxes[1];
                    if (this.loader.gameSetup.frameTexture_p2 != null)
                    {
                        Sprite frameTexture_p2 = SetUI.ConvertTextureToSprite(this.loader.gameSetup.frameTexture_p2 as Texture2D);
                        this.frame.sprite = frameTexture_p2;
                    }
                    else
                    {
                        this.frame.sprite = defaultFrames[1];
                    }
                    break;
            }
        }
    }

    void updateRetryTimes(bool deduct = false)
    {          
        if(deduct)
        {
            if(this.Retry > 0)
            {
                this.Retry--;
            }
        }
        else
        {
            this.NumberOfRetry = LoaderConfig.Instance.gameSetup.retry_times;
            this.Retry = this.NumberOfRetry;
        }

        if (this.retryTimesUIText != null)
        {
            this.retryTimesUIText.text = this.Retry + "/" + this.NumberOfRetry;
        }
    }

    public void NewQuestionWord(string _word, GridWordFormat gridWordFormat)
    {
        SetUI.Set(this.correctAnswerBox, false);
        this.updateRetryTimes(false);
        this.gridManager.UpdateGridWithWord(this.UserId, _word, gridWordFormat);
        this.gridManager.setFirstLetterHint(this.IsShowHintLetter);
    }

    public void updatePlayerIcon(bool _status = false, string _playerName = "", Sprite _icon= null, Color32 _color=default)
    {
        for (int i = 0; i < this.PlayerIcons.Length; i++)
        {
            if (this.PlayerIcons[i] != null) { 
                this.PlayerColor = _color;
                this.PlayerIcons[i].playerColor = _color;
                this.PlayerIcons[i].SetStatus(_status, _playerName, _icon);
            }
        }
    }

    public void SelectCell(Cell cell)
    {
        if (IsAdjacent(cell))
        {
            GameController.Instance.resetIdling();
            cell.Selected();
            this.selectedCells.Add(cell);
            if(this.answerBox != null) this.answerBox.text += cell.content.text;
            this.lineDrawer?.AddPoint(cell.transform.position);
        }
    }

    private bool IsAdjacent(Cell newCell)
    {
        // If there are no selected cells, any cell can be selected
        if (selectedCells.Count == 0) return true;
        // Get the last selected cell
        Cell lastSelected = selectedCells[selectedCells.Count - 1];
        // Check if the new cell is adjacent to the last selected cell
        bool isAdjacent = (Mathf.Abs(newCell.row - lastSelected.row) <= 1 &&
                           Mathf.Abs(newCell.col - lastSelected.col) <= 1);
        // Check if the new cell is already selected
        bool isAlreadySelected = newCell.isSelected;
        // Block connection if not adjacent or already selected
        return isAdjacent && !isAlreadySelected;
    }

    public void StartConnection()
    {
        GameController.Instance.resetIdling();
        this.IsConnectWord = true; // Start drawing
        this.lineDrawer?.StartDrawing();
    }

    string CapitalizeFirstLetter(string str)
    {
        if (string.IsNullOrEmpty(str)) return str; // Return if the string is empty or null
        return char.ToUpper(str[0]) + str.Substring(1).ToLower();
    }

    public void checkAnswer(int currentTime, Action onCompleted = null)
    {
        if (!this.IsCheckedAnswer) {
            var currentQuestion = QuestionController.Instance?.currentQuestion;
            int eachQAScore = currentQuestion.qa.score.full == 0 ? 10 : currentQuestion.qa.score.full;
            int currentScore = this.Score;
            this.answer = this.answerBox.text.ToLower();

            if(this.correctAnswerBox != null && !string.IsNullOrEmpty(currentQuestion.correctAnswer))
            {
                switch (GameController.Instance.gridWordFormat)
                {
                    case GridWordFormat.AllUpper:
                        this.correctAnswerBox.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.correctAnswer.ToUpper();
                        break;
                    case GridWordFormat.AllLower:
                        this.correctAnswerBox.GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.correctAnswer.ToLower();
                        break;
                    case GridWordFormat.FirstUpper:
                        char firstLetter = char.ToUpper(currentQuestion.correctAnswer[0]);
                        string remainingLetters = currentQuestion.correctAnswer.Substring(1).ToLower();
                        this.correctAnswerBox.GetComponentInChildren<TextMeshProUGUI>().text = firstLetter + remainingLetters;
                        break;
                }
            }
            
            var lowerQIDAns = currentQuestion.correctAnswer.ToLower();
            int resultScore = this.scoring.score(this.answer, currentScore, lowerQIDAns, eachQAScore);
            this.Score = resultScore;
            this.IsCheckedAnswer = true;
            this.IsCorrect = this.scoring.correct;
            StartCoroutine(this.showAnswerResult(this.scoring.correct));

            if (this.UserId == 0 && this.loader != null && this.loader.apiManager.IsLogined) // For first player
            {
                float currentQAPercent = 0f;
                int correctId = 0;
                float score = 0f;
                float answeredPercentage;
                int progress = (int)((float)currentQuestion.answeredQuestion / QuestionManager.Instance.totalItems * 100);

                if (this.answer == lowerQIDAns)
                {
                    if (this.CorrectedAnswerNumber < QuestionManager.Instance.totalItems)
                        this.CorrectedAnswerNumber += 1;

                    correctId = 2;
                    score = eachQAScore; // load from question settings score of each question
                    //Debug.Log("Each QA Score!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + eachQAScore + "______answer" + this.answer);
                    currentQAPercent = 100f;
                }
                else
                {
                    if (this.CorrectedAnswerNumber > 0)
                    {
                        this.CorrectedAnswerNumber -= 1;
                    }
                }

                if (this.CorrectedAnswerNumber < QuestionManager.Instance.totalItems)
                {
                    answeredPercentage = this.AnsweredPercentage(QuestionManager.Instance.totalItems);
                }
                else
                {
                    answeredPercentage = 100f;
                }

                this.loader.SubmitAnswer(
                           currentTime,
                           this.Score,
                           answeredPercentage,
                           progress,
                           correctId,
                           currentTime,
                           currentQuestion.qa.qid,
                           currentQuestion.correctAnswerId,
                           this.CapitalizeFirstLetter(this.answer),
                           currentQuestion.correctAnswer,
                           score,
                           currentQAPercent,
                           ()=>
                           {
                               this.resetConnection();
                               onCompleted?.Invoke();
                           }
                           );
            }
            else
            {
                this.resetConnection();
                onCompleted?.Invoke();
            }
        }
    }

    public IEnumerator showAnswerResult(bool correct)
    {
        bool isBattleMode = this.loader.gameSetup.playerNumber > 1;
        if(isBattleMode) this.setCoverBlank(true);
        this.IsConnectWord = false;
        float delay = GameController.Instance.answeredDelay;
        if (correct)
        {
            LogController.Instance?.debug("Add marks" + this.Score);
            this.setGetScorePopup(true);
            AudioController.Instance?.PlayAudio(1);
            yield return new WaitForSeconds(delay - 1f);
            if (!isBattleMode)
            {
                GameController.Instance?.UpdateNextQuestion();
                this.resetAnswer();
            }
        }
        else
        {
            this.updateRetryTimes(true);
            this.setWrongPopup(true);
            AudioController.Instance?.PlayAudio(2);
            if (!isBattleMode)
            {
                if (this.Retry <= 0)
                {
                    SetUI.Set(this.correctAnswerBox, true);
                }
            }
            yield return new WaitForSeconds(delay);
            if (!isBattleMode)
            {
                if (this.Retry <= 0)
                {
                    GameController.Instance?.UpdateNextQuestion();
                }
                this.resetAnswer();
            }
        }
    }

    public void StopConnection(int currentTime= 0)
    {
        //Check Answer
        this.gridManager.setFirstLetterHint(this.IsShowHintLetter);
        if (this.answerBox != null && this.IsConnectWord)
        {
            if (!string.IsNullOrEmpty(this.answerBox.text))
            {
                this.checkAnswer(currentTime);
            }
            this.IsConnectWord = false;
        }
    }

    public void setCoverBlank(bool status)
    {
        GameController.Instance.checkBattleIdling = status;
        if (this.answerBox != null) 
            this.answerBox.GetComponent<CanvasGroup>().DOFade(status ? 0f : 1f, 0f);
        this.IsAnswered = status;
        if (this.lineDrawer != null) this.lineDrawer.lineRenderer.enabled = !status;
        if (this.coverBlank != null)
        {
            this.coverBlank.raycastTarget = status;
            this.coverBlank.DOFillAmount(status ? 1f : 0f, 0.5f);
        }

        if (this.Retry == 0 && !status)
        {
            SetUI.Set(this.correctAnswerBox, true);
        }
    }

    public void setCountDown(float count=0f)
    {
        if (this.countDownText != null && this.countDownBox != null)
        {
            if(count < 5.99f)
            {
                this.countDownBox.DOFade(1f, 0f);
                string countDown = Mathf.FloorToInt(count).ToString();

                if (string.IsNullOrEmpty(this.countDownText.text) && this.timerScaleTween == null)
                {
                    this.timerScaleTween = this.countDownText.transform.DOScale(0.8f, 0.5f).SetLoops(-1, LoopType.Yoyo);
                }
                this.countDownText.text = countDown;
            }
            else
            {
                this.countDownBox.DOFade(0f, 0f);
                this.countDownText.text = "";
            }
        }
    }

    public void resetAnswer()
    {   if(this.timerScaleTween != null) {
           this.countDownText.transform.DOScale(1.0f, 0f);
           this.timerScaleTween.Kill();
           this.timerScaleTween = null;
        }
        this.scoring.correct = false;
        this.IsCheckedAnswer = false;
        this.IsAnswered = false;
        this.IsCorrect = false;
        this.answer = "";
        this.setGetScorePopup(false);
        this.setWrongPopup(false);
        if (this.answerBox != null) this.answerBox.text = "";
    }

    public void resetConnection()
    {
        this.lineDrawer?.FinishDrawing();

        for (int i = 0; i < this.selectedCells.Count; i++)
        {
            if (this.selectedCells[i] != null)
            {
                this.selectedCells[i].DisSelected();
            }
        }
        this.selectedCells.Clear();
        this.gridManager.setFirstLetterHint(this.IsShowHintLetter);
    }

    public void showHintOfFirstLetter(Button hintBtn = null)
    {
        this.IsShowHintLetter = !this.IsShowHintLetter;
        if(hintBtn != null) hintBtn.GetComponent<Image>().color = this.IsShowHintLetter ?  Color.gray: Color.white;
        this.gridManager.setFirstLetterHint(this.IsShowHintLetter);
    }

    public void hiddenHintOfFirstLetter()
    {
        this.gridManager.setFirstLetterHint(false);
    }

    public void setGetScorePopup(bool status)
    {
        SetUI.SetMove(this.correctPopup, status, status ? Vector2.zero : this.originalGetScorePos, status ? 0.5f : 0f);
    }

    public void setWrongPopup(bool status)
    {
        SetUI.SetMove(this.wrongPopup, status, status ? Vector2.zero : this.originalGetScorePos, status ? 0.5f : 0f);
    }
}
