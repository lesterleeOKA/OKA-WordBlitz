using UnityEngine;

[System.Serializable]
public class GridManager
{
    public GameObject cellPrefab;
    public Transform playerPanel;
    public int gridSize = 4;

    public Cell[,] CreateGrid(int playerId)
    {
        Cell[,] cells = new Cell[gridSize, gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject cellObject = GameObject.Instantiate(cellPrefab, this.playerPanel != null ? this.playerPanel : null);
                cellObject.name = "Cell_" + i + "_" +j;
                Cell cell = cellObject.GetComponent<Cell>();
                char randomLetter = (char)Random.Range('A', 'Z' + 1);
                cell.SetTextContent(playerId, randomLetter.ToString());
                cell.row = i;
                cell.col = j;
                cells[i, j] = cell;
            }
        }

        return cells;
    }
}
