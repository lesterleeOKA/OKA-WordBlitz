using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBaseController : MonoBehaviour
{
    public Timer gameTimer;
    public CanvasGroup GameUILayer, TopUILayer, getScorePopup;
    public Vector2 originalGetScorePos = Vector2.zero;
    public EndGamePage endGamePage;
    public int playerNumber = 0;

    protected virtual void Awake()
    {
        LoaderConfig.Instance?.InitialGameBackground();
    }

    protected virtual void Start()
    {
        this.playerNumber = LoaderConfig.Instance != null ? LoaderConfig.Instance.PlayerNumbers : 2;
        SetUI.Set(this.TopUILayer, false, 0f);
        SetUI.Set(this.getScorePopup, false, 0f);
        if (this.getScorePopup != null) this.originalGetScorePos = this.getScorePopup.transform.localPosition;
        this.endGamePage.init(this.playerNumber);
    }

    public virtual void enterGame()
    {
        SetUI.Set(this.TopUILayer, true, 0.5f);
        SetUI.Set(this.GameUILayer, true, 0.5f);
    }

    public virtual void endGame()
    {
        SetUI.Set(this.TopUILayer, false, 0f);
        SetUI.Set(this.GameUILayer, false, 0f);
    }

    public void retryGame()
    {
        if (AudioController.Instance != null) AudioController.Instance.changeBGMStatus(true);
        SceneManager.LoadScene(2);
    }


    public void setGetScorePopup(bool status)
    {
        SetUI.SetMove(this.getScorePopup, status, status ? Vector2.zero : this.originalGetScorePos, status? 0.5f : 0f);
    }

    public void BackToWebpage()
    {
        ExternalCaller.BackToHomeUrlPage();
    }
}
