using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : GameBaseController
{
    public static GameController Instance = null;
    // Start is called before the first frame update

    protected override void Awake()
    {
        if (Instance == null) Instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
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
}
