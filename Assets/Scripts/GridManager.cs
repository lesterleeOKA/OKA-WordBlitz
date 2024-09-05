using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GridManager
{
    public GameObject cellPrefab;
    public Transform playerPanel;
    public int gridRow = 4;
    public int gridColumn = 4;
    public int maxRetries = 10;
    private Cell[,] cells;
    HashSet<Vector2Int> usedPositions = null;
    List<Vector2Int> availablePositions = null;
    HashSet<char> usedLetters = null;
    public bool showQuestionWordPosition = false;

    public Cell[,] CreateGrid(int playerId, string initialWord)
    {
        this.usedPositions = new HashSet<Vector2Int>();
        this.availablePositions = new List<Vector2Int>();
        this.usedLetters = new HashSet<char>();

        cells = new Cell[gridRow, gridColumn];

        for (int i = 0; i < gridRow; i++)
        {
            for (int j = 0; j < gridColumn; j++)
            {
                GameObject cellObject = GameObject.Instantiate(cellPrefab, this.playerPanel != null ? this.playerPanel : null);
                cellObject.name = "Cell_" + i + "_" +j;
                Cell cell = cellObject.GetComponent<Cell>();
                cell.SetTextContent(playerId, "");
                cell.row = i;
                cell.col = j;
                cells[i, j] = cell;
            }
        }

        // If an initial word is provided, place it in the grid
        this.PlaceWordInGrid(playerId, initialWord);
        this.FillRemainingCells(playerId);

        return cells;
    }

    public void UpdateGridWithWord(int playerId, string newWord)
    {
       this.PlaceWordInGrid(playerId, newWord);
       this.FillRemainingCells(playerId);
    }

    // Clear existing cell content
    /* private void ClearGrid(int playerId)
     {
         if (cells != null)
         {
             foreach (Cell cell in cells)
             {
                 if (cell != null)
                 {
                     cell.DisSelected(); // Optional: Reset selection state if needed
                     cell.SetTextContent(playerId, ""); // Use -1 for playerId to indicate empty
                 }
             }
         }
     }*/

    private void PlaceWordInGrid(int playerId, string word)
    {
        this.usedPositions.Clear();
        this.availablePositions.Clear();

        // Populate available positions and reset cells
        for (int x = 0; x < gridRow; x++)
        {
            for (int y = 0; y < gridColumn; y++)
            {
                availablePositions.Add(new Vector2Int(x, y));
                cells[x, y].SetTextContent(playerId, "");
            }
        }

        bool placed = false;
        for (int attempt = 0; attempt < this.maxRetries; attempt++)
        {
            // Randomly select the starting position for the first letter
            Vector2Int startPos = availablePositions[Random.Range(0, availablePositions.Count)];
            availablePositions.Remove(startPos);

            // Place the first letter
            usedPositions.Add(startPos);
            cells[startPos.x, startPos.y].SetTextContent(playerId, word[0].ToString().ToUpper(), this.showQuestionWordPosition ? Color.yellow : Color.white);

            // Attempt to place the remaining letters
            if (PlaceLetters(playerId, word, startPos, 1))
            {
                placed = true; // Successfully placed the word
                break; // Exit the retry loop
            }
            else
            {
                // Backtrack: Clean up if placement failed
                usedPositions.Remove(startPos);
                cells[startPos.x, startPos.y].SetTextContent(playerId, ""); // Clear the cell
            }
        }

        if (!placed)
        {
            LogController.Instance?.debugError("Failed to place the word on the grid after multiple attempts.");
            // Optionally, you could take further action here, like notifying the player or trying a different word.
        }
    }

    private bool PlaceLetters(int playerId, string word, Vector2Int lastPos, int index)
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
                !usedPositions.Contains(newPos))
            {
                // Place the letter
                usedPositions.Add(newPos);
                cells[newPos.x, newPos.y].SetTextContent(playerId, word[index].ToString().ToUpper(), this.showQuestionWordPosition ? Color.yellow : Color.white);

                // Recursively attempt to place the next letter
                if (PlaceLetters(playerId, word, newPos, index + 1))
                    return true;

                // Backtrack
                usedPositions.Remove(newPos);
                cells[newPos.x, newPos.y].SetTextContent(playerId, ""); // Clear the cell
            }
        }

        return false; // Failed to place the word
    }

    private void FillRemainingCells(int playerId)
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

        char randomLetter;

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
                    randomLetter = availableLetters[Random.Range(0, availableLetters.Count)];
                    cells[i, j].SetTextContent(playerId, randomLetter.ToString());
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
