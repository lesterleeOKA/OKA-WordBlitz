using System;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class QuestionDataWrapper
{
    public QuestionList[] QuestionDataArray;
}

[Serializable]
public class QuestionData
{ 
    public List<QuestionList> questions;
}
[Serializable]
public class QuestionList
{
    public int id;
    public string qid;
    public string questionType;
    public string question;
    public string[] answers;
    public string correctAnswer;
    public int star;
    public score score;
    public int correctAnswerIndex;
    public int maxScore;
    public learningObjective learningObjective;
    //public media[] media; 
    public string[] media;
    public Texture texture;
    public AudioClip audioClip;
}

[Serializable]
public class media
{
    public string url;
    public string name;
}

[Serializable]
public class score
{
    public int full;
    public int n;
    public int unit;
}

[Serializable]
public class learningObjective
{
}


public enum QuestionType
{
    None = 0,
    Text = 1,
    Picture = 2,
    Audio = 3,
    FillInBlank = 4
}

[Serializable]
public class CurrentQuestion
{
    public int numberQuestion = 0;
    public int answeredQuestion = 0;
    public QuestionType questiontype = QuestionType.None;
    public QuestionList qa = null;
    public int correctAnswerId;
    public string correctAnswer;
    public string[] answersChoics;
    public CanvasGroup[] questionBgs;
    private RawImage questionImage;
    public CanvasGroup audioPlayBtn = null;
    private AspectRatioFitter aspecRatioFitter = null;
    public CanvasGroup progressiveBar;
    public Image progressFillImage;

    public void setProgressiveBar(bool status)
    {
        if (this.progressiveBar != null)
        {
            this.progressiveBar.DOFade(status ? 1f : 0f, 0f);
        }
    }

    public bool updateProgressiveBar(int totalQuestion, Action onQuestionCompleted = null)
    {
        bool updating = true;
        float progress = 0f;
        if (this.answeredQuestion < totalQuestion)
        {
            progress = (float)this.answeredQuestion / totalQuestion;
            this.answeredQuestion += 1;
            updating = true;
        }
        else
        {
            progress = 1f;
            updating = false;
        }

        progress = Mathf.Clamp(progress, 0f, 1f);
        if (this.progressFillImage != null && this.progressiveBar != null)
        {
            this.progressFillImage.DOFillAmount(progress, 0.5f).OnComplete(() =>
            {
                if (progress >= 1f) onQuestionCompleted?.Invoke();
            });

            int percentage = (int)(progress * 100);
            this.progressiveBar.GetComponentInChildren<NumberCounter>().Value = percentage;
        }
        return updating;
    }

    public void setNewQuestion(QuestionList qa = null, int totalQuestion = 0, bool isLogined = false, Action onQuestionCompleted = null)
    {
        this.setProgressiveBar(isLogined);

        if (isLogined)
        {
            bool updating = this.updateProgressiveBar(totalQuestion, onQuestionCompleted);
            if (!updating)
            {
                return;
            }
        }

        if (qa == null) return;
        this.qa = qa;
        TextMeshProUGUI questionText = null;
        switch (qa.questionType)
        {
            case "picture":
                SetUI.SetGroup(this.questionBgs, 0, 0f);
                this.questionImage = this.questionBgs[0].GetComponentInChildren<RawImage>();
                this.questiontype = QuestionType.Picture;
                var qaImage = qa.texture;
                this.correctAnswer = qa.correctAnswer;
                this.answersChoics = qa.answers;
                this.correctAnswerId = this.answersChoics != null ? Array.IndexOf(this.answersChoics, this.correctAnswer) : 0;

                if (this.questionImage != null && qaImage != null)
                {
                    this.questionImage.enabled = true;
                    this.aspecRatioFitter = this.questionImage.GetComponent<AspectRatioFitter>();
                    this.questionImage.texture = qaImage;
                    var width = this.questionImage.GetComponent<RectTransform>().sizeDelta.x;
                    if (qaImage.width > qaImage.height)
                    {
                        this.questionImage.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 350f);
                    }
                    else
                    {
                        this.questionImage.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 430f);
                    }
                    this.aspecRatioFitter.aspectRatio = (float)qaImage.width / (float)qaImage.height;
                }
                break;
            case "audio":
                SetUI.SetGroup(this.questionBgs, 1, 0f);
                this.audioPlayBtn = this.questionBgs[1].GetComponentInChildren<CanvasGroup>();
                if (this.audioPlayBtn != null)
                {
                    SetUI.Set(this.audioPlayBtn, true, 0f);
                }
                this.questiontype = QuestionType.Audio;
                this.correctAnswer = qa.correctAnswer;
                this.answersChoics = qa.answers;
                this.correctAnswerId = this.answersChoics != null ? Array.IndexOf(this.answersChoics, this.correctAnswer) : 0;
                this.playAudio();
                break;
            case "text":
                SetUI.SetGroup(this.questionBgs, 2, 0f);
                this.questiontype = QuestionType.Text;
                questionText = this.questionBgs[2].GetComponentInChildren<TextMeshProUGUI>();
                if (questionText != null)
                {
                    questionText.text = qa.question;
                }
                this.correctAnswer = qa.correctAnswer;
                this.answersChoics = qa.answers;
                this.correctAnswerId = this.answersChoics != null ? Array.IndexOf(this.answersChoics, this.correctAnswer) : 0;
                break;
            case "fillInBlank":
                SetUI.SetGroup(this.questionBgs, 3, 0f);
                questionText = this.questionBgs[3].GetComponentInChildren<TextMeshProUGUI>();
                if(questionText != null)
                {
                    questionText.text = qa.question;
                }
                this.audioPlayBtn = this.questionBgs[3].GetComponentInChildren<CanvasGroup>();
                if(this.audioPlayBtn != null)
                {
                    SetUI.Set(this.audioPlayBtn, true, 0f);
                }
                this.questiontype = QuestionType.FillInBlank;
                this.correctAnswer = qa.correctAnswer;
                this.answersChoics = qa.answers;
                this.correctAnswerId = this.answersChoics != null ? Array.IndexOf(this.answersChoics, this.correctAnswer) : 0;
                this.playAudio();
                break;
        }

        if (LogController.Instance != null)
        {
            LogController.Instance.debug($"Get new {nameof(this.questiontype)} question");
        }

        if (this.numberQuestion < totalQuestion - 1)
            this.numberQuestion += 1;
        else
            this.numberQuestion = 0;
    }

    public void playAudio()
    {
        if(this.audioPlayBtn != null && this.qa.audioClip != null)
        {
            this.audioPlayBtn.GetComponentInChildren<AudioSource>().clip = this.qa.audioClip;
            this.audioPlayBtn.GetComponentInChildren<AudioSource>().Play();
        }
    }
}

public static class SortExtensions
{
    // Fisher-Yates shuffle algorithm
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1); // Use Unity's Random class
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public static void ShuffleArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, array.Length);
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }
}