using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : UserData
{
    public Scoring scoring;
    public string answer = string.Empty;
    public GridManager gridManager;
    public LineDrawer lineDrawer;
    public Cell[,] grid;
    private List<Cell> selectedCells = new List<Cell>();
    public bool IsCheckedAnswer = false;
    public bool IsConnectWord = false;
    public TextMeshProUGUI answerBox;
    // Start is called before the first frame update

    public void Init(string _word)
    {
        this.scoring.init();
        float frame_width = this.GetComponent<RectTransform>().sizeDelta.x;
        this.grid = gridManager.CreateGrid(this.UserId, _word, frame_width);
    }

    public void NewQuestionWord(string _word)
    {
        this.StopConnection();
        this.gridManager.UpdateGridWithWord(this.UserId, _word);
    }

    public void updatePlayerIcon(bool _status = false, string _playerName = "", Sprite _icon= null)
    {
        for (int i = 0; i < this.PlayerIcons.Length; i++)
        {
            if (this.PlayerIcons[i] != null) { 
                this.PlayerIcons[i].playerColor = this.PlayerColor;
                this.PlayerIcons[i].SetStatus(_status, _playerName, _icon);
            }
        }

    }

    public void SelectCell(Cell cell)
    {
        if (IsAdjacent(cell))
        {
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
        this.IsConnectWord = true; // Start drawing
        this.lineDrawer?.StartDrawing();
    }

    public void checkAnswer(int currentTime)
    {
        if(!this.IsCheckedAnswer) {
            var loader = LoaderConfig.Instance;
            var currentQuestion = QuestionController.Instance?.currentQuestion;
            int eachQAScore = currentQuestion.qa.score == 0 ? 10 : currentQuestion.qa.score;
            int currentScore = this.Score;
            this.answer = this.answerBox.text.ToLower();
            var lowerQIDAns = currentQuestion.correctAnswer.ToLower();
            int resultScore = this.scoring.score(this.answer, currentScore, lowerQIDAns, eachQAScore);
            this.Score = resultScore;
            this.IsCheckedAnswer = true;
            StartCoroutine(this.showAnswerResult(this.scoring.correct));

            if (this.UserId == 0 && loader != null && loader.apiManager.IsLogined) // For first player
            {
                float currentQAPercent = 0f;
                int correctId = 0;
                float score = 0f;
                float answeredPercentage;
                int progress = (int)((float)currentQuestion.answeredQuestion / QuestionManager.Instance.totalItems * 100);

                if (this.answer == currentQuestion.correctAnswer)
                {
                    if (this.CorrectedAnswerNumber < QuestionManager.Instance.totalItems)
                        this.CorrectedAnswerNumber += 1;

                    correctId = 2;
                    score = eachQAScore; // load from question settings score of each question
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

                loader.SubmitAnswer(
                           currentTime,
                           this.Score,
                           answeredPercentage,
                           progress,
                           correctId,
                           currentTime,
                           currentQuestion.qa.qid,
                           currentQuestion.correctAnswerId,
                           this.answer,
                           currentQuestion.correctAnswer,
                           score,
                           currentQAPercent
                           );
            }
        }
    }

    public IEnumerator showAnswerResult(bool correct)
    {
        float delay = 2f;
        if (correct)
        {
            LogController.Instance?.debug("Add marks" + this.Score);
            GameController.Instance?.setGetScorePopup(true);
            AudioController.Instance?.PlayAudio(1);
            yield return new WaitForSeconds(delay);
            GameController.Instance?.setGetScorePopup(false);
        }
        else
        {
            GameController.Instance?.setWrongPopup(true);
            AudioController.Instance?.PlayAudio(2);
            yield return new WaitForSeconds(delay);
            GameController.Instance?.setWrongPopup(false);
        }
        this.scoring.correct = false;
        this.IsCheckedAnswer = false;
        GameController.Instance?.UpdateNextQuestion();
    }

    public void StopConnection(int currentTime= 0)
    {
        //Check Answer
        if(this.answerBox != null)
        {
            if (!string.IsNullOrEmpty(this.answerBox.text) && currentTime > 0)
            {
                this.checkAnswer(currentTime);
            }
        }

        this.IsConnectWord = false;  // Stop drawing
        this.lineDrawer?.FinishDrawing();

        for (int i = 0; i < this.selectedCells.Count; i++)
        {
            if (this.selectedCells[i] != null)
            {
                this.selectedCells[i].DisSelected();
            }
        }
        this.selectedCells.Clear();
        if (this.answerBox != null) this.answerBox.text = "";
    }
}
