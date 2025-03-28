using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GridManager
{
    public GameObject cellPrefab;
    public Transform playerPanel;
    public int gridRow = 4;
    public int gridColumn = 4;
    public int maxRetries = 10;
    private Cell[,] cells;
    private List<Vector2Int> questionCells = new List<Vector2Int>();
    HashSet<Vector2Int> usedPositions = null;
    List<Vector2Int> availablePositions = null;
    HashSet<char> usedLetters = null;
    public bool showQuestionWordPosition = false;

    public Cell[,] CreateGrid(int playerId, string initialWord, float frame_width, Sprite cellSprite = null, GridWordFormat gridWordFormat = GridWordFormat.AllUpper)
    {
        this.usedPositions = new HashSet<Vector2Int>();
        this.availablePositions = new List<Vector2Int>();
        this.usedLetters = new HashSet<char>();
        float grid_width = (frame_width - ((this.gridColumn + 1) * 20f)) / this.gridColumn;
        var gridLayout = this.playerPanel.GetComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(grid_width, grid_width);

        cells = new Cell[gridRow, gridColumn];

        for (int i = 0; i < gridRow; i++)
        {
            for (int j = 0; j < gridColumn; j++)
            {
                GameObject cellObject = GameObject.Instantiate(cellPrefab, this.playerPanel != null ? this.playerPanel : null);
                cellObject.name = "Cell_" + i + "_" +j;
                Cell cell = cellObject.GetComponent<Cell>();
                cell.SetTextContent(playerId, "", default, cellSprite);
                cell.row = i;
                cell.col = j;
                cells[i, j] = cell;
            }
        }

        // If an initial word is provided, place it in the grid
        this.PlaceWordInGrid(playerId, initialWord, gridWordFormat);
        this.FillRemainingCells(playerId, gridWordFormat);

        return cells;
    }

    public void UpdateGridWithWord(int playerId, string newWord, GridWordFormat gridWordFormat)
    {
       this.questionCells.Clear();
       this.PlaceWordInGrid(playerId, newWord, gridWordFormat);
       this.FillRemainingCells(playerId, gridWordFormat);
    }

    public void setLetterHint(bool status, Color32 toColor=default)
    {
        foreach (var wordCellPos in this.questionCells)
        {
            this.cells[wordCellPos.x, wordCellPos.y].SetButtonColor(status? toColor : Color.white);
        }
    }

    public void setFirstLetterHint(bool status)
    {
        for(int i=0; i< this.questionCells.Count; i++)
        {
            if (i == 0)
            {
                this.cells[this.questionCells[i].x, this.questionCells[i].y].SetTextColor(status ? Color.yellow : Color.black);
            }
        }
    }

    private void PlaceWordInGrid(int playerId, string word, GridWordFormat gridWordFormat)
    {
        this.usedPositions.Clear();
        this.availablePositions.Clear();

        string firstLetter = "";
        switch (gridWordFormat)
        {
            case GridWordFormat.AllUpper:
            case GridWordFormat.FirstUpper:
                firstLetter = word[0].ToString().ToUpper();
                break;
            case GridWordFormat.AllLower:
                firstLetter = word[0].ToString().ToLower();
                break;
        }

        // Populate available positions and reset cells
        for (int x = 0; x < gridRow; x++)
        {
            for (int y = 0; y < gridColumn; y++)
            {
                this.availablePositions.Add(new Vector2Int(x, y));
                this.cells[x, y].SetTextContent(playerId, "");
            }
        }

        bool placed = false;
        for (int attempt = 0; attempt < this.maxRetries; attempt++)
        {
            // Randomly select the starting position for the first letter
            Vector2Int startPos = availablePositions[Random.Range(0, availablePositions.Count)];
            this.availablePositions.Remove(startPos);

            // Place the first letter
            this.usedPositions.Add(startPos);
            this.cells[startPos.x, startPos.y].SetTextContent(playerId, firstLetter, this.showQuestionWordPosition ? Color.yellow : Color.white);
            this.questionCells.Add(startPos);

            // Attempt to place the remaining letters
            if (PlaceLetters(playerId, word, startPos, 1, gridWordFormat))
            {
                placed = true; // Successfully placed the word
                break; // Exit the retry loop
            }
            else
            {
                // Backtrack: Clean up if placement failed
                this.usedPositions.Remove(startPos);
                cells[startPos.x, startPos.y].SetTextContent(playerId, ""); // Clear the cell
            }
        }

        if (!placed)
        {
            LogController.Instance?.debugError("Failed to place the word on the grid after multiple attempts.");
            // Optionally, you could take further action here, like notifying the player or trying a different word.
        }
    }

    private bool PlaceLetters(int playerId, string word, Vector2Int lastPos, int index, GridWordFormat gridWordFormat)
    {
        if (index >= word.Length) return true; // All letters placed successfully

        // Directions for adjacent cells (up, down, left, right)
        Vector2Int[] directions = {
        new Vector2Int(0, 1),   // Right
        new Vector2Int(1, 0),   // Down
        new Vector2Int(0, -1),  // Left
        new Vector2Int(-1, 0)   // Up
        };

        // Shuffle directions
        directions = directions.OrderBy(x => Random.value).ToArray();

        foreach (var direction in directions)
        {
            Vector2Int newPos = lastPos + direction;

            // Check if the new position is within bounds and not already used
            if (newPos.x >= 0 && newPos.x < gridRow && newPos.y >= 0 && newPos.y < gridColumn &&
                !this.usedPositions.Contains(newPos))
            {

                string remainLetter = "";
                switch (gridWordFormat)
                {
                    case GridWordFormat.AllUpper:
                        remainLetter = word[index].ToString().ToUpper();
                        break;
                    case GridWordFormat.AllLower:
                    case GridWordFormat.FirstUpper:
                        remainLetter = word[index].ToString().ToLower();
                        break;
                }

                // Place the letter
                this.usedPositions.Add(newPos);
                this.cells[newPos.x, newPos.y].SetTextContent(playerId, remainLetter, this.showQuestionWordPosition ? Color.yellow : Color.white);
                this.questionCells.Add(newPos);
                // Recursively attempt to place the next letter
                if (PlaceLetters(playerId, word, newPos, index + 1, gridWordFormat))
                    return true;

                // Backtrack
                this.usedPositions.Remove(newPos);
                this.cells[newPos.x, newPos.y].SetTextContent(playerId, ""); // Clear the cell
            }
        }

        return false; // Failed to place the word
    }

    private void FillRemainingCells(int playerId, GridWordFormat gridWordFormat)
    {
        this.usedLetters.Clear();

        // Collect letters already used in the grid
        foreach (var cell in cells)
        {
            if (!string.IsNullOrEmpty(cell.content.text))
            {
                usedLetters.Add(cell.content.text[0]);
            }
        }

        // Create a list of available letters
        List<char> availableLetters = new List<char>();
        for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            if (!usedLetters.Contains(letter))
            {
                availableLetters.Add(letter);
            }
        }

        // If no available letters, fill with a default or random letter
        if (availableLetters.Count == 0)
        {
            availableLetters.AddRange(Enumerable.Range('A', 26).Select(i => (char)i));
        }

        for (int i = 0; i < gridRow; i++)
        {
            for (int j = 0; j < gridColumn; j++)
            {
                if (string.IsNullOrEmpty(cells[i, j].content.text)) // If cell is empty
                {
                    // Pick a random letter from the available letters

                    string randLetter = availableLetters[Random.Range(0, availableLetters.Count)].ToString();
                    switch (gridWordFormat)
                    {
                        case GridWordFormat.AllUpper:
                            randLetter = randLetter.ToUpper();
                            break;
                        case GridWordFormat.AllLower:
                        case GridWordFormat.FirstUpper:
                            randLetter = randLetter.ToLower();
                            break;
                    }

                    cells[i, j].SetTextContent(playerId, randLetter);
                }
            }
        }
    }

    private void FillNormalCells(int playerId)
    {
        for (int i = 0; i < gridRow; i++)
        {
            for (int j = 0; j < gridColumn; j++)
            {
                char randomLetter = (char)Random.Range('A', 'Z' + 1);
                cells[i, j].SetTextContent(playerId, randomLetter.ToString());
            }
        }
    }

}


public enum GridWordFormat
{
    AllUpper = 0,
    AllLower = 1,
    FirstUpper = 2
}