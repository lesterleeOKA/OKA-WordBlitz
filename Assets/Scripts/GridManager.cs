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
    private Cell[,] cells;

    public Cell[,] CreateGrid(int playerId)
    {
        cells = new Cell[gridRow, gridColumn];

        for (int i = 0; i < gridRow; i++)
        {
            for (int j = 0; j < gridColumn; j++)
            {
                GameObject cellObject = GameObject.Instantiate(cellPrefab, this.playerPanel != null ? this.playerPanel : null);
                cellObject.name = "Cell_" + i + "_" +j;
                Cell cell = cellObject.GetComponent<Cell>();
               // char randomLetter = (char)UnityEngine.Random.Range('A', 'Z' + 1);
                cell.SetTextContent(playerId, "");
                cell.row = i;
                cell.col = j;
                cells[i, j] = cell;
            }
        }

        // If an initial word is provided, place it in the grid
        this.PlaceWordInGrid(playerId, "Computer");
        this.FillRemainingCells(playerId);

        return cells;
    }

    public void UpdateGridWithWord(int playerId, string newWord)
    {
        ClearGrid(playerId);
        PlaceWordInGrid(playerId, newWord);
        FillRemainingCells(playerId);
    }

    // Clear existing cell content
    private void ClearGrid(int playerId)
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
    }

    private void PlaceWordInGrid(int playerId, string word)
    {
        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        // Randomly select the starting position for the first letter
        Vector2Int startPos;
        do
        {
            startPos = new Vector2Int(Random.Range(0, gridRow), Random.Range(0, gridColumn));
        } while (usedPositions.Contains(startPos)); // Ensure position is unique

        // Place the first letter
        usedPositions.Add(startPos);
        cells[startPos.x, startPos.y].SetTextContent(playerId, word[0].ToString());

        Vector2Int lastPos = startPos;

        // Directions for adjacent cells (up, down, left, right)
        Vector2Int[] directions = {
        new Vector2Int(0, 1),   // Right
        new Vector2Int(1, 0),   // Down
        new Vector2Int(0, -1),  // Left
        new Vector2Int(-1, 0)   // Up
    };

        // Place the remaining letters
        for (int i = 1; i < word.Length; i++)
        {
            Vector2Int newPos;
            bool positionFound;

            do
            {
                positionFound = false;

                // Randomly select a direction
                Vector2Int direction = directions[Random.Range(0, directions.Length)];
                newPos = lastPos + direction;

                // Check if the new position is within bounds and not already used
                if (newPos.x >= 0 && newPos.x < gridRow && newPos.y >= 0 && newPos.y < gridColumn &&
                    !usedPositions.Contains(newPos))
                {
                    positionFound = true;
                }

            } while (!positionFound);

            // Place the letter
            usedPositions.Add(newPos);
            cells[newPos.x, newPos.y].SetTextContent(playerId, word[i].ToString().ToUpper()); // set char to upper
            lastPos = newPos; // Update last position
        }
    }

    private void FillRemainingCells(int playerId)
    {
        HashSet<char> usedLetters = new HashSet<char>();

        // Collect letters already used in the grid
        foreach (var cell in cells)
        {
            if (!string.IsNullOrEmpty(cell.content.text))
            {
                usedLetters.Add(cell.content.text[0]);
            }
        }

        char randomLetter;

        for (int i = 0; i < gridRow; i++)
        {
            for (int j = 0; j < gridColumn; j++)
            {
                if (string.IsNullOrEmpty(cells[i, j].content.text)) // If cell is empty
                {
                    do
                    {
                        randomLetter = (char)Random.Range('A', 'Z' + 1);
                    } while (usedLetters.Contains(randomLetter)); // Ensure it's not a used letter

                    cells[i, j].SetTextContent(playerId, randomLetter.ToString()); // Use -1 for playerId for random letters
                }
            }
        }
    }
}
