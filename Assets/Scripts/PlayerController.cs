using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : UserData
{
    public GridManager gridManager;
    public LineDrawer lineDrawer;
    public Cell[,] grid;
    private List<Cell> selectedCells = new List<Cell>();
    public bool IsConnectWord = false;
    public TextMeshProUGUI answerBox;
    // Start is called before the first frame update

    public void Init()
    {
        this.grid = gridManager.CreateGrid(this.UserId);
    }

    public void NewQuestionWord(string _word)
    {
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

    public void StopConnection()
    {
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
