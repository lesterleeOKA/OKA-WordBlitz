using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestionController : MonoBehaviour
{
    public static QuestionController Instance = null;
    public CurrentQuestion currentQuestion;
    //public bool moveTonextQuestion = false;
    //public bool allowCheckingWords = true;
    //public float delayToNextQuestion = 2f;
    //public float count = 0f;

    private void Awake()
    {
        if(Instance == null) Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
       // this.count = this.delayToNextQuestion;
    }

    // Update is called once per frame
    void Update()
    {
        /*if (GameController.Instance != null && StartGame.Instance != null)
        {
            if (!GameController.Instance.gameTimer.endGame && StartGame.Instance.startedGame && this.allowCheckingWords)
            {
                if (this.moveTonextQuestion)
                {
                    if (this.count > 0f)
                    {
                        this.count -= Time.deltaTime;
                    }
                    else
                    {
                        this.count = this.delayToNextQuestion;
                        this.allowCheckingWords = false;
                    }
                }
            }
        }*/
    }
    public void nextQuestion()
    {
        LogController.Instance?.debug("next question");
        this.GetQuestionAnswer();
    }

    public void GetQuestionAnswer()
    {
        if (LoaderConfig.Instance == null || QuestionManager.Instance == null)
            return;

        try
        {
            var questionDataList = QuestionManager.Instance.questionData;
            LogController.Instance?.debug("Loaded questions:" + questionDataList.Data.Count);
            if (questionDataList == null || questionDataList.Data == null || questionDataList.Data.Count == 0)
            {
                return;
            }

            string correctAnswer = this.currentQuestion.correctAnswer;
            int questionCount = questionDataList.Data.Count;
            QuestionList qa = questionDataList.Data[this.currentQuestion.numberQuestion];
            this.currentQuestion.setNewQuestion(qa, questionCount);
            //this.moveTonextQuestion = false;
        }
        catch (Exception e)
        {
            LogController.Instance?.debugError(e.Message);
        }
    }

    public void PlayCurrentQuestionAudio()
    {
        this.currentQuestion.playAudio();
    }
}
