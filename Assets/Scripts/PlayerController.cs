using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int playerId = 0;
    public GridManager gridManager;
    public LineDrawer lineDrawer;
    public Cell[,] grid;
    private List<Cell> selectedCells = new List<Cell>();
    public bool IsConnectWord = false;
    // Start is called before the first frame update

    public void InitialPlayerGrid()
    {
        this.grid = gridManager.CreateGrid(this.playerId);
    }

    public void SelectCell(Cell cell)
    {
        if (IsAdjacent(cell))
        {
            cell.Selected();
            this.selectedCells.Add(cell);
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
    }
}
