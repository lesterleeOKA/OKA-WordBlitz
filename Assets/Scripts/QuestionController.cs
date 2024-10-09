using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestionController : MonoBehaviour
{
    public static QuestionController Instance = null;
    public CurrentQuestion currentQuestion;

    private void Awake()
    {
        if(Instance == null) Instance = this;
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
            LogController.Instance?.debug("Loaded questions:" + questionDataList.questions.Count);
            if (questionDataList == null || questionDataList.questions == null || questionDataList.questions.Count == 0)
            {
                return;
            }

            string correctAnswer = this.currentQuestion.correctAnswer;
            int questionCount = questionDataList.questions.Count;
            QuestionList qa = questionDataList.questions[this.currentQuestion.numberQuestion];
            this.currentQuestion.setNewQuestion(qa, questionCount, () =>
            {
                if (LoaderConfig.Instance.apiManager.IsLogined)
                {
                    GameController.Instance.endGame();
                    return;
                }
            });
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
