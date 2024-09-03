using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : GameBaseController
{
    public static GameController Instance = null;
    public PlayerController[] playerControllers;

    protected override void Awake()
    {
        if (Instance == null) Instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < this.playerControllers.Length; i++)
        {
            if (this.playerControllers[i] != null)
            {
                this.playerControllers[i].InitialPlayerGrid();
            }
        }
    }

    public override void enterGame()
    {
        base.enterGame();
    }

    public override void endGame()
    {
        bool showSuccess = false;
        /*for (int i = 0; i < this.playersList.Count; i++)
        {
            var playerController = this.playersList[i].GetComponent<PlayerController>();
            if (playerController != null)
            {
                if (playerController.Score >= 30)
                {
                    showSuccess = true;
                }

                this.endGamePage.updateFinalScore(i, playerController.Score);
            }
        }*/
        this.endGamePage.setStatus(true, showSuccess);

        base.endGame();
    }

    private void Update()
    {
        // Handle mouse input
        if (Input.GetMouseButtonUp(0))
        {
            for (int i = 0; i < this.playerControllers.Length; i++)
            {
                if (this.playerControllers[i] != null && this.playerControllers[i].IsConnectWord)
                {
                    this.playerControllers[i].StopConnection();
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
                            player.StopConnection();
                            break;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            for (int i = 0; i < this.playerControllers.Length; i++)
            {
                if (this.playerControllers[i] != null && this.playerControllers[i].IsConnectWord)
                {
                    HandleMouse(this.playerControllers[i]);
                }
            }
        }
    }

    private PlayerController GetPlayerByTouchIndex(int touchIndex)
    {
        // Map touch index to player index (assuming two players)
        if (touchIndex < playerControllers.Length)
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
