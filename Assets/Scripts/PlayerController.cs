using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : UserData
{
    public Scoring scoring;
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
        this.grid = gridManager.CreateGrid(this.UserId, _word);
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
            if (this.PlayerIcons[i] != null)
                this.PlayerIcons[i].SetStatus(_status, _playerName, _icon);
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

    public void checkAnswer()
    {
        if(!this.IsCheckedAnswer) { 
            int currentScore = this.Score;
            var lowerUserAns = this.answerBox.text.ToLower();
            var lowerQIDAns = QuestionController.Instance?.currentQuestion.correctAnswer.ToLower();
            int resultScore = this.scoring.score(lowerUserAns, currentScore, lowerQIDAns);
            this.Score = resultScore;
            this.IsCheckedAnswer = true;
            StartCoroutine(this.showResult(this.scoring.correct));
        }
        //LogController.Instance?.debug("Add marks" + this.Score);
    }

    public IEnumerator showResult(bool correct)
    {
        float delay = 1f;
        if (correct)
        {
            delay = 2f;
            SetUI.SetMove(GameController.Instance?.getScorePopup, true, new Vector2(0f, 0f), 0.5f);
        }
        AudioController.Instance?.PlayAudio(correct ? 1 : 2);
        yield return new WaitForSeconds(delay);
        SetUI.SetMove(GameController.Instance?.getScorePopup, false, GameController.Instance.originalGetScorePos, 0f);
        this.scoring.correct = false;
        this.IsCheckedAnswer = false;
        if(correct) GameController.Instance?.UpdateNextQuestion();
    }

    public void StopConnection()
    {
        //Check Answer
        if(this.answerBox != null)
        {
            if (!string.IsNullOrEmpty(this.answerBox.text))
            {
                this.checkAnswer();
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
